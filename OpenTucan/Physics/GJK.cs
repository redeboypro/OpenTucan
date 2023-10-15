using OpenTK;
using OpenTucan.Entities;

namespace OpenTucan.Physics
{
    public static class GJK
    {
        public static bool Intersects(ref (Rigidbody rigidbody, ConvexShape shape) a, ref (Rigidbody rigidbody, ConvexShape shape) b, out Simplex points)
        {
            var support = Support(ref a, ref b, Vector3.UnitX);

            points = new Simplex();
            points.PushFront(support);

            var direction = -support;

            while (true)
            {
                support = Support(ref a, ref b, direction);

                if (Vector3.Dot(support, direction) <= 0)
                {
                    return false;
                }
                points.PushFront(support);

                if (NextSimplex(ref points, direction, out direction))
                {
                    return true;
                }
            }
        }

        private static bool NextSimplex(ref Simplex points, Vector3 inDirection, out Vector3 outDirection)
        {
            outDirection = inDirection;
            
            switch (points.Size)
            {
                case 2:
                    return Line(ref points, out outDirection);
                case 3:
                    return Triangle(ref points, out outDirection);
                case 4:
                    return Tetrahedron(ref points, inDirection, out outDirection);
            }

            return false;
        }
        
        public static bool SameDirection(Vector3 direction, Vector3 ao)
        {
            return Vector3.Dot(direction, ao) > 0;
        }

        private static bool Line(ref Simplex points, out Vector3 direction)
        {
            var a = points[0];
            var b = points[1];

            var ab = b - a;
            var ao = -a;

            if (SameDirection(ab, ao))
            {
                direction = Vector3.Cross(Vector3.Cross(ab, ao), ab);
            }
            else
            {
                points.InitializeFromList(a);
                direction = ao;
            }

            return false;
        }
        
        private static bool Triangle(ref Simplex points, out Vector3 direction)
        {
            var a = points[0];
            var b = points[1];
            var c = points[2];

            var ab = b - a;
            var ac = c - a;
            var ao = -a;
 
            var abc = Vector3.Cross(ab, ac);
 
            if (SameDirection(Vector3.Cross(abc, ac), ao)) 
            {
                if (SameDirection(ac, ao)) 
                {
                    points.InitializeFromList(a, c);
                    direction = Vector3.Cross(Vector3.Cross(ac, ao), ac);
                }

                else {
                    points.InitializeFromList(a, b);
                    return Line(ref points, out direction);
                }
            }
            else
            {
                if (SameDirection(Vector3.Cross(ab, abc), ao))
                {
                    points.InitializeFromList(a, b);
                    return Line(ref points, out direction);
                }

                if (SameDirection(abc, ao)) 
                {
                    direction = abc;
                }
                else {
                    points.InitializeFromList(a, c, b);
                    direction = -abc;
                }
            }

            return false;
        }
        
        private static bool Tetrahedron(ref Simplex points, Vector3 inDirection, out Vector3 outDirection)
        {
            outDirection = inDirection;
            
            var a = points[0];
            var b = points[1];
            var c = points[2];
            var d = points[3];

            var ab = b - a;
            var ac = c - a;
            var ad = d - a;
            var ao =   - a;
 
            var abc = Vector3.Cross(ab, ac);
            var acd = Vector3.Cross(ac, ad);
            var adb = Vector3.Cross(ad, ab);
 
            if (SameDirection(abc, ao)) 
            {
                points.InitializeFromList(a, b, c);
                return Triangle(ref points, out outDirection);
            }
		
            if (SameDirection(acd, ao)) 
            {
                points.InitializeFromList(a, c, d);
                return Triangle(ref points, out outDirection);
            }
 
            if (SameDirection(adb, ao)) 
            {
                points.InitializeFromList(a, d, b);
                return Triangle(ref points, out outDirection);
            }
 
            return true;
        }
        
        public static Vector3 Support(ref (Rigidbody rigidbody, ConvexShape shape) a, ref (Rigidbody rigidbody, ConvexShape shape) b, Vector3 direction)
        {
            return a.shape.FindFurthestPoint(a.rigidbody, direction) - b.shape.FindFurthestPoint(b.rigidbody, -direction);
        }
    }
}