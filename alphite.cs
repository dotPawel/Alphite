using System;

namespace alphite
{
    public class alphite
    {
        static bool startExit = false;
        static int cursorRow = 0;
        static int cursorCol = 0;
        static string[] lines = 
        { 
            "   Welcome to Alphite, version 1.0",
            " A simple CLI text editor for UniCMD",
            "",
            "If you see this you most likely haven't loaded",
            "in a file, to load one:",
            "   * Press ESC to exit",
            "   * Load the directory where your file is",
            "   * Start Alphite with your file using:",
            "     '.$alphite /in {file.extension}'",
            "",
            " Thank you for using Alphite, made by .pwl",
        };
        public static string Version = "1.0";
        static int statusBarHeight = 1;
        static int bottomBarHeight = 1;
        public static string filePath;
        public static int saveCount = 0;
        static string bottomBarText = "Press Escape to exit | INSERT to save";
        static string lastActionText = "";
        public static void Main(string[] args){ // exe handling
            //MainLoop(@"D:\VS Projects\alphite\alphite.cs");
            MainLoop(@"");
        }
        public static bool MainLoop(string file){ // Called from UniCMD
            try{
                if (File.Exists(file)){
                    filePath = file;
                    bottomBarText = "Press Escape to exit | INSERT to save";
                    string[] filelines = File.ReadAllLines(file);
                    if (filelines.Length > 0){
                        lines = filelines;
                    } else{
                        lines = new string[] { "" };
                    }
                } else{
                    bottomBarText = "Press Escape to exit | NO FILE LOADED";
                }

                Console.Clear();
                Console.BackgroundColor = ConsoleColor.Magenta;
                FilledWriteLine(" Alphite, version " + Version);
                Console.BackgroundColor = ConsoleColor.Black;

                PrintStatusBar();
                PrintLines();
                PrintBottomBar();
            
                while (!startExit)
                {
                    int cursorTop = cursorRow + statusBarHeight;
                    if (cursorTop >= Console.WindowHeight)
                    {
                        cursorTop = Console.WindowHeight - 1;
                    }

                    Console.SetCursorPosition(cursorCol, cursorTop);

                    var key = Console.ReadKey(true);
                    HandleKey(key);
                }
            } catch (Exception ex) {
                Console.Clear();
                Console.ResetColor();
                Console.WriteLine("  Alphite crashed!");
                Console.WriteLine(" - - - - - - - - - - - - ");
                Console.WriteLine(" Crashdump data:");
                Console.WriteLine("  exception / " + ex.Message);
                Console.WriteLine("  startExit / " + startExit);
                Console.WriteLine("  cursorRow / " + cursorRow);
                Console.WriteLine("  cursorCol / " + cursorCol);
                Console.WriteLine("  filePath / " + filePath);
                Console.WriteLine("  saveCount / " + saveCount);
                Console.Write("  lines / [ ");
                foreach (string line in lines){
                    Console.Write(line + " ");
                }
                Console.Write("]\n");
                Console.WriteLine(" - - - - - - - - - - - - ");
                Console.WriteLine("[Press any key to exit]");
                Console.ReadKey();
            }
            ResetAlphite();
            return true;
        }

        static void HandleKey(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Insert:
                    if (File.Exists(filePath)){
                                File.WriteAllText(filePath, "");
                                foreach (string line in lines){
                                using (StreamWriter sw = File.AppendText(filePath))
                                {
                                    sw.WriteLine(line);
                                    sw.Dispose();
                                }
                            }
                            saveCount += 1;
                            lastActionText = "// Saved changes (" + saveCount + ")";
                            PrintStatusBar();
                        }
                    break;
                case ConsoleKey.LeftArrow:
                    if (cursorCol > 0)
                        cursorCol--;
                    break;
                case ConsoleKey.RightArrow:
                    if (cursorCol < lines[cursorRow].Length && cursorCol < Console.WindowWidth - 1)
                        cursorCol++;
                        break;
                case ConsoleKey.DownArrow:
                    if (cursorRow < lines.Length - 1)
                    {
                        if (cursorRow < Console.WindowHeight - statusBarHeight - bottomBarHeight - 2)
                        {
                            cursorRow++;
                            cursorCol = Math.Min(cursorCol, lines[cursorRow].Length);
                        }
                        else
                        {
                            ScrollLines(-1);
                        }
                    }
                    break;
                case ConsoleKey.UpArrow:
                    if (cursorRow > 0)
                    {
                        if (cursorRow > 0)
                        {
                            cursorRow--;
                            cursorCol = Math.Min(cursorCol, lines[cursorRow].Length);
                        }
                        else
                        {
                            ScrollLines(1);
                        }
                    }
                    break;
                case ConsoleKey.Backspace:
                    if (cursorCol == 0 && cursorRow > 0 && lines[cursorRow].Length == 0)
                    {
                        DeleteLine(cursorRow);
                        cursorRow--;
                        cursorCol = lines[cursorRow].Length;
                    }
                    else if (cursorCol > 0)
                    {
                        lines[cursorRow] = lines[cursorRow].Remove(cursorCol - 1, 1);
                        cursorCol--;
                    }
                    break;
                case ConsoleKey.Enter:
                    var left = lines[cursorRow].Substring(0, cursorCol);
                    var right = lines[cursorRow].Substring(cursorCol);
                    lines[cursorRow] = left;
                    Array.Resize(ref lines, lines.Length + 1);
                    Array.Copy(lines, cursorRow + 1, lines, cursorRow + 2, lines.Length - cursorRow - 2);
                    lines[cursorRow + 1] = right;
                    cursorRow++;
                    cursorCol = 0;
                    break;
                case ConsoleKey.Escape:
                    startExit = true;
                    break;
                default:
                    if (cursorCol < Console.WindowWidth - 1)
                    {
                        lines[cursorRow] = lines[cursorRow].Insert(cursorCol, key.KeyChar.ToString());
                        cursorCol++;
                    }
                    break;
            }
            
