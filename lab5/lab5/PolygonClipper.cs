using System;
using System.Collections.Generic;

namespace lab5
{
    public class PolygonClipper
    {
        private const double EPSILON = 1e-7;

        public class PolyVertex
        {
            public Point2D Point;
            public PolyVertex Next = null!;
            public PolyVertex Prev = null!;
            public PolyVertex Neighbor = null!;
            public bool IsIntersection = false;
            public bool IsEntry = false;
            public bool Visited = false;
            public double DistanceAlongEdge = 0;

            public PolyVertex(Point2D p) { Point = p; }
        }

        private static bool IsEqual(double a, double b) => Math.Abs(a - b) < EPSILON;

        private static bool IsSamePoint(Point2D a, Point2D b) => IsEqual(a.X, b.X) && IsEqual(a.Y, b.Y);

        private static bool IsInsideWindow(Point2D p, AABB window)
        {
            return p.X >= window.XMin - EPSILON && p.X <= window.XMax + EPSILON &&
                   p.Y >= window.YMin - EPSILON && p.Y <= window.YMax + EPSILON;
        }

        private static List<Point2D> EnsureClockwise(List<Point2D> poly)
        {
            double signedArea = 0;
            for (int i = 0; i < poly.Count; i++)
            {
                Point2D p1 = poly[i];
                Point2D p2 = poly[(i + 1) % poly.Count];
                signedArea += (p2.X - p1.X) * (p2.Y + p1.Y);
            }

            List<Point2D> result = new List<Point2D>(poly);
            if (signedArea > 0)
            {
                result.Reverse();
            }
            return result;
        }

        private static Point2D? FindIntersection(Point2D a1, Point2D a2, Point2D b1, Point2D b2, out double ua, out double ub)
        {
            ua = 0; ub = 0;
            double den = (b2.Y - b1.Y) * (a2.X - a1.X) - (b2.X - b1.X) * (a2.Y - a1.Y);
            if (IsEqual(den, 0)) return null;

            ua = ((b2.X - b1.X) * (a1.Y - b1.Y) - (b2.Y - b1.Y) * (a1.X - b1.X)) / den;
            ub = ((a2.X - a1.X) * (a1.Y - b1.Y) - (a2.Y - a1.Y) * (a1.X - b1.X)) / den;

            if (ua >= -EPSILON && ua <= 1.0 + EPSILON && ub >= -EPSILON && ub <= 1.0 + EPSILON)
            {
                ua = Math.Clamp(ua, 0.0, 1.0);
                ub = Math.Clamp(ub, 0.0, 1.0);
                return new Point2D(a1.X + ua * (a2.X - a1.X), a1.Y + ua * (a2.Y - a1.Y));
            }
            return null;
        }

        private static PolyVertex CreateCircularList(List<Point2D> points)
        {
            PolyVertex head = new PolyVertex(points[0]);
            PolyVertex current = head;
            for (int i = 1; i < points.Count; i++)
            {
                PolyVertex next = new PolyVertex(points[i]);
                current.Next = next;
                next.Prev = current;
                current = next;
            }
            current.Next = head;
            head.Prev = current;
            return head;
        }

        private static List<PolyVertex> GetIntersectionsList(PolyVertex head)
        {
            List<PolyVertex> list = new List<PolyVertex>();
            PolyVertex current = head;
            do
            {
                if (current.IsIntersection) list.Add(current);
                current = current.Next;
            } while (current != head);
            return list;
        }

        private static void InsertVertex(PolyVertex v1, PolyVertex v2, PolyVertex intersect)
        {
            PolyVertex current = v1.Next;
            while (current != v2 && current.DistanceAlongEdge < intersect.DistanceAlongEdge)
            {
                current = current.Next;
            }
            PolyVertex prev = current.Prev;
            prev.Next = intersect;
            intersect.Prev = prev;
            intersect.Next = current;
            current.Prev = intersect;
        }

