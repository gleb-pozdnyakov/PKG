using System;

namespace lab5
{
    public static class LineClippers
    {
        private const int INSIDE = 0;
        private const int LEFT = 1;
        private const int RIGHT = 2;
        private const int BOTTOM = 4;
        private const int TOP = 8;

        private static int ComputeOutCode(Point2D p, AABB window)
        {
            int code = INSIDE;
            if (Point2D.IsLess(p.X, window.XMin)) code |= LEFT;
            else if (Point2D.IsGreater(p.X, window.XMax)) code |= RIGHT;

            if (Point2D.IsLess(p.Y, window.YMin)) code |= BOTTOM;
            else if (Point2D.IsGreater(p.Y, window.YMax)) code |= TOP;
            return code;
        }
        public static Segment2D? ClipCohenSutherland(Segment2D segment, AABB window, OperationTracker tracker)
        {
            if (!segment.Bounds.Intersects(window)) return null;

            double x1 = segment.P1.X, y1 = segment.P1.Y;
            double x2 = segment.P2.X, y2 = segment.P2.Y;

            int code1 = ComputeOutCode(segment.P1, window);
            int code2 = ComputeOutCode(segment.P2, window);
            bool accept = false;

            while (true)
            {
                if ((code1 | code2) == 0)
                {
                    accept = true;
                    break;
                }
                if ((code1 & code2) != 0)
                {
                    break;
                }

                int outCodeOut = code1 != 0 ? code1 : code2;
                double x = 0, y = 0;

                if ((outCodeOut & TOP) != 0)
                {
                    tracker.Multiplications++; tracker.Divisions++;
                    x = x1 + (x2 - x1) * (window.YMax - y1) / (y2 - y1);
                    y = window.YMax;
                }
                else if ((outCodeOut & BOTTOM) != 0)
                {
                    tracker.Multiplications++; tracker.Divisions++;
                    x = x1 + (x2 - x1) * (window.YMin - y1) / (y2 - y1);
                    y = window.YMin;
                }
                else if ((outCodeOut & RIGHT) != 0)
                {
                    tracker.Multiplications++; tracker.Divisions++;
                    y = y1 + (y2 - y1) * (window.XMax - x1) / (x2 - x1);
                    x = window.XMax;
                }
                else if ((outCodeOut & LEFT) != 0)
                {
                    tracker.Multiplications++; tracker.Divisions++;
                    y = y1 + (y2 - y1) * (window.XMin - x1) / (x2 - x1);
                    x = window.XMin;
                }

                if (outCodeOut == code1)
                {
                    x1 = x; y1 = y;
                    code1 = ComputeOutCode(new Point2D(x1, y1), window);
                }
                else
                {
                    x2 = x; y2 = y;
                    code2 = ComputeOutCode(new Point2D(x2, y2), window);
                }
            }

            if (accept)
                return new Segment2D(new Point2D(x1, y1), new Point2D(x2, y2));

            return null;
        }
        private static bool ClipTest(double p, double q, ref double t1, ref double t2, OperationTracker tracker)
        {
            if (Point2D.IsEqual(p, 0))
            {
                return Point2D.IsGreaterOrEqual(q, 0);
            }

            tracker.Divisions++;
            double t = q / p;
            if (p < 0)
            {
                if (t > t2) return false;
                if (t > t1) t1 = t;
            }
            else
            {
                if (t < t1) return false;
                if (t < t2) t2 = t;
            }
            return true;
        }
        public static Segment2D? ClipLiangBarsky(Segment2D segment, AABB window, OperationTracker tracker)
        {
            if (!segment.Bounds.Intersects(window)) return null;

            double x1 = segment.P1.X, y1 = segment.P1.Y;
            double dx = segment.P2.X - x1;
            double dy = segment.P2.Y - y1;

            double t1 = 0.0;
            double t2 = 1.0;

            if (ClipTest(-dx, x1 - window.XMin, ref t1, ref t2, tracker) &&
                ClipTest(dx, window.XMax - x1, ref t1, ref t2, tracker) &&
                ClipTest(-dy, y1 - window.YMin, ref t1, ref t2, tracker) &&
                ClipTest(dy, window.YMax - y1, ref t1, ref t2, tracker))
            {
                if (Point2D.IsLessOrEqual(t1, t2))
                {
                    tracker.Multiplications += 2;
                    double nx1 = x1 + t1 * dx;
                    double ny1 = y1 + t1 * dy;

                    tracker.Multiplications += 2;
                    double nx2 = x1 + t2 * dx;
                    double ny2 = y1 + t2 * dy;

                    return new Segment2D(new Point2D(nx1, ny1), new Point2D(nx2, ny2));
                }
            }
            return null;
        }
    }
}
