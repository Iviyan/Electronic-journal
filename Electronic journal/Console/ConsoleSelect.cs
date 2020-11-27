using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using static Electronic_journal.Helper;

namespace Electronic_journal
{
    public class ConsoleSelect
    {
        private int startX;
        public int StartX
        {
            get => startX;
            set
            {
                startX = value;
            }
        }
        private int startY;
        public int StartY
        {
            get => startY;
            set
            {
                startY = value;
            }
        }
        private int maxWidth;
        public int MaxWidth
        {
            get => (maxWidth == 0) ? Console.WindowWidth : maxWidth;
            set
            {
                if (value > 2 || value == 0)
                    maxWidth = value;
                else
                    throw new Exception("The max width must be >2 or 0");
            }
        }
        private int maxHeight;
        public int MaxHeight
        {
            get => (maxHeight == 0) ? Console.WindowHeight : maxHeight;
            set
            {
                if (value >= 0)
                    maxHeight = value;
                else
                    throw new Exception("The max width must be >0 or 0");
            }
        }
        public int CurrentMaxWidth { get; private set; }
        public int CurrentHeight { get; private set; } = 0;
        public int ContentHeight { get; private set; }
        public ObservableCollection<string> Choices { get; private set; }
        private HashSet<int> Disabled;
        private int interval;
        public int Interval
        {
            get => interval;
            set
            {
                interval = value;
            }
        }

        private int page;
        public int Page
        {
            get => page;
            set
            {
                if (value < 0 || value > PageCount) return;

                Clear();
                var p = Pages[value];
                page = value;
                CalculateCurrentPage();
                Write();

                selectedIndex = p.start;
                Select(p.start);
            }
        }
        private int selectedIndex;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (value < 0 || value > Choices.Count) return;
                Select(selectedIndex, ' ', ' ');

