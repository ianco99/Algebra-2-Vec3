using System;

namespace CustomMath
{
	// normal ⋅ P + distance = 0
	public struct MyPlane : IEquatable<MyPlane>
	{
		#region Variables

		public Vec3 normal;

		public float distance; //

		#endregion

		#region Propiedades

		public MyPlane flipped => new MyPlane(-normal, -distance);

		#endregion

		#region Constructores

		/// <summary>
		/// Devuelve un plano que pasa por el punto proporcionado y tiene la normal pasada como parámetro.
		/// </summary>
		/// <param name="inNormal"></param>
		/// <param name="inPoint"></param>
		public MyPlane(Vec3 inNormal, Vec3 inPoint)
		{
			normal = Vec3.Normalize(inNormal);
			distance = -Vec3.Dot(normal, inPoint);
		}

		/// <summary>
		/// Devuelve un plano que se encuentra a una distancia pasada como parametro del origen, y que apunta a la normal pasada como parámetro.
		/// </summary>
		/// <param name="inNormal"></param>
		/// <param name="inDistance"></param>
		public MyPlane(Vec3 inNormal, float inDistance)
		{
			normal = Vec3.Normalize(inNormal);
			distance = inDistance;
		}

		/// <summary>
		/// Devuelve un plano que pasa por los tres puntos proporcionados como parámetros.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="c"></param>
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

		/// <summary>
		/// Devuelve un plano desplazado por un vector de translación.
		/// </summary>
		/// <param name="plane"></param>
		/// <param name="translation"></param>
		/// <returns></returns>
		public static MyPlane Translate(MyPlane plane, Vec3 translation)
		{
			return new MyPlane(plane.normal, plane.distance - Vec3.Dot(plane.normal, translation));
		}

		#endregion

		#region Métodos de Instancia

		/// <summary>
		/// Devuelve un vector distancia del punto pasado como parámetro a la definición del plano (punto más cercano al parámetro, dentro del plano).
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public Vec3 ClosestPointOnPlane(Vec3 point)
		{
			float dist = GetDistanceToPoint(point);
			return point - (normal * dist);
		}

		/// <summary>
		/// Niega los componentes del plano instanciado.
		/// </summary>
		public void Flip()
		{
			normal = -normal;
			distance = -distance;
		}

		/// <summary>
		/// Devuelve la distancia en metros a la que se encuentra un punto al plano instanciado.
		/// Utilizando la ecuación (sin igualar a cero)  obtenemos la distancia entre el punto a evaluar y el
		/// plano instanciado.
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public float GetDistanceToPoint(Vec3 point)
		{
			return Vec3.Dot(normal, point) + distance;
		}

		/// <summary>
		/// Devuelve la lectura del signo de la distancia del plano instanciado al punto para saber de que lado
		/// del plano se encuentra el punto.
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public bool GetSide(Vec3 point)
		{
			return GetDistanceToPoint(point) > 0.0f;
		}

		/// <summary>
		/// Devuelve un booleano que indica si ambos puntos estan del mismo lado de un plano.
		/// </summary>
		/// <param name="inPt0"></param>
		/// <param name="inPt1"></param>
		/// <returns></returns>
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