        public static Point2D[][] ClipPolygon(List<Point2D> subjectPoly, AABB window)
        {
            List<List<Point2D>> resultContours = new List<List<Point2D>>();
            if (subjectPoly == null || subjectPoly.Count < 3) return Array.Empty<Point2D[]>();

            List<Point2D> orderedSubject = EnsureClockwise(subjectPoly);

            List<Point2D> clipPoly = new List<Point2D>
            {
                new Point2D(window.XMin, window.YMin),
                new Point2D(window.XMax, window.YMin),
                new Point2D(window.XMax, window.YMax),
                new Point2D(window.XMin, window.YMax)
            };

            PolyVertex sHead = CreateCircularList(orderedSubject);
            PolyVertex cHead = CreateCircularList(clipPoly);

            PolyVertex sCurr = sHead;
            do
            {
                PolyVertex sNext = sCurr.Next;
                while (sNext.IsIntersection) sNext = sNext.Next;

                PolyVertex cCurr = cHead;
                do
                {
                    PolyVertex cNext = cCurr.Next;
                    while (cNext.IsIntersection) cNext = cNext.Next;

                    Point2D? pt = FindIntersection(sCurr.Point, sNext.Point, cCurr.Point, cNext.Point, out double ua, out double ub);
                    if (pt.HasValue)
                    {
                        bool atSubjectEnd = IsEqual(ua, 1.0);
                        bool atClipEnd = IsEqual(ub, 1.0);
                        if (atSubjectEnd || atClipEnd)
                        {
                            cCurr = cNext;
                            continue;
                        }

                        double sDx = sNext.Point.X - sCurr.Point.X;
                        double sDy = sNext.Point.Y - sCurr.Point.Y;
                        double cDx = cNext.Point.X - cCurr.Point.X;
                        double cDy = cNext.Point.Y - cCurr.Point.Y;
                        double cross = cDx * sDy - cDy * sDx;

                        if (!IsEqual(cross, 0))
                        {
                            bool isEntry = cross > 0;

                            PolyVertex sInter = new PolyVertex(pt.Value)
                            {
                                IsIntersection = true,
                                DistanceAlongEdge = ua,
                                IsEntry = isEntry
                            };
                            PolyVertex cInter = new PolyVertex(pt.Value)
                            {
                                IsIntersection = true,
                                DistanceAlongEdge = ub,
                                IsEntry = isEntry
                            };

                            sInter.Neighbor = cInter;
                            cInter.Neighbor = sInter;

                            InsertVertex(sCurr, sNext, sInter);
                            InsertVertex(cCurr, cNext, cInter);
                        }
                    }
                    cCurr = cNext;
                } while (cCurr != cHead);

                sCurr = sNext;
            } while (sCurr != sHead);

            List<PolyVertex> sIntersections = GetIntersectionsList(sHead);
            foreach (var startVertex in sIntersections)
            {
                if (!startVertex.Visited && startVertex.IsEntry)
                {
                    List<Point2D> contour = new List<Point2D>();
                    PolyVertex current = startVertex;
                    bool onSubject = true;

                    int safetyCounter = 0;
                    while (safetyCounter++ < 10000)
                    {
                        current.Visited = true;
                        if (current.Neighbor != null) current.Neighbor.Visited = true;

                        if (contour.Count == 0 || !IsSamePoint(contour[^1], current.Point))
                        {
                            contour.Add(current.Point);
                        }

                        if (current.IsIntersection)
                        {
                            if (onSubject && !current.IsEntry)
                            {
                                current = current.Neighbor!;
                                onSubject = false;
                            }
                            else if (!onSubject && current.IsEntry)
                            {
                                current = current.Neighbor!;
                                onSubject = true;
                            }
                        }

                        current = current.Next;

                        if (IsSamePoint(current.Point, startVertex.Point))
                        {
                            break;
                        }
                    }

                    if (contour.Count >= 3)
                    {
                        resultContours.Add(contour);
                    }
                }
            }

            if (resultContours.Count == 0)
            {
                bool allInside = true;
                foreach (var p in orderedSubject)
                {
                    if (!IsInsideWindow(p, window))
                    {
                        allInside = false;
                        break;
                    }
                }
                if (allInside)
                {
                    resultContours.Add(new List<Point2D>(orderedSubject));
                }
            }

            Point2D[][] finalArr = new Point2D[resultContours.Count][];
            for (int i = 0; i < resultContours.Count; i++)
            {
                finalArr[i] = resultContours[i].ToArray();
            }
            return finalArr;
        }
    }
}
