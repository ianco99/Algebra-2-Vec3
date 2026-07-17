using System;

namespace CustomMath
{
    public struct MyPlane : IEquatable<MyPlane>
    {
        #region Variables
        
        public Vec3 normal;
        
        public float distance;  //

        #endregion

        #region Propiedades

        public MyPlane flipped => new MyPlane(-normal, -distance);

        #endregion

        #region Constructores

        public MyPlane(Vec3 inNormal, Vec3 inPoint)
        {
            normal = Vec3.Normalize(inNormal);
            distance = -Vec3.Dot(normal, inPoint);
        }

        public MyPlane(Vec3 inNormal, float inDistance)
        {
            normal = Vec3.Normalize(inNormal);
            distance = inDistance;
        }

        public MyPlane(Vec3 a, Vec3 b, Vec3 c)
        {
            normal = Vec3.Normalize(Vec3.Cross(b - a, c - a));
            distance = -Vec3.Dot(normal, a);
        }

        #endregion

        #region Operadores

        public static bool operator ==(MyPlane lhs, MyPlane rhs)
        {
            return lhs.normal == rhs.normal && Math.Abs(lhs.distance - rhs.distance) < Vec3.epsilon;
        }

        public static bool operator !=(MyPlane lhs, MyPlane rhs)
        {
            return !(lhs == rhs);
        }

        #endregion

        #region Métodos Estáticos

        public static MyPlane Translate(MyPlane plane, Vec3 translation)
        {
            return new MyPlane(plane.normal, plane.distance - Vec3.Dot(plane.normal, translation));
        }

        #endregion

        #region Métodos de Instancia

        public Vec3 ClosestPointOnPlane(Vec3 point)
        {
            float dist = GetDistanceToPoint(point);
            return point - (normal * dist);
        }

        public void Flip()
        {
            normal = -normal;
            distance = -distance;
        }

        public float GetDistanceToPoint(Vec3 point)
        {
            return Vec3.Dot(normal, point) + distance;
        }

        public bool GetSide(Vec3 point)
        {
            return GetDistanceToPoint(point) > 0.0f;
        }

        public bool SameSide(Vec3 inPt0, Vec3 inPt1)
        {
            float dist0 = GetDistanceToPoint(inPt0);
            float dist1 = GetDistanceToPoint(inPt1);
            return (dist0 > 0.0f && dist1 > 0.0f) || (dist0 <= 0.0f && dist1 <= 0.0f);
        }

        public void Set3Points(Vec3 a, Vec3 b, Vec3 c)
        {
            normal = Vec3.Normalize(Vec3.Cross(b - a, c - a));
            distance = -Vec3.Dot(normal, a);
        }

        public void SetNormalAndPosition(Vec3 inNormal, Vec3 inPoint)
        {
            normal = Vec3.Normalize(inNormal);
            distance = -Vec3.Dot(normal, inPoint);
        }

        public void Translate(Vec3 translation)
        {
            distance -= Vec3.Dot(normal, translation);
        }

        #endregion

        #region Internals (Overrides)

        public override bool Equals(object other)
        {
            if (!(other is MyPlane)) return false;
            return Equals((MyPlane)other);
        }

        public bool Equals(MyPlane other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return normal.GetHashCode() ^ (distance.GetHashCode() << 2);
        }

        public override string ToString()
        {
            return $"(normal: {normal}, distance: {distance})";
        }

        #endregion
    }
}