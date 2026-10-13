using System;

namespace Lab3
{
    public static class PointOperations
    {
        public static PixelBuffer ConvertToGrayscale(PixelBuffer input)
        {
            int width = input.Width;
            int height = input.Height;
            PixelBuffer output = new PixelBuffer(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte r, g, b;
                    input.GetPixelUnsafe(x, y, out r, out g, out b);
                    byte gray = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
                    output.SetPixelUnsafe(x, y, gray, gray, gray);
                }
            }

            return output;
        }

        public static PixelBuffer LinearContrast(PixelBuffer input)
        {
            int width = input.Width;
            int height = input.Height;
            PixelBuffer output = new PixelBuffer(width, height);

            byte minBrightness = 255;
            byte maxBrightness = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte r, g, b;
                    input.GetPixelUnsafe(x, y, out r, out g, out b);
                    byte gray = (byte)(0.299 * r + 0.587 * g + 0.114 * b);

                    if (gray < minBrightness) minBrightness = gray;
                    if (gray > maxBrightness) maxBrightness = gray;
                }
            }

            if (maxBrightness == minBrightness)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte r, g, b;
                        input.GetPixelUnsafe(x, y, out r, out g, out b);
                        output.SetPixelUnsafe(x, y, r, g, b);
                    }
                }
                return output;
            }

            double scale = 255.0 / (maxBrightness - minBrightness);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte r, g, b;
                    input.GetPixelUnsafe(x, y, out r, out g, out b);

                    int newR = (int)((r - minBrightness) * scale);
                    int newG = (int)((g - minBrightness) * scale);
                    int newB = (int)((b - minBrightness) * scale);

                    newR = Math.Max(0, Math.Min(255, newR));
                    newG = Math.Max(0, Math.Min(255, newG));
                    newB = Math.Max(0, Math.Min(255, newB));

                    output.SetPixelUnsafe(x, y, (byte)newR, (byte)newG, (byte)newB);
                }
            }

            return output;
        }
    }
}
