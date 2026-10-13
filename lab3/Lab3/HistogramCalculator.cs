namespace Lab3
{
    public static class HistogramCalculator
    {
        public static int[] CalculateBrightnessHistogram(PixelBuffer buffer)
        {
            int[] histogram = new int[256];
            int width = buffer.Width;
            int height = buffer.Height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte r, g, b;
                    buffer.GetPixelUnsafe(x, y, out r, out g, out b);
                    byte brightness = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
                    histogram[brightness]++;
                }
            }

            return histogram;
        }
    }
}
