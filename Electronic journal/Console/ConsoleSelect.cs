using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

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
        public int CurrentHeight { get; private set; }
        public ObservableCollection<string> Choices { get; private set; }
        private HashSet<int> disabled = new HashSet<int>();
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
                select(p.start);
            }
        }
        private int selectedIndex;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (value < 0 || value > Choices.Count) return;
                select(selectedIndex, ' ', ' ');

                if (value < Pages[Page].start)
                {
                    Clear();
                    Page = GetPageIndex(value);
                    CalculateCurrentPage();
                    Write();
                }
                if (value > Pages[Page].end)
                {
                    Clear();
                    Page = GetPageIndex(value);
                    CalculateCurrentPage();
                    Write();
                }
                selectedIndex = value;
                select(selectedIndex);
            }
        }

        (int top, int width, int height)[] selHelper;
        (int start, int end)[] Pages;

        public int PageCount => Pages.Length;


        public ConsoleSelect(string[] choices, int selectedIndex = 0, int interval = 0, int startX = 0, int startY = 0, int maxWidth = 0, int maxHeight = 0)
        {
            Choices = new ObservableCollection<string>(choices);
            Choices.CollectionChanged += Choices_CollectionChanged;

            this.selectedIndex = selectedIndex;
            this.interval = interval;
            this.startX = startX;
            this.startY = startY;
            this.maxWidth = maxWidth;
            this.maxHeight = maxHeight;

            CurrentMaxWidth = 0;

            CalculatePages();
            CalculateCurrentPage();
            Write();
        }

        public void Update(string[] choices)
        {
            Clear();
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
                    Edit(e.NewStartingIndex, e.NewItems[0] as string);
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

            if (startIndex > 0)
            {
                int lineHeight = GetLineHeight(startIndex);
                if (selHelper[startIndex].height == lineHeight)
                {
                    selHelper[startIndex] = (
                        selHelper[startIndex].top,
                        lineHeight > 1 ? MaxWidth - 2 : Choices[startIndex].Length,
                        lineHeight);
                    return;
                }
            }

            selHelper = new (int top, int width, int height)[page.end - page.start + 1];

            if (startIndex == 0) startIndex = page.start;

            int line = PageCount > 1 ? 1 : 0;
            for (int i = startIndex; i <= page.end; i++)
            {
                int lineHeight = GetLineHeight(i);
                int i_ = i - startIndex;
                selHelper[i_] =
                    (
                        line,
                        lineHeight > 1 ? MaxWidth - 2 : Choices[i].Length,
                        lineHeight
                    );
                line += lineHeight + interval;
            }
            CurrentHeight = line - interval;
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

            bool multiplePages = !first && PageCount > 1;

            int line = multiplePages ? 1 : 0,
                firstPageIndex = 0,
                maxHeight = MaxHeight;
            if (startIndex > 0)
            {
                int pageInd = GetPageIndex(startIndex);
                if (pageInd > 0) firstPageIndex = Pages[pageInd].start;

                if (pageInd != Page) startIndex = Pages[pageInd].start;
                else line = selHelper[startIndex - Pages[pageInd].start].top;
            }
            if (firstPageIndex == startIndex && GetLineHeight(startIndex) > MaxHeight) throw new Exception("The string is too large");

            for (int i = startIndex; i < Choices.Count; i++)
            {
                int lineHeight = GetLineHeight(i);
                int lineWidth = lineHeight > 1 ? MaxWidth - 2 : Choices[i].Length;
                if (CurrentMaxWidth < lineWidth) CurrentMaxWidth = lineWidth;

                if (line + lineHeight >= maxHeight)
                {
                    var page = (firstPageIndex, i - 1);
                    if (!multiplePages) //////////
                    {
                        if (line + lineHeight == maxHeight && i + 1 == Choices.Count)
                        {
                            Pages = new (int start, int end)[] { (0, Choices.Count - 1) };
                            return;
                        }

                        if (line - interval >= maxHeight)
                        {
                            page = (firstPageIndex, i - 2);
                            i--;
                        }
                        multiplePages = true;
                    }
                    pages.Add(page);
                    firstPageIndex = i;
                    if (!first && page == Pages[pages.Count - 1])
                    {
                        Pages = pages.ToArray();
                        return;
                    }
                    line = 1;
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
            if (line > 0)
                pages.Add((firstPageIndex, Choices.Count - 1));
            
            Pages = pages.ToArray();
        }

        int WriteLine(string text, int top)
        {
            string[] lines = Helper.Split(text, MaxWidth - 2);

            for (int i = 0; i < lines.Length; i++)
            {
                Console.SetCursorPosition(StartX + 1, StartY + top + i);
                bool disable = disabled.Contains(i);
                if (disable) Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(lines[i]);
                if (disable) Console.ForegroundColor = ConsoleColor.White;
            }
            return lines.Length;
        }

        public void Write(int startIndex = 0)
        {
            //Helper.mb($"=> {PageCount}");
            if (startIndex >= Choices.Count) return;

            if (PageCount > 1)
            {
                Console.SetCursorPosition(StartX + 1, StartY);
                ConsoleHelper.WriteCenter($"{Page + 1}/{PageCount}", StartX + 1, CurrentMaxWidth, '-', '-');
            }
            int line = selHelper[startIndex].top;
            var page = Pages[Page];

            for (int i = startIndex != 0 ? startIndex : page.start; i <= page.end; i++)
            {
                line += interval + WriteLine(Choices[i], line);
            }
        }
        public void Clear()
        {
            ConsoleHelper.ClearArea(StartX, StartY, StartX + CurrentMaxWidth + 1, StartY + CurrentHeight);
        }
        public void Clear(int startIndex, int endIndex)
        {
            var sh1 = selHelper[startIndex];
            var sh2 = selHelper[endIndex];
            ConsoleHelper.ClearArea(StartX, StartY + sh1.top, StartX + CurrentMaxWidth + 1, StartY + sh2.top + sh2.height - 1);
        }

        public void Edit(int index, string newText)
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


            if (SelectedIndex == index) select(index);
        }



        public void select(int sel, char cStart = '>', char cEnd = '<')
        {
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
            if (!disabled.Contains(index))
            {
                disabled.Add(index);
                WriteLine(Choices[index], selHelper[index].top);
            }
        }
        public void Enable(int index)
        {
            if (disabled.Contains(index))
            {
                disabled.Remove(index);
                WriteLine(Choices[index], selHelper[index].top);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="selectedIndex"></param>
        /// <returns>True -> abort the selection</returns>
        public delegate bool PressKey(ConsoleKeyInfo key, int selectedIndex);
        public int Choice(PressKey onPressKey) => Choice(0, onPressKey);
        public int Choice(int selectIndex = 0, PressKey onPressKey = null)
        {
            int count = Choices.Count;
            if (count == 0) { Console.ReadKey(true); return -1; }//throw new Exception("The list of choices is empty");

            SelectedIndex = selectIndex;
            bool onPressKeyEventExists = onPressKey != null;

            if (count == 0) throw new Exception("Count of choices must be >0");

            ConsoleKeyInfo info;
            while (true)
            {
                info = Console.ReadKey(true);
                switch (info.Key)
                {
                    case ConsoleKey.DownArrow:
                        if (SelectedIndex + 1 < count) SelectedIndex++;
                        else SelectedIndex = 0;
                        break;
                    case ConsoleKey.UpArrow:
                        if (SelectedIndex > 0) SelectedIndex--;
                        else SelectedIndex = count - 1;
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
                        if (!disabled.Contains(SelectedIndex)) return SelectedIndex;
                        break;
                    default:
                        if (onPressKeyEventExists && onPressKey(info, selectedIndex)) return -1;
                        break;
                }
            }
        }
    }
}