            PrintStatusBar();
            PrintLines();
            PrintBottomBar();
        }

        static void PrintStatusBar()
        {
            // https://cdn.discordapp.com/attachments/826861992345993227/1212081734200459364/video0-28-1.mp4?ex=65f08a14&is=65de1514&hm=ffdf258a97d0e12c9ebbcdcfffae320975cec3cf7e8b2ecd93407334f472f8b1&
            Console.BackgroundColor = ConsoleColor.Magenta;
            Console.SetCursorPosition(0, 0);
            FilledWriteLine(" Alphite, version " + Version + " " + lastActionText);
            Console.BackgroundColor = ConsoleColor.Black;
        }

        static void PrintLines()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            int availableHeight = Console.WindowHeight - statusBarHeight - bottomBarHeight;
            int startLine = Math.Max(0, cursorRow - availableHeight + 1);
            int endLine = Math.Min(lines.Length - 1, startLine + availableHeight - 1);

            for (int i = startLine; i <= endLine; i++)
            {
                Console.SetCursorPosition(0, i - startLine + statusBarHeight);
                if (i < lines.Length)
                    Console.Write(lines[i].PadRight(Console.WindowWidth));
            }
        }

        static void PrintBottomBar()
        {
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.SetCursorPosition(0, Console.WindowHeight - bottomBarHeight);
            FilledWrite(bottomBarText); // Corrected method call
        }

        static void DeleteLine(int index)
        {
            for (int i = index; i < lines.Length - 1; i++)
            {
                lines[i] = lines[i + 1];
            }
            Array.Resize(ref lines, lines.Length - 1);
        }
        static void ScrollLines(int direction)
        {
            int startIndex = Math.Max(0, cursorRow - (Console.WindowHeight - statusBarHeight - bottomBarHeight - 1));
            int endIndex = Math.Min(lines.Length - 1, startIndex + (Console.WindowHeight - statusBarHeight - bottomBarHeight - 1));

            if (startIndex + direction >= 0 && endIndex + direction < lines.Length)
            {
                startIndex += direction;
                endIndex += direction;
            }

            Console.MoveBufferArea(0, statusBarHeight + 1, Console.WindowWidth, endIndex - startIndex + 1, 0, statusBarHeight);

            cursorRow -= direction;
        }
        public static void FilledWriteLine(string text)
        {
            Console.Write(text);
            int width = Console.WindowWidth;
            int currentCursorPosition = Console.CursorLeft;
            int remainingWidth = width - currentCursorPosition;
            string space = new string(' ', remainingWidth);
            Console.Write(space + "\n");
        }
        public static void FilledWrite(string text)
        {
            Console.Write(text);
            int width = Console.WindowWidth;
            int currentCursorPosition = Console.CursorLeft;
            int remainingWidth = width - currentCursorPosition;
            string space = new string(' ', remainingWidth);
            Console.Write(space);
        }
        private static void ResetAlphite()
        {
            startExit = false;
            cursorRow = 0;
            cursorCol = 0;
            Array.Clear(lines);
            lines = new string[] { "" };
            filePath = null;
            saveCount = 0;
            bottomBarText = "Press Escape to exit | NO FILE LOADED";
            lastActionText = "";
            Console.ResetColor();
            Console.Clear();
        }
    }
}
