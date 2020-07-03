using SharpDX;

namespace Rml.SharpDx
{
    public static class PlaneHelper
    {
        public static bool ProjectPlane(this ref Plane plane, Vector3 point, Vector3 projectDirection, out Vector3 projectPoint)
        {
            var rayPlus = new Ray(point, projectDirection);
            var rayMinus = new Ray(point, -projectDirection);

            if (rayPlus.Intersects(ref plane, out projectPoint))
                return true;
                    
            if (rayMinus.Intersects(ref plane, out projectPoint))
                return true;

            return false;
        }
    }
}