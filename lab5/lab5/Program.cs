using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace lab5
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }

    public partial class MainForm : Form
    {
        private List<Segment2D> allSegments = new List<Segment2D>();
        private List<Point2D> subjectPolygon = new List<Point2D>();
        private QuadtreeNode quadtree;
        private AABB clipWindow;

        private double zoom = 1.0;
        private PointF panOffset = new PointF(0, 0);
        private PointF lastMousePos;
        private bool isDraggingWindow = false;
        private bool isResizingWindow = false;

        private SKGLControl skiaCanvas;
        private Label txtVisibleCount;
        private Label txtTime;
        private Label txtOps;
        private CheckBox cbUseQuadtree;
        private RadioButton rbCohen;

        private List<Segment2D> queryCache = new List<Segment2D>();
        private List<Segment2D> clippedSegments = new List<Segment2D>();
        private Point2D[][] polyClippedResult = null!;
        private SKPicture? cachedBackgroundPicture = null;

        private SKPath cachedBgLinesPath = new SKPath();
        private SKPath clippedLinesPath = new SKPath();
        private OperationTracker tracker = new OperationTracker();
        private long lastTickTime = 0;

        public MainForm()
        {
            this.Text = "Отсечение геометрии";
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;

            using (Process p = Process.GetCurrentProcess())
            {
                p.PriorityClass = ProcessPriorityClass.High;
            }

            Panel panel = new Panel { Width = 280, Dock = DockStyle.Left, BackColor = Color.FromArgb(245, 245, 247) };
            this.Controls.Add(panel);

            Button btnLoadFile = new Button { Text = "📁 Загрузить из файла (input.txt)", Top = 15, Left = 15, Width = 250, Height = 35 };
            btnLoadFile.Click += (s, e) => LoadSegmentsFromFile();
            panel.Controls.Add(btnLoadFile);

            Button btnGenLines = new Button { Text = "Сгенерировать 500 000 линий", Top = 60, Left = 15, Width = 250, Height = 35 };
            btnGenLines.Click += (s, e) => GenerateSampleLines(500000);
            panel.Controls.Add(btnGenLines);

            Button btnGenPoly = new Button { Text = "Сгенерировать случайный полигон", Top = 105, Left = 15, Width = 250, Height = 35 };
            btnGenPoly.Click += (s, e) => GenerateRandomPolygon();
            panel.Controls.Add(btnGenPoly);

            rbCohen = new RadioButton { Text = "Сазерленд-Коэн", Top = 155, Left = 20, Checked = true, Width = 200 };
            rbCohen.CheckedChanged += (s, e) => UpdateScene();
            panel.Controls.Add(rbCohen);

            RadioButton rbLiang = new RadioButton { Text = "Лианг-Барски", Top = 180, Left = 20, Width = 200 };
            rbLiang.CheckedChanged += (s, e) => UpdateScene();
            panel.Controls.Add(rbLiang);

            cbUseQuadtree = new CheckBox { Text = "Использовать Quadtree индекс", Top = 215, Left = 20, Checked = true, Width = 230 };
            cbUseQuadtree.CheckedChanged += (s, e) => UpdateScene();
            panel.Controls.Add(cbUseQuadtree);

            Label lbl1 = new Label { Text = "Выбрано в кадре:", Top = 260, Left = 15, Width = 200 };
            txtVisibleCount = new Label { Text = "0", Top = 280, Left = 15, Font = new Font(this.Font, FontStyle.Bold), ForeColor = Color.Blue, Width = 200 };
            panel.Controls.Add(lbl1); panel.Controls.Add(txtVisibleCount);

            Label lbl2 = new Label { Text = "Время отсечения:", Top = 310, Left = 15, Width = 200 };
            txtTime = new Label { Text = "0.00 мс", Top = 330, Left = 15, Font = new Font(this.Font, FontStyle.Bold), ForeColor = Color.Green, Width = 200 };
            panel.Controls.Add(lbl2); panel.Controls.Add(txtTime);

            Label lbl3 = new Label { Text = "Арифм. операции (умн / дел):", Top = 365, Left = 15, Width = 250 };
            txtOps = new Label { Text = "0 / 0", Top = 385, Left = 15, Font = new Font(this.Font, FontStyle.Bold), ForeColor = Color.OrangeRed, Width = 200 };
            panel.Controls.Add(lbl3); panel.Controls.Add(txtOps);

            skiaCanvas = new SKGLControl { Dock = DockStyle.Fill };
            skiaCanvas.PaintSurface += SkiaCanvas_PaintSurface;
            skiaCanvas.MouseDown += Canvas_MouseDown;
            skiaCanvas.MouseMove += Canvas_MouseMove;
            skiaCanvas.MouseUp += Canvas_MouseUp;
            skiaCanvas.MouseWheel += Canvas_MouseWheel;
            this.Controls.Add(skiaCanvas);

            clipWindow = new AABB(200, 200, 500, 450);
            quadtree = new QuadtreeNode(new AABB(-5000, -5000, 5000, 5000));

            GenerateSampleLines(500000);
        }

        private void LoadSegmentsFromFile()
        {
            using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string[] lines = File.ReadAllLines(ofd.FileName);
                        if (lines.Length < 2) return;

                        allSegments.Clear(); quadtree.Clear(); subjectPolygon.Clear(); polyClippedResult = null!;
                        InvalidateBackgroundCache();

                        int n = int.Parse(lines[0].Trim(), System.Globalization.CultureInfo.InvariantCulture);

                        int currentLineIdx = 1;
                        for (int i = 0; i < n && currentLineIdx < lines.Length; i++)
                        {
                            string[] tokens = lines[currentLineIdx++].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if (tokens.Length < 4) continue;

                            double x1 = double.Parse(tokens[0], System.Globalization.CultureInfo.InvariantCulture);
                            double y1 = double.Parse(tokens[1], System.Globalization.CultureInfo.InvariantCulture);
                            double x2 = double.Parse(tokens[2], System.Globalization.CultureInfo.InvariantCulture);
                            double y2 = double.Parse(tokens[3], System.Globalization.CultureInfo.InvariantCulture);

                            var seg = new Segment2D(new Point2D(x1, y1), new Point2D(x2, y2));
                            allSegments.Add(seg); quadtree.Insert(seg);
                        }

                        if (currentLineIdx < lines.Length)
                        {
                            string[] winTokens = lines[currentLineIdx].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if (winTokens.Length >= 4)
                            {
                                clipWindow.XMin = double.Parse(winTokens[0], System.Globalization.CultureInfo.InvariantCulture);
                                clipWindow.YMin = double.Parse(winTokens[1], System.Globalization.CultureInfo.InvariantCulture);
                                clipWindow.XMax = double.Parse(winTokens[2], System.Globalization.CultureInfo.InvariantCulture);
                                clipWindow.YMax = double.Parse(winTokens[3], System.Globalization.CultureInfo.InvariantCulture);
                            }
                        }

                        zoom = 1.0; panOffset = new PointF(0, 0);
                        UpdateScene();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка чтения файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void GenerateSampleLines(int count)
        {
            allSegments.Clear(); quadtree.Clear(); subjectPolygon.Clear(); polyClippedResult = null!;
            InvalidateBackgroundCache();

            Random rnd = new Random(42);
            for (int i = 0; i < count; i++)
            {
                double x1 = rnd.NextDouble() * 1600 - 400; double y1 = rnd.NextDouble() * 1200 - 300;
                double x2 = x1 + (rnd.NextDouble() * 60 - 30); double y2 = y1 + (rnd.NextDouble() * 60 - 30);
                var seg = new Segment2D(new Point2D(x1, y1), new Point2D(x2, y2));
                allSegments.Add(seg); quadtree.Insert(seg);
            }
            UpdateScene();
        }

        private void GenerateRandomPolygon()
        {
            allSegments.Clear(); quadtree.Clear(); subjectPolygon.Clear(); clippedSegments.Clear(); polyClippedResult = null!;
            InvalidateBackgroundCache();

            Random polyRnd = new Random();
            double cx = 350, cy = 320; int pointsCount = polyRnd.Next(5, 12);
            for (int i = 0; i < pointsCount; i++)
            {
                double angle = (i * 2 * Math.PI) / pointsCount;
                double r = (i % 2 == 0) ? polyRnd.Next(130, 180) : polyRnd.Next(50, 90);
                subjectPolygon.Add(new Point2D(cx + r * Math.Cos(angle), cy + r * Math.Sin(angle)));
            }
            UpdateScene();
        }

        private void InvalidateBackgroundCache()
        {
            if (cachedBackgroundPicture != null)
            {
                cachedBackgroundPicture.Dispose();
                cachedBackgroundPicture = null;
            }
        }

        private void UpdateScene()
        {
            long currentTick = Environment.TickCount64;
            if (currentTick - lastTickTime < 12 && subjectPolygon.Count == 0) return;
            lastTickTime = currentTick;

            tracker.Reset();
            Stopwatch sw = Stopwatch.StartNew();

            IEnumerable<Segment2D> segmentsToProcess = allSegments;
            if (cbUseQuadtree.Checked && subjectPolygon.Count == 0)
            {
                HashSet<Segment2D> hashResult = new HashSet<Segment2D>();
                quadtree.Query(clipWindow, hashResult);
                queryCache.Clear(); queryCache.AddRange(hashResult);
                segmentsToProcess = queryCache;
            }

            var clippedSegmentsBag = new System.Collections.Concurrent.ConcurrentBag<Segment2D>();
            bool useCohen = rbCohen.Checked;
            object lockObj = new object();
            int processedCount = 0;

            if (subjectPolygon.Count == 0)
            {
                System.Threading.Tasks.Parallel.ForEach(segmentsToProcess, () => new OperationTracker(), (seg, loopState, localTracker) =>
                {
                    var res = useCohen
                        ? LineClippers.ClipCohenSutherland(seg, clipWindow, localTracker)
                        : LineClippers.ClipLiangBarsky(seg, clipWindow, localTracker);

                    if (res.HasValue) clippedSegmentsBag.Add(res.Value);
                    return localTracker;
                },
                (localTracker) => {
                    lock (lockObj)
                    {
                        tracker.Multiplications += localTracker.Multiplications;
                        tracker.Divisions += localTracker.Divisions;
                    }
                });

                processedCount = clippedSegmentsBag.Count;
                clippedSegments = new List<Segment2D>(clippedSegmentsBag);
                polyClippedResult = null!;

                clippedLinesPath.Reset();
                int itemsLimit = clippedSegments.Count;

                System.Threading.Tasks.Parallel.For(0, Environment.ProcessorCount, () => new SKPath(), (threadIdx, loopState, localPath) =>
                {
                    int itemsPerThread = (itemsLimit + Environment.ProcessorCount - 1) / Environment.ProcessorCount;
                    int startIdx = threadIdx * itemsPerThread;
                    int endIdx = Math.Min(startIdx + itemsPerThread, itemsLimit);

                    for (int i = startIdx; i < endIdx; i++)
                    {
                        var seg = clippedSegments[i];
                        localPath.MoveTo((float)seg.P1.X, (float)seg.P1.Y);
                        localPath.LineTo((float)seg.P2.X, (float)seg.P2.Y);
                    }
                    return localPath;
                },
                (localPath) =>
                {
                    lock (lockObj)
                    {
                        clippedLinesPath.AddPath(localPath);
                        localPath.Dispose();
                    }
                });
            }
            else
            {
                polyClippedResult = PolygonClipper.ClipPolygon(subjectPolygon, clipWindow);
                processedCount = polyClippedResult != null ? polyClippedResult.Length : 0;
                clippedSegments.Clear();
                clippedLinesPath.Reset();
            }

            sw.Stop();
            txtVisibleCount.Text = processedCount.ToString();
            txtTime.Text = sw.Elapsed.TotalMilliseconds.ToString("F2") + " мс";
            txtOps.Text = $"{tracker.Multiplications} / {tracker.Divisions}";

            lock (lockObj)
            {
                skiaCanvas.Refresh();
            }
        }

        private void SkiaCanvas_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.White);

            float scaleX = (float)(e.BackendRenderTarget.Width / skiaCanvas.Width);
            float scaleY = (float)(e.BackendRenderTarget.Height / skiaCanvas.Height);
            canvas.Scale(scaleX, scaleY);

            canvas.Translate(panOffset.X, panOffset.Y);
            canvas.Scale((float)zoom, (float)zoom);

            using (var paint = new SKPaint { Color = SKColors.LightGray, StrokeWidth = 0.5f })
            {
                canvas.DrawLine(-10000, 0, 10000, 0, paint);
                canvas.DrawLine(0, -10000, 0, 10000, paint);
            }

            if (subjectPolygon.Count == 0)
            {
                if (cachedBackgroundPicture == null)
                {
                    using (var recorder = new SKPictureRecorder())
                    using (var recordingCanvas = recorder.BeginRecording(SKRect.Create(-10000, -10000, 20000, 20000)))
                    {
                        using (var paint = new SKPaint { Color = new SKColor(220, 50, 50, 140), StrokeWidth = 0.5f, IsAntialias = false, Style = SKPaintStyle.Stroke })
                        {
                            cachedBgLinesPath.Reset();
                            int totalLines = allSegments.Count;
                            for (int i = 0; i < totalLines; i++)
                            {
                                var seg = allSegments[i];
                                cachedBgLinesPath.MoveTo((float)seg.P1.X, (float)seg.P1.Y);
                                cachedBgLinesPath.LineTo((float)seg.P2.X, (float)seg.P2.Y);
                            }
                            recordingCanvas.DrawPath(cachedBgLinesPath, paint);
                        }
                        cachedBackgroundPicture = recorder.EndRecording();
                    }
                }
                canvas.DrawPicture(cachedBackgroundPicture);

                using (var paint = new SKPaint { Color = SKColors.Blue, StrokeWidth = 1.0f, IsAntialias = true, Style = SKPaintStyle.Stroke })
                {
                    canvas.DrawPath(clippedLinesPath, paint);
                }
            }
            else
            {
                using (var paint = new SKPaint { Color = new SKColor(255, 0, 0, 120), StrokeWidth = 1f, Style = SKPaintStyle.Stroke })
                {
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 4, 4 }, 0);
                    using (var path = CreateSkiaPath(subjectPolygon.ToArray()))
                    {
                        if (path != null) canvas.DrawPath(path, paint);
                    }
                }

                if (polyClippedResult != null && polyClippedResult.Length > 0)
                {
                    using (var fillPaint = new SKPaint { Color = new SKColor(52, 199, 89, 120), Style = SKPaintStyle.Fill })
                    using (var strokePaint = new SKPaint { Color = new SKColor(34, 139, 34), StrokeWidth = 2f, Style = SKPaintStyle.Stroke, IsAntialias = true })
                    {
                        for (int i = 0; i < polyClippedResult.Length; i++)
                        {
                            using (var path = CreateSkiaPath(polyClippedResult[i]))
                            {
                                if (path != null)
                                {
                                    canvas.DrawPath(path, fillPaint);
                                    canvas.DrawPath(path, strokePaint);
                                }
                            }
                        }
                    }
                }
            }

            using (var paint = new SKPaint { Color = SKColors.Black, StrokeWidth = 2.0f, Style = SKPaintStyle.Stroke, IsAntialias = true })
            {
                canvas.DrawRect((float)clipWindow.XMin, (float)clipWindow.YMin, (float)(clipWindow.XMax - clipWindow.XMin), (float)(clipWindow.YMax - clipWindow.YMin), paint);
            }
        }

        private SKPath? CreateSkiaPath(Point2D[] points)
        {
            if (points == null || points.Length == 0) return null;
            var path = new SKPath();
            path.MoveTo((float)points[0].X, (float)points[0].Y);
            for (int i = 1; i < points.Length; i++)
            {
                path.LineTo((float)points[i].X, (float)points[i].Y);
            }
            path.Close();
            return path;
        }
        private Point2D ToWorldSpace(PointF screenPos) => new Point2D((screenPos.X - panOffset.X) / zoom, (screenPos.Y - panOffset.Y) / zoom);
        private void Canvas_MouseDown(object? sender, MouseEventArgs e)
        {
            lastMousePos = e.Location; var worldPos = ToWorldSpace(lastMousePos);
            if (e.Button == MouseButtons.Left)
            {
                if (worldPos.X >= clipWindow.XMin && worldPos.X <= clipWindow.XMax &&
                worldPos.Y >= clipWindow.YMin && worldPos.Y <= clipWindow.YMax)
                {
                    isDraggingWindow = true;
                    skiaCanvas.Capture = true;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                isResizingWindow = true;
                skiaCanvas.Capture = true;
            }
        }
        private void Canvas_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!isDraggingWindow && !isResizingWindow) return;
            var worldPos = ToWorldSpace(e.Location); var prevWorldPos = ToWorldSpace(lastMousePos);
            double dx = worldPos.X - prevWorldPos.X; double dy = worldPos.Y - prevWorldPos.Y;
            if (isDraggingWindow) { clipWindow.XMin += dx; clipWindow.XMax += dx; clipWindow.YMin += dy; clipWindow.YMax += dy; }
            else if (isResizingWindow) { clipWindow.XMax = Math.Max(clipWindow.XMin + 10, clipWindow.XMax + dx); clipWindow.YMax = Math.Max(clipWindow.YMin + 10, clipWindow.YMax + dy); }
            lastMousePos = e.Location; UpdateScene();
        }
        private void Canvas_MouseUp(object? sender, MouseEventArgs e) { isDraggingWindow = false; isResizingWindow = false; skiaCanvas.Capture = false; }
        private void Canvas_MouseWheel(object? sender, MouseEventArgs e)
        {
            var worldBefore = ToWorldSpace(e.Location);
            if (e.Delta > 0) zoom *= 1.1; else zoom /= 1.1;
            zoom = Math.Max(0.1, Math.Min(zoom, 10.0));
            panOffset.X = (float)(e.Location.X - worldBefore.X * zoom);
            panOffset.Y = (float)(e.Location.Y - worldBefore.Y * zoom);
            skiaCanvas.Invalidate();
        }
    }
}