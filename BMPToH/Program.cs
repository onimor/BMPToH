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
            Console.WriteLine("Использование: BmpToHeader <input.bmp>");
            return;
        }

        string inputPath = args[0];
        if (!File.Exists(inputPath))
        {
            Console.WriteLine($"Файл {inputPath} не найден.");
            return;
        }

        string fileName = Path.GetFileNameWithoutExtension(inputPath);
        string outputPath = Path.ChangeExtension(inputPath, ".h");

        using Bitmap bmp = new Bitmap(inputPath);
        int width = bmp.Width;
        int height = bmp.Height;

        var data = new MemoryStream();

        for (int y = 0; y < height; y++)
        {
            int bitCount = 0;
            int currentByte = 0;

            for (int x = 0; x < width; x++)
            {
                Color pixel = bmp.GetPixel(x, y);
                int bit = (pixel.R + pixel.G + pixel.B) / 3 < 128 ? 1 : 0; // черный = 1, белый = 0

                currentByte = (currentByte << 1) | bit;
                bitCount++;

                if (bitCount == 8)
                {
                    data.WriteByte((byte)currentByte);
                    currentByte = 0;
                    bitCount = 0;
                }
            }

            if (bitCount > 0) // добиваем последний байт в строке
            {
                currentByte <<= (8 - bitCount);
                data.WriteByte((byte)currentByte);
            }
        }

        // Генерация .h файла
        using var writer = new StreamWriter(outputPath, false, Encoding.UTF8);
        writer.WriteLine($"const unsigned char {fileName}[] = {{");

        byte[] bytes = data.ToArray();
        for (int i = 0; i < bytes.Length; i++)
        {
            writer.Write($"0x{bytes[i]:X2}");
            if (i < bytes.Length - 1) writer.Write(", ");
            if ((i + 1) % 12 == 0) writer.WriteLine();
        }

        writer.WriteLine("\n};");

        Console.WriteLine($"Файл сохранён: {outputPath}");
    }
}
