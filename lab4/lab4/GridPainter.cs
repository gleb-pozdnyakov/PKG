using System;
using System.Drawing;
using System.Windows.Forms;

namespace lab4
{
    public class GridPainter
    {
        private readonly VirtualGrid _virtualGrid;
        private int _pixelSize;

        public GridPainter(VirtualGrid virtualGrid, int pixelSize)
        {
            _virtualGrid = virtualGrid;
            _pixelSize = pixelSize;
        }

        public int PixelSize
        {
            get => _pixelSize;
            set => _pixelSize = Math.Max(1, value);
        }

        public void Paint(Graphics g, int viewWidth, int viewHeight)
        {
            g.Clear(Color.White);

            int centerX = _virtualGrid.Width / 2;
            int centerY = _virtualGrid.Height / 2;

            int offsetX = viewWidth / 2 - centerX * _pixelSize;
            int offsetY = viewHeight / 2 - centerY * _pixelSize;

            for (int x = 0; x < _virtualGrid.Width; x++)
            {
                for (int y = 0; y < _virtualGrid.Height; y++)
                {
                    double intensity = _virtualGrid.GetPixel(x, y);
                    if (intensity > 0)
                    {
                        int alpha = (int)(intensity * 255);
                        using (Brush brush = new SolidBrush(Color.FromArgb(alpha, Color.Black)))
                        {
                            g.FillRectangle(brush, offsetX + x * _pixelSize, offsetY + y * _pixelSize, _pixelSize, _pixelSize);
                        }
                    }
                }
            }

            if (_pixelSize >= 5)
            {
                using (Pen gridPen = new Pen(Color.LightGray, 1))
                {
                    for (int x = 0; x <= _virtualGrid.Width; x++)
                    {
                        int posX = offsetX + x * _pixelSize;
                        g.DrawLine(gridPen, posX, offsetY, posX, offsetY + _virtualGrid.Height * _pixelSize);
                    }
                    for (int y = 0; y <= _virtualGrid.Height; y++)
                    {
                        int posY = offsetY + y * _pixelSize;
                        g.DrawLine(gridPen, offsetX, posY, offsetX + _virtualGrid.Width * _pixelSize, posY);
                    }
                }
            }

            using (Pen axisPen = new Pen(Color.Red, 2))
            {
                int axisX = offsetX + centerX * _pixelSize;
                int axisY = offsetY + centerY * _pixelSize + _pixelSize;

                g.DrawLine(axisPen, axisX, 0, axisX, viewHeight);
                g.DrawLine(axisPen, 0, axisY, viewWidth, axisY);
            }
        }
    }
}
