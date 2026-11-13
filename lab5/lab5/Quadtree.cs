using System.Collections.Generic;

namespace lab5
{
    public class QuadtreeNode
    {
        public AABB Bounds { get; private set; }
        public int MaxObjects { get; } = 32;
        public int MaxLevels { get; } = 8;
        public int Level { get; }

        public List<Segment2D> Objects { get; } = new List<Segment2D>();
        public QuadtreeNode[] Children { get; private set; } = null;

        public QuadtreeNode(AABB bounds, int level = 0)
        {
            Bounds = bounds;
            Level = level;
        }

        public void Clear()
        {
            Objects.Clear();
            if (Children != null)
            {
                foreach (var child in Children) child.Clear();
                Children = null;
            }
        }

        private void Subdivide()
        {
            double midX = Bounds.XMin + (Bounds.XMax - Bounds.XMin) / 2;
            double midY = Bounds.YMin + (Bounds.YMax - Bounds.YMin) / 2;

            Children = new QuadtreeNode[4];
            Children[0] = new QuadtreeNode(new AABB(Bounds.XMin, midY, midX, Bounds.YMax), Level + 1);
            Children[1] = new QuadtreeNode(new AABB(midX, midY, Bounds.XMax, Bounds.YMax), Level + 1);
            Children[2] = new QuadtreeNode(new AABB(Bounds.XMin, Bounds.YMin, midX, midY), Level + 1);
            Children[3] = new QuadtreeNode(new AABB(midX, Bounds.YMin, Bounds.XMax, midY), Level + 1);
        }

        public void Insert(Segment2D item)
        {
            if (!Bounds.Intersects(item.Bounds)) return;

            if (Children != null)
            {
                foreach (var child in Children) child.Insert(item);
                return;
            }

            Objects.Add(item);

            if (Objects.Count > MaxObjects && Level < MaxLevels)
            {
                if (Children == null) Subdivide();

                foreach (var obj in Objects)
                {
                    foreach (var child in Children) child.Insert(obj);
                }
                Objects.Clear();
            }
        }

        public void Query(AABB searchBounds, HashSet<Segment2D> result)
        {
            if (!Bounds.Intersects(searchBounds)) return;

            if (Children != null)
            {
                foreach (var child in Children) child.Query(searchBounds, result);
            }
            else
            {
                foreach (var obj in Objects)
                {
                    if (obj.Bounds.Intersects(searchBounds))
                    {
                        result.Add(obj);
                    }
                }
            }
        }
    }
}
