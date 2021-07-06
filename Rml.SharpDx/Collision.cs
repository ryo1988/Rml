using SharpDX;

namespace Rml.SharpDx
{
    public static class Collision
    {
        public static bool LineIntersectsRectangle(Vector2 p1, Vector2 p2, RectangleF rectangle)
        {
            return LineIntersectsLine(p1, p2, new Vector2(rectangle.X, rectangle.Y), new Vector2(rectangle.X + rectangle.Width, rectangle.Y)).isIntersect ||
                   LineIntersectsLine(p1, p2, new Vector2(rectangle.X + rectangle.Width, rectangle.Y),
                       new Vector2(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height)).isIntersect ||
                   LineIntersectsLine(p1, p2, new Vector2(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height),
                       new Vector2(rectangle.X, rectangle.Y + rectangle.Height)).isIntersect ||
                   LineIntersectsLine(p1, p2, new Vector2(rectangle.X, rectangle.Y + rectangle.Height), new Vector2(rectangle.X, rectangle.Y)).isIntersect ||
                   LineContainsRectangle(p1, p2, rectangle);
        }

        public static (bool isIntersect, Vector2 intersection) LineIntersectsLine(Vector2 l1P1, Vector2 l1P2, Vector2 l2P1, Vector2 l2P2, float allowableLimit = float.Epsilon)
        {
            var d = (l1P2.X - l1P1.X) * (l2P2.Y - l2P1.Y) - (l1P2.Y - l1P1.Y) * (l2P2.X - l2P1.X);

            if (System.Math.Abs(d) < allowableLimit)
            {
                return (false, default);
            }

            var u = ((l2P1.X - l1P1.X) * (l2P2.Y - l2P1.Y) - (l2P1.Y - l1P1.Y) * (l2P2.X - l2P1.X)) / d;
            var v = ((l2P1.X - l1P1.X) * (l1P2.Y - l1P1.Y) - (l2P1.Y - l1P1.Y) * (l1P2.X - l1P1.X)) / d;

            if (u < 0.0f - allowableLimit || u > 1.0f + allowableLimit || v < 0.0f - allowableLimit || v > 1.0f + allowableLimit)
            {
                return (false, default);
            }

            return (true, new Vector2(l1P1.X + u * (l1P2.X - l1P1.X),  l1P1.Y + u * (l1P2.Y - l1P1.Y)));
        }

        public static bool LineContainsRectangle(Vector2 p1, Vector2 p2, RectangleF rectangle)
        {
            return rectangle.Contains(p1) && rectangle.Contains(p2);
        }

        public static bool Contains(this RectangleF rectangle, Vector2 p1, Vector2 p2)
        {
            return LineContainsRectangle(p1, p2, rectangle);
        }
    }
}
