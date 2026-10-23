using System;

namespace lab4
{
    public class VirtualGrid : IRasterTarget
    {
        private readonly double[,] _grid;
        public int Width { get; }
        public int Height { get; }

        public VirtualGrid(int width, int height)
        {
            Width = width;
            Height = height;
            _grid = new double[width, height];
        }

        public void SetPixel(int x, int y, double intensity)
        {
            // Жесткая проверка границ (защита от падения при стресс-тестах)
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                // Если в одну точку попадает несколько линий, выбираем максимальную яркость
                if (intensity > _grid[x, y])
                {
                    _grid[x, y] = intensity;
                }
            }
        }

        public double GetPixel(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                return _grid[x, y];
            return 0;
        }

        public void Clear()
        {
            Array.Clear(_grid, 0, _grid.Length);
        }
    }
}
