using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace lab4
{
    public partial class Form1 : Form
    {
        private VirtualGrid _virtualGrid;
        private GridPainter _gridPainter;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _virtualGrid = new VirtualGrid(100, 100);
            _gridPainter = new GridPainter(_virtualGrid, (int)numScale.Value);

            if (cmbAlgorithms.Items.Count == 0)
            {
                cmbAlgorithms.Items.Add("Пошаговый");
                cmbAlgorithms.Items.Add("ЦДА");
                cmbAlgorithms.Items.Add("Брезенхем (линия)");
                cmbAlgorithms.Items.Add("Брезенхем (окружность)");
                cmbAlgorithms.Items.Add("Алгоритм Ву");
                cmbAlgorithms.Items.Add("Кастл-Питвей (дуга)");
            }
            cmbAlgorithms.SelectedIndex = 2;

            pnlCanvas.Paint += PnlCanvas_Paint;
            pnlCanvas.Resize += PnlCanvas_Resize;
            numScale.ValueChanged += NumScale_ValueChanged;
            btnDraw.Click += btnDraw_Click;
            btnRunBenchmark.Click += btnRunBenchmark_Click;

            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, pnlCanvas, new object[] { true });
        }

        private void NumScale_ValueChanged(object sender, EventArgs e)
        {
            _gridPainter.PixelSize = (int)numScale.Value;
            pnlCanvas.Invalidate();
        }

        private void PnlCanvas_Resize(object sender, EventArgs e)
        {
            pnlCanvas.Invalidate();
        }

        private void PnlCanvas_Paint(object sender, PaintEventArgs e)
        {
            if (_gridPainter != null)
            {
                _gridPainter.Paint(e.Graphics, pnlCanvas.Width, pnlCanvas.Height);
            }
        }

        private void btnDraw_Click(object sender, EventArgs e)
        {
            _virtualGrid.Clear();

            int x0 = (int)numX0.Value;
            int y0 = (int)numY0.Value;
            int x1 = (int)numX1.Value;
            int y1 = (int)numY1.Value;
            int r = (int)numR.Value;

            int centerX = _virtualGrid.Width / 2;
            int centerY = _virtualGrid.Height / 2;

            int gridX0 = centerX + x0;
            int gridY0 = centerY - y0;
            int gridX1 = centerX + x1;
            int gridY1 = centerY - y1;
            int gridXc = centerX + x0;
            int gridYc = centerY - y0;

            Stopwatch sw = Stopwatch.StartNew();

            switch (cmbAlgorithms.SelectedIndex)
            {
                case 0:
                    RasterEngine.StepByStepLine(gridX0, gridY0, gridX1, gridY1, _virtualGrid);
                    break;
                case 1:
                    RasterEngine.DDALine(gridX0, gridY0, gridX1, gridY1, _virtualGrid);
                    break;
                case 2:
                    RasterEngine.BresenhamLine(gridX0, gridY0, gridX1, gridY1, _virtualGrid);
                    break;
                case 3:
                    RasterEngine.BresenhamCircle(gridXc, gridYc, r, _virtualGrid);
                    break;
                case 4:
                    RasterEngine.WuLine(gridX0, gridY0, gridX1, gridY1, _virtualGrid);
                    break;
                case 5:
                    RasterEngine.CastlePitewayArc(gridXc, gridYc, r, _virtualGrid);
                    break;
            }



            sw.Stop();
            lblTime.Text = $"Время: {sw.ElapsedTicks} тиков ({sw.Elapsed.TotalMilliseconds:F4} мс)";

            pnlCanvas.Invalidate();
        }

        private void btnRunBenchmark_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            btnRunBenchmark.Enabled = false;

            Benchmark bench = new Benchmark();

            long timeStep = bench.RunLineTest("Step", 100000);
            long timeDDA = bench.RunLineTest("DDA", 100000);
            long timeBres = bench.RunLineTest("Bresenham", 100000);
            long timeWu = bench.RunLineTest("Wu", 100000);

            long timeCircle = bench.RunCircleTest(50000);

            this.Cursor = Cursors.Default;
            btnRunBenchmark.Enabled = true;

            string report = "Результаты Бенчмарка (100 000 линий / 50 000 окр.):\n\n" +
                $"Пошаговый алгоритм: {timeStep} мс\n" +
                $"Алгоритм ЦДА: {timeDDA} мс\n" +
                $"Алгоритм Брезенхема: {timeBres} мс\n" +
                $"Алгоритм Ву (сглаживание): {timeWu} мс\n\n" +
                $"Окружность Брезенхема: {timeCircle} мс";
            MessageBox.Show(report, "Стресс-тестирование завершено", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
