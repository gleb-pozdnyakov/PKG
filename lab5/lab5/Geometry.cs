using System;

namespace lab5
{
    public class OperationTracker
    {
        public long Multiplications { get; set; }
        public long Divisions { get; set; }

        public void Reset()
        {
            Multiplications = 0;
            Divisions = 0;
        }
    }

    public struct Point2D
    {
        public double X;
        public double Y;

        public const double Epsilon = 1e-7;

        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static bool IsEqual(double a, double b) => Math.Abs(a - b) < Epsilon;
        public static bool IsLess(double a, double b) => (b - a) >= Epsilon;
        public static bool IsLessOrEqual(double a, double b) => (b - a) > -Epsilon;
        public static bool IsGreater(double a, double b) => (a - b) >= Epsilon;
        public static bool IsGreaterOrEqual(double a, double b) => (a - b) > -Epsilon;
    }

    public struct Segment2D
    {
        public Point2D P1;
        public Point2D P2;
        public AABB Bounds;

        public Segment2D(Point2D p1, Point2D p2)
        {
            P1 = p1;
            P2 = p2;

            double minX = Math.Min(p1.X, p2.X);
            double maxX = Math.Max(p1.X, p2.X);
            double minY = Math.Min(p1.Y, p2.Y);
            double maxY = Math.Max(p1.Y, p2.Y);
            Bounds = new AABB(minX, minY, maxX, maxY);
        }
    }

    public struct AABB
    {
        public double XMin;
        public double YMin;
        public double XMax;
        public double YMax;

        public AABB(double xMin, double yMin, double xMax, double yMax)
        {
            XMin = xMin;
            YMin = yMin;
            XMax = xMax;
            YMax = yMax;
        }

        public readonly bool Intersects(AABB other)
        {
            if (Point2D.IsGreater(XMin, other.XMax) || Point2D.IsLess(XMax, other.XMin)) return false;
            if (Point2D.IsGreater(YMin, other.YMax) || Point2D.IsLess(YMax, other.YMin)) return false;
            return true;
        }

        public readonly bool Contains(Point2D p)
        {
            return Point2D.IsGreaterOrEqual(p.X, XMin) && Point2D.IsLessOrEqual(p.X, XMax) &&
                   Point2D.IsGreaterOrEqual(p.Y, YMin) && Point2D.IsLessOrEqual(p.Y, YMax);
        }
    }
}
