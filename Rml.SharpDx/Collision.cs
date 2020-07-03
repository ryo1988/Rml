using SharpDX;

namespace Rml.SharpDx
{
    public static class Collision
    {
        public static bool LineIntersectsRectangle(Vector2 p1, Vector2 p2, RectangleF rectangle)
        {
            return LineIntersectsLine(p1, p2, new Vector2(rectangle.X, rectangle.Y), new Vector2(rectangle.X + rectangle.Width, rectangle.Y)) ||
                   LineIntersectsLine(p1, p2, new Vector2(rectangle.X + rectangle.Width, rectangle.Y),
                       new Vector2(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height)) ||
                   LineIntersectsLine(p1, p2, new Vector2(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height),
                       new Vector2(rectangle.X, rectangle.Y + rectangle.Height)) ||
                   LineIntersectsLine(p1, p2, new Vector2(rectangle.X, rectangle.Y + rectangle.Height), new Vector2(rectangle.X, rectangle.Y)) ||
                   LineContainsRectangle(p1, p2, rectangle);
        }

        public static bool LineIntersectsLine(Vector2 l1P1, Vector2 l1P2, Vector2 l2P1, Vector2 l2P2)
        {
            var q = (l1P1.Y - l2P1.Y) * (l2P2.X - l2P1.X) - (l1P1.X - l2P1.X) * (l2P2.Y - l2P1.Y);
            var d = (l1P2.X - l1P1.X) * (l2P2.Y - l2P1.Y) - (l1P2.Y - l1P1.Y) * (l2P2.X - l2P1.X);

            if (System.Math.Abs(d) < float.Epsilon)
            {
                return false;
            }

            var r = q / d;

            q = (l1P1.Y - l2P1.Y) * (l1P2.X - l1P1.X) - (l1P1.X - l2P1.X) * (l1P2.Y - l1P1.Y);
            var s = q / d;

            return !(r < 0) && !(r > 1) && !(s < 0) && !(s > 1);
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
