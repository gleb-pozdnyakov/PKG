using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Lab3
{
    public unsafe class PixelBuffer : IDisposable
    {
        private Bitmap _bitmap;
        private BitmapData _bitmapData;
        private byte* _basePtr;
        private bool _isLocked;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Stride { get; private set; }
        public int BytesPerPixel { get; private set; }
        public PixelBuffer(Bitmap bitmap)
        {
            _bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
            Width = bitmap.Width;
            Height = bitmap.Height;
            LockBuffer();
        }

        public PixelBuffer(int width, int height, PixelFormat format = PixelFormat.Format24bppRgb)
        {
            _bitmap = new Bitmap(width, height, format);
            Width = width;
            Height = height;
            LockBuffer();
        }

        private void LockBuffer()
        {
            if (_isLocked) return;

            PixelFormat format = _bitmap.PixelFormat;
            if (format != PixelFormat.Format24bppRgb && format != PixelFormat.Format32bppRgb)
            {
                Bitmap temp = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(temp))
                {
                    g.DrawImage(_bitmap, 0, 0, Width, Height);
                }
                _bitmap = temp;
                format = PixelFormat.Format24bppRgb;
            }

            BytesPerPixel = (format == PixelFormat.Format24bppRgb) ? 3 : 4;

            Rectangle rect = new Rectangle(0, 0, Width, Height);
            _bitmapData = _bitmap.LockBits(rect, ImageLockMode.ReadWrite, format);
            _basePtr = (byte*)_bitmapData.Scan0.ToPointer();
            Stride = _bitmapData.Stride;
            _isLocked = true;
        }

        public void UnlockBuffer()
        {
            if (!_isLocked) return;
            _bitmap.UnlockBits(_bitmapData);
            _isLocked = false;
        }

        public Bitmap GetBitmap()
        {
            UnlockBuffer();
            return _bitmap;
        }


        public void GetPixelUnsafe(int x, int y, out byte r, out byte g, out byte b)
        {
            byte* pixelPtr = _basePtr + (y * Stride) + (x * BytesPerPixel);
            b = pixelPtr[0];
            g = pixelPtr[1];
            r = pixelPtr[2];
        }

        public void SetPixelUnsafe(int x, int y, byte r, byte g, byte b)
        {
            byte* pixelPtr = _basePtr + (y * Stride) + (x * BytesPerPixel);
            pixelPtr[0] = b;
            pixelPtr[1] = g;
            pixelPtr[2] = r;
        }
        public void GetPixelExtrapolated(int x, int y, EdgeStrategy strategy, out byte r, out byte g, out byte b)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                GetPixelUnsafe(x, y, out r, out g, out b);
                return;
            }

            if (strategy == EdgeStrategy.Clamp)
            {
                x = Math.Max(0, Math.Min(x, Width - 1));
                y = Math.Max(0, Math.Min(y, Height - 1));
            }
            else if (strategy == EdgeStrategy.Reflect)
            {
                if (x < 0) x = -x - 1;
                if (x >= Width) x = 2 * Width - x - 1;
                if (y < 0) y = -y - 1;
                if (y >= Height) y = 2 * Height - y - 1;

                x = Math.Max(0, Math.Min(x, Width - 1));
                y = Math.Max(0, Math.Min(y, Height - 1));
            }

            GetPixelUnsafe(x, y, out r, out g, out b);
        }

        public void Dispose()
        {
            UnlockBuffer();
        }
    }
}