                if (value < Pages[Page].start)
                {
                    Clear();
                    page = GetPageIndex(value);
                    CalculateCurrentPage();
                    Write();
                }
                if (value > Pages[Page].end)
                {
                    Clear();
                    page = GetPageIndex(value);
                    CalculateCurrentPage();
                    Write();
                }
                selectedIndex = value;
                Select(selectedIndex);
            }
        }

        (int top, int width, int height)[] selHelper;
        (int start, int end)[] Pages;

        public int PageCount => Pages.Length;


        public ConsoleSelect(string[] choices, int selectedIndex = 0, int interval = 0, int startX = 0, int startY = 0, int maxWidth = 0, int maxHeight = 0, int[] disabled = null, bool write = true)
        {
            Choices = new ObservableCollection<string>(choices);
            Choices.CollectionChanged += Choices_CollectionChanged;

            this.selectedIndex = selectedIndex;
            this.interval = interval;
            this.startX = startX;
            this.startY = startY;
            this.maxWidth = maxWidth;
            this.maxHeight = maxHeight;

            if (disabled != null)
                Disabled = new(disabled);
            else
                Disabled = new();

            CurrentMaxWidth = 0;

            CalculatePages();
            CalculateCurrentPage();
            if (write) Write();
        }

        public void Update(string[] choices, bool clear = true)
        {
            if (clear) Clear();
            selectedIndex = 0;
            Pages = null;
            page = 0;
            Choices = new ObservableCollection<string>(choices);
            Choices.CollectionChanged += Choices_CollectionChanged;

            CalculatePages();
            CalculateCurrentPage();
            Write();
        }

        private void Choices_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Replace:
                    //Edit(e.NewStartingIndex, e.NewItems[0] as string);
                    string newText = e.NewItems[0] as string;

                    int pageInd = GetPageIndex(e.NewStartingIndex);
                    if (pageInd != Page)
                    {
                        if (pageInd > Page)
                        {
                            CalculatePages(e.NewStartingIndex);
                            return;
                        }
                        else
                        {
                            var page_ = Pages[Page];
                            CalculatePages(e.NewStartingIndex);
                            if (page_ == Pages[Page]) return;
                            else
                            {
                                CalculateCurrentPage(page_.start);
                                Write();
                                return;
                            }
                        }
                    }

                    var sh_ = selHelper[e.NewStartingIndex];
                    int lineHeght = GetLineHeight(e.NewStartingIndex);
                    if (sh_.height == lineHeght)
                    {
                        CalculateCurrentPage(e.NewStartingIndex);
                        var sh = selHelper[e.NewStartingIndex];
                        ConsoleHelper.ClearArea(StartX + 1, StartY + sh.top, StartX + sh_.width + 2, StartY + sh.top + sh.height - 1);
                        WriteLine(newText, sh.top);
                        if (CurrentMaxWidth < sh.width) CurrentMaxWidth = sh.width;
                    }
                    else
                    {
                        if (SelectedIndex == e.NewStartingIndex) Select(e.NewStartingIndex, ' ', ' ');
                        CalculatePages(e.NewStartingIndex);
                        CalculateCurrentPage(e.NewStartingIndex);
                        Write(e.NewStartingIndex);
                    }

                    if (SelectedIndex == e.NewStartingIndex) Select(e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        Clear();
                        CalculatePages();
                        if (Page > Pages.Length - 1) page = Pages.Length - 1;
                        CalculateCurrentPage();
                        Write();
                        if (selectedIndex > Pages[Page].end)
                            selectedIndex = Pages[Page].end;
                        Select(selectedIndex);
                        return; // TODO: доделать
                        //Helper.mb(Helper.ArrayToStr(Pages), '\n', Helper.ArrayToStr(selHelper), '\n', e.OldStartingIndex);
                       /* int startIndex = e.OldStartingIndex - (e.OldStartingIndex == Choices.Count ? 1 : 0);
                        if (GetPageIndex(e.OldStartingIndex) == Page)
                        {
                            var page_ = Pages[Page];
                            if (Page == 1 && page_.end - page_.start == 1)
                            {
                                CalculatePages();
                                if (Pages.Length > 1)
                                {
                                    Clear(e.OldStartingIndex, Pages[Page].end);
                                    CalculateCurrentPage();
                                    selectedIndex--;
                                    Select(selectedIndex);
                                }
                                else
                                {
                                    Clear();
                                    page = 0;
                                    CalculateCurrentPage();
                                    Write();
                                    selectedIndex = Pages[0].end;
                                    Select(selectedIndex);
                                }
                            }
                            else
                            if (page_.end - page_.start + 1 == 1)
                            {
                                Clear();
                                if (Page == 0)
                                {
                                    CalculatePages();
                                    CalculateCurrentPage();
                                }
                                else
                                {
                                    CalculatePages(Page == 1 ? 0 : Pages[Page - 1].start);
                                    page--;
                                    CalculateCurrentPage();
                                    Write();
                                    selectedIndex--;
                                    Select(SelectedIndex);
                                }
                            }
                            else
                            {
                                Clear(e.OldStartingIndex, Pages[Page].end);
                                CalculatePages(startIndex);
                                if (Pages.Length > 1)
                                {
                                    CalculateCurrentPage(startIndex);
                                    Write(e.OldStartingIndex);
                                }
                                else
                                {
                                    CalculateCurrentPage(0);
                                    Write();
                                }
                                if (e.OldStartingIndex == Choices.Count)
                                {
                                    selectedIndex--;
                                    Select(SelectedIndex);
                                }
                                else
                                    Select(SelectedIndex);
                            }
                        }
                        else
                            CalculatePages(startIndex);*/
                    }
                    break;
                case NotifyCollectionChangedAction.Add:
                    {
                        // Helper.mb(Helper.ArrayToStr(selHelper), '\n', Helper.ArrayToStr(Pages), '\n', e.NewStartingIndex);
                        Clear();
                        CalculatePages();
                        CalculateCurrentPage();
                        Write();
                        if (selectedIndex > Pages[Page].end)
                            selectedIndex = Pages[Page].end;
                        Select(selectedIndex);
                        return; // TODO: доделать

                        /* if (Pages.Length == 0)
                         {
                             CalculatePages();
                             CalculateCurrentPage();
                             Write();
                             Select(0);
                         }
                         else if (e.NewStartingIndex == Choices.Count - 1) // TODO: -----
                         {
                             CalculatePages(Pages[Page].start);//e.NewStartingIndex == 0 ? 0 : e.NewStartingIndex - 1);
                             if (Page == 0 && e.NewStartingIndex > Pages[0].end)
                             {
                                 if (selectedIndex == Pages[0].end)
                                 {
                                     Clear();
                                     page++;
                                     CalculateCurrentPage();
                                     Write();
                                     Select(SelectedIndex);
                                 }
                                 else
                                 {
                                     Clear();
                                     CalculateCurrentPage();
                                     Write();
                                     //selectedIndex--;
                                     Select(selectedIndex);
                                 }
                             }
                             else
                             if (GetPageIndex(e.NewStartingIndex) == Page)
                             {
                                 CalculateCurrentPage(e.NewStartingIndex);
                                 Write(e.NewStartingIndex);
                             }
                             else
                             {
                                 UpdatePageView();
                             }
                         }
                         else
                         if (GetPageIndex(e.NewStartingIndex) <= Page)
                         {
                             if (GetPageIndex(e.NewStartingIndex) < Page)
                             {
                                 Clear(Pages[Page].start, Pages[Page].end);
                                 CalculatePages(e.NewStartingIndex);
                                 CalculateCurrentPage(e.NewStartingIndex);
                                 Write(e.NewStartingIndex);
                                 SelectedIndex++;
                             }
                             else
                             {
                                 if (Pages[Page].end < e.NewStartingIndex + 1)
                                 {
                                     CalculatePages(e.NewStartingIndex);
                                     page++;
                                     Clear();
                                     CalculateCurrentPage(e.NewStartingIndex + 1);
                                     Write();
                                     selectedIndex++;
                                     Select(selectedIndex);
                                 }
                                 else
                                 {
                                     Clear(e.NewStartingIndex, Pages[Page].end - 1);
                                     CalculatePages(e.NewStartingIndex);
                                     CalculateCurrentPage(e.NewStartingIndex);
                                     Write(e.NewStartingIndex);
                                     SelectedIndex++;
                                 }
                             }
                         }
                         else
                             CalculatePages(e.NewStartingIndex);*/
                        // Helper.mb(Helper.ArrayToStr(selHelper), '\n', Helper.ArrayToStr(Pages), '\n', e.NewStartingIndex);
                    }
                    break;
            }
        }

        int GetLineHeight(int length, int maxLength) => (int)Math.Ceiling(length / (double)maxLength);
        int GetLineHeight(int index) => GetLineHeight(Choices[index].Length, MaxWidth - 2);
        int GetPageIndex(int choiceIndex)
        {
            for (int i = 0; i < PageCount; i++)
                if (choiceIndex >= Pages[i].start && choiceIndex <= Pages[i].end)
                    return i;
            return -1;
        }

        void CalculateCurrentPage(int startIndex = 0)
        {
            if (Choices.Count == 0)
            {
                selHelper = new (int, int, int)[0];
                return;
            }
            //Helper.mb(Helper.ArrayToStr(Pages));
            var page = Pages[Page];

            if (startIndex < 0 || startIndex != 0 && (startIndex < page.start || startIndex > page.end)) throw new ArgumentOutOfRangeException();

            /*if (startIndex > 0 && page.end - page.start + 1 == selHelper.Length)
            {
                //mb();
                int lineHeight = GetLineHeight(startIndex);
                if (selHelper[startIndex].height == lineHeight)
                {
                    selHelper[startIndex] = (
                        selHelper[startIndex].top,
                        lineHeight > 1 ? MaxWidth - 2 : Choices[startIndex].Length,
                        lineHeight);
                    return;
                }
            }*/

            int line = PageCount > 1 ? 1 : 0;
            //if (startIndex > 0 && selHelper != null && startIndex - page.start < selHelper.Length) line = selHelper[startIndex - page.start].top;
            if (startIndex > 0 && selHelper != null)
            {
                if (startIndex - page.start < selHelper.Length)
                    line = selHelper[startIndex - page.start].top;
                else if (startIndex - page.start == selHelper.Length)
                {
                    var sh = selHelper[startIndex - page.start - 1];
                    line = sh.top + sh.height + interval;
                }
            }

            if (selHelper != null)
            {
                if (page.end - page.start + 1 != selHelper.Length)
                    Array.Resize(ref selHelper, page.end - page.start + 1);
            }
            else
                selHelper = new (int top, int width, int height)[page.end - page.start + 1];


            if (startIndex == 0) startIndex = page.start;
            for (int i = startIndex; i <= page.end; i++)
            {
                int lineHeight = GetLineHeight(i);
                int i_ = i - page.start;
                selHelper[i_] =
                    (
                        line,
                        lineHeight > 1 ? MaxWidth - 2 : Choices[i].Length,
                        lineHeight
                    );
                line += lineHeight + interval;
            }
            ContentHeight = line - interval;
            //Helper.mb(Helper.ArrayToStr(selHelper));
        }
        void CalculatePages(int startIndex = 0)
        {
            if (Choices.Count == 0)
            {
                selHelper = new (int, int, int)[0];
                Pages = new (int, int)[0];
                return;
            }
            if (startIndex < 0 || Pages != null && startIndex >= Choices.Count) throw new ArgumentOutOfRangeException();

            bool first = Pages == null;
            List<(int start, int end)> pages = new List<(int start, int end)>();

            bool multiplePages = !first && PageCount > 1 && startIndex > Pages[0].end;

            int line = multiplePages ? 1 : 0,
                firstPageIndex = 0,
                maxHeight = MaxHeight;
            if (startIndex > 0)
            {
                int pageInd = GetPageIndex(startIndex);
                if (pageInd > 0) firstPageIndex = Pages[pageInd].start;

                if (pageInd != Page) startIndex = Pages[pageInd].start;
                else if (multiplePages) line = selHelper[startIndex - Pages[pageInd].start].top;

                pages.AddRange(Pages.Take(pageInd));
            }
            if (firstPageIndex == startIndex && GetLineHeight(startIndex) > MaxHeight) throw new Exception("The string is too large");

            for (int i = startIndex; i < Choices.Count; i++)
            {
                int lineHeight = GetLineHeight(i);
                int lineWidth = lineHeight > 1 ? MaxWidth - 2 : Choices[i].Length;
                if (CurrentMaxWidth < lineWidth) CurrentMaxWidth = lineWidth;

                if (line + lineHeight >= maxHeight/* - (firstPageIndex > 0 ? 1 : 0)*/)
                {
                    var page = (firstPageIndex, i);
                    if (!multiplePages) //////////
                    {
                        if (line + lineHeight == maxHeight && i + 1 == Choices.Count)
                        {
                            Pages = new (int start, int end)[] { (0, Choices.Count - 1) };
                            return;
                        }

                        //if (line - interval >= maxHeight) // TODO: ???
                        //{
                        //    page = (firstPageIndex, i - 2);
                        //    i--;
                        //} else

                        page = (firstPageIndex, i - 1);
                        firstPageIndex = i;
                        line = 1 + lineHeight + interval;
                        multiplePages = true;
                    }
                    else
                    {
                        if (line + lineHeight == maxHeight)
                        {
                            //page = (firstPageIndex, i);
                            firstPageIndex = i + 1;
                            line = 1;
                        }
                        else
                        {
                            page = (firstPageIndex, i - 1);
                            firstPageIndex = i;
                            line = 1 + lineHeight + interval;
                        }
                    }
                    pages.Add(page);

                    if (!first && page == Pages[pages.Count - 1] && Pages[Pages.Length - 1].end == Choices.Count - 1)
                    {
                        Pages = pages.ToArray();
                        return;
                    }
                    //line = 1;
                    if (i + 1 < Choices.Count && GetLineHeight(i + 1) >= MaxHeight) throw new Exception("The string is too large");
                }
                else
                    line += lineHeight + interval;
            }
            if (pages.Count == 0)
            {
                Pages = new (int start, int end)[] { (0, Choices.Count - 1) };
                return;
            }
            if (line > (multiplePages ? 1 : 0))
                pages.Add((firstPageIndex, Choices.Count - 1));

            Pages = pages.ToArray();
        }

        int WriteLine(string text, int top)
        {
            string[] lines = Helper.Split(text, MaxWidth - 2);

            for (int i = 0; i < lines.Length; i++)
            {
                Console.SetCursorPosition(StartX + 1, StartY + top + i);
                bool disable = Disabled.Contains(i);
                if (disable) Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(lines[i]);
                if (disable) Console.ForegroundColor = ConsoleColor.White;
            }
            return lines.Length;
        }

        int pageViewLength = 0;
        public void ClearPageView()
        {
            using (new CursonPosition(StartX + 1, StartY))
                Console.Write(new string(' ', pageViewLength));
        }
        public void UpdatePageView()
        {
            using (new CursonPosition(StartX + 1, StartY))
            {
                ConsoleHelper.WriteCenter($"{Page + 1}/{PageCount}", StartX + 1, CurrentMaxWidth, '-', '-');
                if (Console.CursorLeft - startX - 1 < pageViewLength)
                    Console.Write(new string(' ', Console.CursorLeft - startX - 1 - pageViewLength));
                pageViewLength = Console.CursorLeft - startX - 1;
            }
        }
        public void Write(int startIndex = 0)
        {
            //Helper.mb($"=> {PageCount}");
            if (startIndex >= Choices.Count) return;

            if (PageCount > 1)
                UpdatePageView();
            else if (pageViewLength > 0)
                ClearPageView();

            var page = Pages[Page];

            startIndex = startIndex != 0 ? startIndex : page.start;
            int line = selHelper[startIndex - page.start].top;
            for (int i = startIndex; i <= page.end; i++)
            {
                line += interval + WriteLine(Choices[i], line);
            }
            CurrentHeight = ContentHeight;
        }
        public void Clear()
        {
            if (CurrentHeight == 0) return;
            ConsoleHelper.ClearArea(StartX, StartY, StartX + CurrentMaxWidth + 1, StartY + CurrentHeight);
            Console.SetCursorPosition(0, StartY);
            CurrentHeight = 0;
        }
        public void Clear(int startIndex, int endIndex)
        {
            var page = Pages[GetPageIndex(startIndex)];
            var sh1 = selHelper[startIndex - page.start];
            var sh2 = selHelper[endIndex - page.start];
            ConsoleHelper.ClearArea(StartX, StartY + sh1.top, StartX + CurrentMaxWidth + 1, StartY + sh2.top + sh2.height);
        }

        /*void Edit(int index, string newText) // TODO: перенести в on...
        {
            if (newText == Choices[index]) return;
            if (GetPageIndex(index) != Page)
            {
                CalculatePages(index);
                return;
            }

            var sh_ = selHelper[index];
            Choices[index] = newText;

            if (PageCount == 1) CalculateCurrentPage(index);
            else CalculatePages(index);

            var sh = selHelper[index];

            if (sh_.height == sh.height)
            {
                ConsoleHelper.ClearArea(StartX + 1, StartY + sh.top, StartX + sh_.width - 1, StartY + sh.top + sh.height - 1);
                WriteLine(newText, sh.top);
                return;
            }
            else
                Write(index);


            if (SelectedIndex == index) Select(index);
        }*/

        /*public void Remove(int index)
        {
            if (PageCount == 1) CalculateCurrentPage(index);
            else CalculatePages(index);
        }*/

        public void Select(int sel, char cStart = '>', char cEnd = '<')
        {
            if (Choices.Count == 0) return;
            var sh = selHelper[sel - Pages[Page].start]; //Helper.mb(sh);
            for (int i = 0; i < sh.height; i++)
            {
                Console.SetCursorPosition(StartX, StartY + sh.top + i);
                Console.Write(cStart);
                Console.SetCursorPosition(StartX + sh.width + 1, StartY + sh.top + i);
                Console.Write(cEnd);

            }
        }

        public void Disable(int index)
        {
            if (!Disabled.Contains(index))
            {
                Disabled.Add(index);
                WriteLine(Choices[index], selHelper[index].top);
            }
        }
        public void Enable(int index)
        {
            if (Disabled.Contains(index))
            {
                Disabled.Remove(index);
                WriteLine(Choices[index], selHelper[index].top);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>null -> continue, int -> return int</returns>
        public delegate int? PressKey(ConsoleKeyInfo key, int selectedIndex);
        public int Choice(PressKey onPressKey, bool rewrite = false) => Choice(0, onPressKey, rewrite);
        public int Choice(int selectIndex = 0, PressKey onPressKey = null, bool rewrite = false)
        {
            if (Choices.Count == 0) { Console.ReadKey(true); return -1; }//throw new Exception("The list of choices is empty");
            if (rewrite) Write();

            SelectedIndex = selectIndex;
            bool onPressKeyEventExists = onPressKey != null;

            if (Choices.Count == 0) throw new Exception("Count of choices must be >0");

            ConsoleKeyInfo info;
            while (true)
            {
                info = Console.ReadKey(true);
                switch (info.Key)
                {
                    case ConsoleKey.DownArrow:
                        if (SelectedIndex + 1 < Choices.Count) SelectedIndex++;
                        else SelectedIndex = 0;
                        break;
                    case ConsoleKey.UpArrow:
                        if (SelectedIndex > 0) SelectedIndex--;
                        else SelectedIndex = Choices.Count - 1;
                        break;
                    case ConsoleKey.RightArrow:
                        if (Page + 1 < PageCount) Page++;
                        else Page = 0;
                        break;
                    case ConsoleKey.LeftArrow:
                        if (Page > 0) Page--;
                        else Page = PageCount - 1;
                        break;
                    case ConsoleKey.Enter:
                        if (!Disabled.Contains(SelectedIndex)) return SelectedIndex;
                        break;
                    default:
                        if (onPressKeyEventExists)
                        {
                            int? r = onPressKey(info, selectedIndex);
                            if (r != null) return (int)r;
                        }
                        break;
                }
            }
        }
    }
}