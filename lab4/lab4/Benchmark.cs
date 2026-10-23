using System;
using System.Diagnostics;

namespace lab4
{
    public class Benchmark
    {
        private readonly VirtualGrid _benchGrid;
        private readonly Random _rand;

        public Benchmark()
        {
            _benchGrid = new VirtualGrid(2000, 2000);
            _rand = new Random();
        }

        public long RunLineTest(string algorithmType, int count)
        {
            _benchGrid.Clear();
            int w = _benchGrid.Width;
            int h = _benchGrid.Height;

            int[,] coords = new int[count, 4];
            for (int i = 0; i < count; i++)
            {
                coords[i, 0] = _rand.Next(0, w);
                coords[i, 1] = _rand.Next(0, h);
                coords[i, 2] = _rand.Next(0, w);
                coords[i, 3] = _rand.Next(0, h);
            }

            Stopwatch sw = Stopwatch.StartNew();

            for (int i = 0; i < count; i++)
            {
                int x0 = coords[i, 0];
                int y0 = coords[i, 1];
                int x1 = coords[i, 2];
                int y1 = coords[i, 3];

                switch (algorithmType)
                {
                    case "Step":
                        RasterEngine.StepByStepLine(x0, y0, x1, y1, _benchGrid);
                        break;
                    case "DDA":
                        RasterEngine.DDALine(x0, y0, x1, y1, _benchGrid);
                        break;
                    case "Bresenham":
                        RasterEngine.BresenhamLine(x0, y0, x1, y1, _benchGrid);
                        break;
                    case "Wu":
                        RasterEngine.WuLine(x0, y0, x1, y1, _benchGrid);
                        break;
                }
            }

            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        public long RunCircleTest(int count)
        {
            _benchGrid.Clear();
            int w = _benchGrid.Width;
            int h = _benchGrid.Height;

            int[,] circles = new int[count, 3];
            for (int i = 0; i < count; i++)
            {
                circles[i, 0] = _rand.Next(500, w - 500);
                circles[i, 1] = _rand.Next(500, h - 500);
                circles[i, 2] = _rand.Next(5, 400);
            }

            Stopwatch sw = Stopwatch.StartNew();

            for (int i = 0; i < count; i++)
            {
                RasterEngine.BresenhamCircle(circles[i, 0], circles[i, 1], circles[i, 2], _benchGrid);
            }

            sw.Stop();
            return sw.ElapsedMilliseconds;
        }
    }
}
