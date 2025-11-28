using System;

namespace GltronMonoGame
{
    public class Segment
    {
        public Vec vStart = new Vec();
        public Vec vDirection = new Vec();
        public float t1 = 0.0f;
        public float t2 = 0.0f;

        public Segment() { }

        public Segment(Vec start, Vec direction)
        {
            vStart = start;
            vDirection = direction;
        }

        public Vec Intersect(Segment other)
        {
            Vec v1 = vDirection;
            Vec v2 = other.vDirection;
            Vec v3 = other.vStart.Sub(vStart);

            float cross = v1.v[0] * v2.v[1] - v1.v[1] * v2.v[0];

            if (Math.Abs(cross) < 0.0001f)
            {
                // Parallel or collinear
                return null;
            }

            t1 = (v3.v[0] * v2.v[1] - v3.v[1] * v2.v[0]) / cross;
            t2 = (v3.v[0] * v1.v[1] - v3.v[1] * v1.v[0]) / cross;

            if (t1 >= 0.0f && t1 < 1.0f && t2 >= 0.0f && t2 < 1.0f)
            {
                // Intersection point
                return new Vec(
                    vStart.v[0] + t1 * v1.v[0],
                    vStart.v[1] + t1 * v1.v[1]
                );
            }

            return null;
        }
    }
}
