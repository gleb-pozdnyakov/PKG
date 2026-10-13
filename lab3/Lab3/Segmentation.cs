using System;

namespace Lab3
{
    public static class Segmentation
    {
        private static readonly int[,] SobelX = {
            { -1, 0, 1 },
            { -2, 0, 2 },
            { -1, 0, 1 }
        };

        private static readonly int[,] SobelY = {
            { -1, -2, -1 },
            {  0,  0,  0 },
            {  1,  2,  1 }
        };

        public static void ApplySobel(PixelBuffer input, EdgeStrategy strategy, double threshold, out PixelBuffer magnitudeOutput, out PixelBuffer directionOutput)
        {
            int width = input.Width;
            int height = input.Height;

            magnitudeOutput = new PixelBuffer(width, height);
            directionOutput = new PixelBuffer(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double gradX = 0;
                    double gradY = 0;

                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            byte r, g, b;
                            input.GetPixelExtrapolated(x + kx, y + ky, strategy, out r, out g, out b);
                            double intensity = 0.299 * r + 0.587 * g + 0.114 * b;

                            gradX += intensity * SobelX[ky + 1, kx + 1];
                            gradY += intensity * SobelY[ky + 1, kx + 1];
                        }
                    }

                    double magnitude = Math.Sqrt(gradX * gradX + gradY * gradY);
                    byte magByte = (byte)Math.Max(0, Math.Min(255, magnitude));

                    if (magByte < threshold)
                    {
                        magnitudeOutput.SetPixelUnsafe(x, y, 0, 0, 0);
                    }
                    else
                    {
                        magnitudeOutput.SetPixelUnsafe(x, y, magByte, magByte, magByte);
                    }

                    double angle = Math.Atan2(gradY, gradX);
                    if (angle < 0)
                    {
                        angle += 2 * Math.PI;
                    }

                    byte rDir, gDir, bDir;
                    AngleToRgb(angle, out rDir, out gDir, out bDir);

                    if (magnitude > 10)
                    {
                        directionOutput.SetPixelUnsafe(x, y, rDir, gDir, bDir);
                    }
                    else
                    {
                        directionOutput.SetPixelUnsafe(x, y, 0, 0, 0);
                    }
                }
            }
        }

        private static void AngleToRgb(double angle, out byte r, out byte g, out byte b)
        {
            double degree = angle * (180.0 / Math.PI);
            double sector = degree / 60.0;
            int i = (int)Math.Floor(sector);
            double f = sector - i;

            double p = 0;
            double q = 255 * (1 - f);
            double t = 255 * f;

            switch (i % 6)
            {
                case 0: r = 255; g = (byte)t; b = (byte)p; break;
                case 1: r = (byte)q; g = 255; b = (byte)p; break;
                case 2: r = (byte)p; g = 255; b = (byte)t; break;
                case 3: r = (byte)p; g = (byte)q; b = 255; break;
                case 4: r = (byte)t; g = (byte)p; b = 255; break;
                default: r = 255; g = (byte)p; b = (byte)q; break;
            }
        }
    }
}
