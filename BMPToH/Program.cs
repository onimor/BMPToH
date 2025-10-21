using System;
using System.Drawing;
using System.IO;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Использование: BMPToH <input.bmp> [--vertical | --horizontal]");
            Console.WriteLine("По умолчанию используется режим --vertical (SSD1306/SSD1309).");
            return;
        }

        string inputPath = args[0];
        if (!File.Exists(inputPath))
        {
            Console.WriteLine($"Файл {inputPath} не найден.");
            return;
        }

        bool verticalMode = true;
        if (args.Length > 1)
        {
            verticalMode = args[1].Equals("--vertical", StringComparison.OrdinalIgnoreCase);
        }

        string fileName = Path.GetFileNameWithoutExtension(inputPath);
        string outputPath = Path.ChangeExtension(inputPath, ".h");

        using Bitmap bmp = new Bitmap(inputPath);
        int width = bmp.Width;
        int height = bmp.Height;

        using var data = new MemoryStream();

        if (verticalMode)
        {
            // --- РЕЖИМ ДЛЯ SSD1306 / SSD1309 ---
            int pages = (height + 7) / 8; // округляем вверх

            for (int x = 0; x < width; x++)
            {
                for (int page = 0; page < pages; page++)
                {
                    byte b = 0;
                    for (int bit = 0; bit < 8; bit++)
                    {
                        int y = page * 8 + bit;
                        int bitValue = 0;

                        if (y < height)
                        {
                            Color pixel = bmp.GetPixel(x, y);
                            bitValue = (pixel.R + pixel.G + pixel.B) / 3 < 128 ? 1 : 0;
                        }

                        b |= (byte)(bitValue << bit);
                    }
                    data.WriteByte(b);
                }
            }
        }
        else
        {
            // --- ГОРИЗОНТАЛЬНЫЙ РЕЖИМ ---
            for (int y = 0; y < height; y++)
            {
                int bitCount = 0;
                int currentByte = 0;

                for (int x = 0; x < width; x++)
                {
                    Color pixel = bmp.GetPixel(x, y);
                    int bit = (pixel.R + pixel.G + pixel.B) / 3 < 128 ? 1 : 0;
                    currentByte = (currentByte << 1) | bit;
                    bitCount++;

                    if (bitCount == 8)
                    {
                        data.WriteByte((byte)currentByte);
                        bitCount = 0;
                        currentByte = 0;
                    }
                }

                if (bitCount > 0)
                {
                    currentByte <<= (8 - bitCount);
                    data.WriteByte((byte)currentByte);
                }
            }
        }

        // --- Запись .h файла ---
        byte[] bytes = data.ToArray();

        using var writer = new StreamWriter(outputPath, false, Encoding.UTF8);
        writer.WriteLine($"// Bitmap: {width}x{height}");
        writer.WriteLine($"// Mode: {(verticalMode ? "VERTICAL (SSD1306/SSD1309)" : "HORIZONTAL")}");
        writer.WriteLine($"const unsigned char {fileName}[] = {{");

        for (int i = 0; i < bytes.Length; i++)
        {
            writer.Write($"0x{bytes[i]:X2}");
            if (i < bytes.Length - 1) writer.Write(", ");
            if ((i + 1) % 16 == 0) writer.WriteLine();
        }

        writer.WriteLine("\n};");
        writer.WriteLine($"// Total bytes: {bytes.Length}");
        writer.WriteLine($"// Width: {width}, Height: {height}");

        Console.WriteLine($"✅ Конвертация завершена!");
        Console.WriteLine($"Режим: {(verticalMode ? "VERTICAL (SSD1306/SSD1309)" : "HORIZONTAL")}");
        Console.WriteLine($"Файл сохранён: {outputPath}");
    }
}
