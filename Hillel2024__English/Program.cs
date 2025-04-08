using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    class Column
    {
        public int X;
        public int Y;
        public int Length;
        public int Speed;
        public int FrameCount;
    }

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        AnsiConsole.Clear();

        var random = new Random();
        int width = Console.WindowWidth;
        int height = Console.WindowHeight;

        var columns = InitializeColumns(width, height, random);

        while(true)
        {
            if(Console.WindowWidth != width || Console.WindowHeight != height)
            {
                width = Console.WindowWidth;
                height = Console.WindowHeight;
                AnsiConsole.Clear();
                columns = InitializeColumns(width, height, random);
            }

            AnsiConsole.Cursor.Hide();

            foreach(var column in columns)
            {
                column.FrameCount++;
                if(column.FrameCount < column.Speed)
                    continue;

                column.FrameCount = 0;

                // Стерти символ, що залишився з хвоста
                int clearY = column.Y - column.Length;
                if(clearY >= 0 && clearY < height)
                {
                    Console.SetCursorPosition(column.X, clearY);
                    Console.Write(' ');
                }

                // Малюємо новий "кадр" потоку
                for(int i = 0; i < column.Length; i++)
                {
                    int y = column.Y - i;
                    if(y < 0 || y >= height) continue;

                    Console.SetCursorPosition(column.X, y);
                    char c = GetRandomChar();

                    var gradient = InterpolateColor("#00FF00", "#003300", i / (float)column.Length);
                    var style = new Style(foreground: gradient);

                    AnsiConsole.Write(new Markup($"[{style.Foreground.ToMarkup()}]{c}[/]"));
                }

                column.Y++;
                if(column.Y - column.Length > height)
                {
                    column.Y = 0;
                    column.Length = random.Next(4, 20);
                }
            }

            Thread.Sleep(50);
        }
    }

    static List<Column> InitializeColumns(int width, int height, Random random)
    {
        var columns = new List<Column>();
        for(int x = 0; x < width; x++)
        {
            if(random.NextDouble() < 0.3)
            {
                columns.Add(new Column
                {
                    X = x,
                    Y = random.Next(height),
                    Length = random.Next(4, 20),
                    Speed = random.Next(1, 4), // випадкова швидкість
                    FrameCount = 0
                });
            }
        }
        return columns;
    }

    static char GetRandomChar()
    {
        string chars = "01abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ@#$%^&*";
        return chars[new Random(Guid.NewGuid().GetHashCode()).Next(chars.Length)];
    }

    static Color InterpolateColor(string startHex, string endHex, float t)
    {
        t = Math.Clamp(t, 0, 1);
        var start = HexToRgb(startHex);
        var end = HexToRgb(endHex);

        int r = (int)(start.r + (end.r - start.r) * t);
        int g = (int)(start.g + (end.g - start.g) * t);
        int b = (int)(start.b + (end.b - start.b) * t);

        return new Color((byte)r, (byte)g, (byte)b);
    }

    static (int r, int g, int b) HexToRgb(string hex)
    {
        hex = hex.TrimStart('#');
        return (
            Convert.ToInt32(hex.Substring(0, 2), 16),
            Convert.ToInt32(hex.Substring(2, 2), 16),
            Convert.ToInt32(hex.Substring(4, 2), 16)
        );
    }
}
