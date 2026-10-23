using System;

namespace lab4
{
    public static class RasterEngine
    {
        public static void BresenhamLine(int x0, int y0, int x1, int y1, IRasterTarget target)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);

            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;

            int err = dx - dy;

            while (true)
            {
                target.SetPixel(x0, y0, 1.0);

                if (x0 == x1 && y0 == y1) break;

                int e2 = 2 * err;

                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        public static void StepByStepLine(int x0, int y0, int x1, int y1, IRasterTarget target)
        {
            if (x0 == x1 && y0 == y1)
            {
                target.SetPixel(x0, y0, 1.0);
                return;
            }

            if (Math.Abs(x1 - x0) >= Math.Abs(y1 - y0))
            {
                if (x0 > x1)
                {
                    int t = x0; x0 = x1; x1 = t;
                    t = y0; y0 = y1; y1 = t;
                }
                double k = (double)(y1 - y0) / (x1 - x0);
                for (int x = x0; x <= x1; x++)
                {
                    int y = (int)Math.Round(y0 + k * (x - x0));
                    target.SetPixel(x, y, 1.0);
                }
            }
            else
            {
                if (y0 > y1)
                {
                    int t = x0; x0 = x1; x1 = t;
                    t = y0; y0 = y1; y1 = t;
                }
                double kInv = (double)(x1 - x0) / (y1 - y0);
                for (int y = y0; y <= y1; y++)
                {
                    int x = (int)Math.Round(x0 + kInv * (y - y0));
                    target.SetPixel(x, x, 1.0);
                }
            }
        }

        public static void DDALine(int x0, int y0, int x1, int y1, IRasterTarget target)
        {
            int dx = x1 - x0;
            int dy = y1 - y0;
            int steps = Math.Max(Math.Abs(dx), Math.Abs(dy));

            if (steps == 0)
            {
                target.SetPixel(x0, y0, 1.0);
                return;
            }

            double xInc = (double)dx / steps;
            double yInc = (double)dy / steps;

            double x = x0;
            double y = y0;

            for (int i = 0; i <= steps; i++)
            {
                target.SetPixel((int)x, (int)y, 1.0);

                x += xInc;
                y += yInc;
            }
        }



        public static void BresenhamCircle(int xc, int yc, int r, IRasterTarget target)
        {
            int x = 0;
            int y = r;
            int d = 3 - 2 * r;

            while (x <= y)
            {
                target.SetPixel(xc + x, yc + y, 1.0);
                target.SetPixel(xc - x, yc + y, 1.0);
                target.SetPixel(xc + x, yc - y, 1.0);
                target.SetPixel(xc - x, yc - y, 1.0);
                target.SetPixel(xc + y, yc + x, 1.0);
                target.SetPixel(xc - y, yc + x, 1.0);
                target.SetPixel(xc + y, yc - x, 1.0);
                target.SetPixel(xc - y, yc - x, 1.0);

                if (d < 0)
                {
                    d += 4 * x + 6;
                }
                else
                {
                    d += 4 * (x - y) + 10;
                    y--;
                }
                x++;
            }
        }

        public static void WuLine(int x0, int y0, int x1, int y1, IRasterTarget target)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                int t = x0; x0 = y0; y0 = t;
                t = x1; x1 = y1; y1 = t;
            }
            if (x0 > x1)
            {
                int t = x0; x0 = x1; x1 = t;
                t = y0; y0 = y1; y1 = t;
            }

            double dx = x1 - x0;
            double dy = y1 - y0;
            double gradient = dx == 0.0 ? 1.0 : dy / dx;

            int xend = x0;
            double yend = y0 + gradient * (xend - x0);
            int xpxl1 = xend;
            int ypxl1 = (int)Math.Floor(yend);

            if (steep)
            {
                target.SetPixel(ypxl1, xpxl1, 1.0 - (yend - Math.Floor(yend)));
                target.SetPixel(ypxl1 + 1, xpxl1, yend - Math.Floor(yend));
            }
            else
            {
                target.SetPixel(xpxl1, ypxl1, 1.0 - (yend - Math.Floor(yend)));
                target.SetPixel(xpxl1, ypxl1 + 1, yend - Math.Floor(yend));
            }

            double intery = y0 + gradient;

            xend = x1;
            yend = y1 + gradient * (xend - x1);
            int xpxl2 = xend;
            int ypxl2 = (int)Math.Floor(yend);

            if (steep)
            {
                target.SetPixel(ypxl2, xpxl2, 1.0 - (yend - Math.Floor(yend)));
                target.SetPixel(ypxl2 + 1, xpxl2, yend - Math.Floor(yend));
            }
            else
            {
                target.SetPixel(xpxl2, ypxl2, 1.0 - (yend - Math.Floor(yend)));
                target.SetPixel(xpxl2, ypxl2 + 1, yend - Math.Floor(yend));
            }

            if (steep)
            {
                for (int x = xpxl1 + 1; x < xpxl2; x++)
                {
                    target.SetPixel((int)Math.Floor(intery), x, 1.0 - (intery - Math.Floor(intery)));
                    target.SetPixel((int)Math.Floor(intery) + 1, x, intery - Math.Floor(intery));
                    intery += gradient;
                }
            }
            else
            {
                for (int x = xpxl1 + 1; x < xpxl2; x++)
                {
                    target.SetPixel(x, (int)Math.Floor(intery), 1.0 - (intery - Math.Floor(intery)));
                    target.SetPixel(x, (int)Math.Floor(intery) + 1, intery - Math.Floor(intery));
                    intery += gradient;
                }
            }
        }

        public static void CastlePitewayArc(int xc, int yc, int r, IRasterTarget target)
        {
            int x = 0;
            int y = r;
            int d = 3 - 2 * r;

            while (x <= y)
            {
                target.SetPixel(xc + x, yc - y, 1.0);
                target.SetPixel(xc + y, yc - x, 1.0);

                if (d < 0)
                {
                    d += 4 * x + 6;
                }
                else
                {
                    d += 4 * (x - y) + 10;
                    y--;
                }
                x++;
            }
        }

    }
}
