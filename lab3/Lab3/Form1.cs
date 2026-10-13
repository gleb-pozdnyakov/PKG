using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Lab3
{
    public partial class Form1 : Form
    {
        private Bitmap _sourceBitmap;
        private Bitmap _processedBitmap;
        private Bitmap _directionBitmap;

        public Form1()
        {
            InitializeComponent();
            SetupComponents();
        }

        private void SetupComponents()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.Add(EdgeStrategy.Clamp);
            comboBox1.Items.Add(EdgeStrategy.Reflect);
            comboBox1.SelectedIndex = 0;

            trackBar1.Minimum = 0;
            trackBar1.Maximum = 255;
            trackBar1.Value = 40;

            ConfigureChart(chart1);
            ConfigureChart(chart2);
        }

        private void ConfigureChart(Chart chart)
        {
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.ChartAreas.Add(new ChartArea("Default"));

            Series series = new Series("Brightness")
            {
                ChartType = SeriesChartType.Column,
                Color = Color.Gray
            };
            chart.Series.Add(series);
        }

        private void UpdateHistogram(Chart chart, Bitmap bitmap)
        {
            if (bitmap == null || chart.Series.Count == 0) return;

            chart.Series[0].Points.Clear();
            int[] histogram;

            using (PixelBuffer buffer = new PixelBuffer(bitmap))
            {
                histogram = HistogramCalculator.CalculateBrightnessHistogram(buffer);
            }

            for (int i = 0; i < histogram.Length; i++)
            {
                chart.Series[0].Points.AddXY(i, histogram[i]);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.BMP;*.JPG;*.PNG";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _sourceBitmap = new Bitmap(openFileDialog.FileName);
                    pictureBox1.Image = _sourceBitmap;

                    _processedBitmap = null;
                    _directionBitmap = null;
                    pictureBox2.Image = null;

                    UpdateHistogram(chart1, _sourceBitmap);
                    if (chart2.Series.Count > 0)
                    {
                        chart2.Series[0].Points.Clear();
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_sourceBitmap == null) return;

            using (PixelBuffer inputBuffer = new PixelBuffer(_sourceBitmap))
            {
                using (PixelBuffer outputBuffer = PointOperations.LinearContrast(inputBuffer))
                {
                    _processedBitmap = new Bitmap(outputBuffer.GetBitmap());
                }
            }
            pictureBox2.Image = _processedBitmap;
            UpdateHistogram(chart2, _processedBitmap);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (_sourceBitmap == null) return;

            EdgeStrategy strategy = (EdgeStrategy)comboBox1.SelectedItem;
            double threshold = trackBar1.Value;

            using (PixelBuffer inputBuffer = new PixelBuffer(_sourceBitmap))
            {
                PixelBuffer magBuffer;
                PixelBuffer dirBuffer;

                Segmentation.ApplySobel(inputBuffer, strategy, threshold, out magBuffer, out dirBuffer);

                _processedBitmap = new Bitmap(magBuffer.GetBitmap());
                _directionBitmap = new Bitmap(dirBuffer.GetBitmap());

                magBuffer.Dispose();
                dirBuffer.Dispose();
            }
            pictureBox2.Image = _processedBitmap;
            UpdateHistogram(chart2, _processedBitmap);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (_directionBitmap == null)
            {
                button3_Click(sender, e);
            }

            if (_directionBitmap != null)
            {
                pictureBox2.Image = _directionBitmap;
            }
        }
    }
}
