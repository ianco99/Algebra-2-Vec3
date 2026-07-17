using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CustomMath
{
	public struct Vec3 : IEquatable<Vec3>
	{
		#region Variables

		public float x;
		public float y;
		public float z;

		public float sqrMagnitude => SqrMagnitude(this);

		public Vec3 normalized => Vec3.Normalize(this);

		/// <summary>
		/// El largo del vector.
		/// </summary>
		public float magnitude => Magnitude(this);

		#endregion

		#region constants

		public const float epsilon = 1e-05f;

		#endregion

		#region Default Values

		public static Vec3 Zero
		{
			get { return new Vec3(0.0f, 0.0f, 0.0f); }
		}

		public static Vec3 One
		{
			get { return new Vec3(1.0f, 1.0f, 1.0f); }
		}

		public static Vec3 Forward
		{
			get { return new Vec3(0.0f, 0.0f, 1.0f); }
		}

		public static Vec3 Back
		{
			get { return new Vec3(0.0f, 0.0f, -1.0f); }
		}

		public static Vec3 Right
		{
			get { return new Vec3(1.0f, 0.0f, 0.0f); }
		}

		public static Vec3 Left
		{
			get { return new Vec3(-1.0f, 0.0f, 0.0f); }
		}

		public static Vec3 Up
		{
			get { return new Vec3(0.0f, 1.0f, 0.0f); }
		}

		public static Vec3 Down
		{
			get { return new Vec3(0.0f, -1.0f, 0.0f); }
		}

		public static Vec3 PositiveInfinity
		{
			get { return new Vec3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity); }
		}

		public static Vec3 NegativeInfinity
		{
			get { return new Vec3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity); }
		}

		#endregion

		#region Constructors

		public Vec3(float x, float y)
		{
			this.x = x;
			this.y = y;
			this.z = 0.0f;
		}

		public Vec3(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public Vec3(Vec3 v3)
		{
			this.x = v3.x;
			this.y = v3.y;
			this.z = v3.z;
		}

		public Vec3(Vector3 v3)
		{
			this.x = v3.x;
			this.y = v3.y;
			this.z = v3.z;
		}

		public Vec3(Vector2 v2)
		{
			this.x = v2.x;
			this.y = v2.y;
			this.z = 0.0f;
		}

		#endregion

		#region Operators

		public static bool operator ==(Vec3 left, Vec3 right)
		{
			float diff_x = left.x - right.x;
			float diff_y = left.y - right.y;
			float diff_z = left.z - right.z;
			float sqrmag = diff_x * diff_x + diff_y * diff_y + diff_z * diff_z;
			return sqrmag < epsilon * epsilon;
		}

		public static bool operator !=(Vec3 left, Vec3 right)
		{
			return !(left == right);
		}

		public static Vec3 operator +(Vec3 leftV3, Vec3 rightV3)
		{
			return new Vec3(leftV3.x + rightV3.x, leftV3.y + rightV3.y, leftV3.z + rightV3.z);
		}

		public static Vec3 operator -(Vec3 leftV3, Vec3 rightV3)
		{
			return new Vec3(leftV3.x - rightV3.x, leftV3.y - rightV3.y, leftV3.z - rightV3.z);
		}

		public static Vec3 operator -(Vec3 v3)
		{
			return new Vec3(-v3.x, -v3.y, -v3.z);
		}

		public static Vec3 operator *(Vec3 v3, float scalar)
		{
			return new Vec3(v3.x * scalar, v3.y * scalar, v3.z * scalar);
		}

		public static Vec3 operator *(float scalar, Vec3 v3)
		{
			return new Vec3(v3.x * scalar, v3.y * scalar, v3.z * scalar);
		}

		public static Vec3 operator /(Vec3 v3, float scalar)
		{
			return new Vec3(v3.x / scalar, v3.y / scalar, v3.z / scalar);
		}

		public static implicit operator Vector3(Vec3 v3)
		{
			return new Vector3(v3.x, v3.y, v3.z);
		}

		public static implicit operator Vector2(Vec3 v2)
		{
			return new Vector2(v2.x, v2.y);
		}

		public static Vec3 operator *(Quaternion rotation, Vec3 point)
		{
			float num1 = rotation.x * 2f;
			float num2 = rotation.y * 2f;
			float num3 = rotation.z * 2f;
			float num4 = rotation.x * num1;
			float num5 = rotation.y * num2;
			float num6 = rotation.z * num3;
			float num7 = rotation.x * num2;
			float num8 = rotation.x * num3;
			float num9 = rotation.y * num3;
			float num10 = rotation.w * num1;
			float num11 = rotation.w * num2;
			float num12 = rotation.w * num3;
			Vec3 vector3;
			vector3.x = (float)((1.0 - ((double)num5 + (double)num6)) * (double)point.x +
			                    ((double)num7 - (double)num12) * (double)point.y +
			                    ((double)num8 + (double)num11) * (double)point.z);
			vector3.y = (float)(((double)num7 + (double)num12) * (double)point.x +
			                    (1.0 - ((double)num4 + (double)num6)) * (double)point.y +
			                    ((double)num9 - (double)num10) * (double)point.z);
			vector3.z = (float)(((double)num8 - (double)num11) * (double)point.x +
			                    ((double)num9 + (double)num10) * (double)point.y +
			                    (1.0 - ((double)num4 + (double)num5)) * (double)point.z);
			return vector3;
		}

		#endregion

		#region Functions

		public override string ToString()
		{
			return "X = " + x.ToString() + "   Y = " + y.ToString() + "   Z = " + z.ToString();
		}

		/// <summary>
		/// Devuelve el ángulo entre ambos vectores.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static float Angle(Vec3 from, Vec3 to)
		{
			float dot = Dot(from, to);
			float magProduct = from.magnitude * to.magnitude;

			if (magProduct < epsilon) return 0f;

			float cos = Math.Clamp(dot / magProduct, -1.0f, 1.0f);//por errores de punto flotante

			return (float)Math.Acos(cos) * (180.0f / (float)Math.PI);
		}

		/// <summary>
		/// Devuelve un vector de igual dirección pero longitud limitada por un parámetro.
		/// </summary>
		/// <param name="vector"></param>
		/// <param name="maxLength"></param>
		/// <returns></returns>
		public static Vec3 ClampMagnitude(Vec3 vector, float maxLength)
		{
			float sqrMag = vector.sqrMagnitude;

			if (sqrMag > maxLength * maxLength)
			{
				float mag = (float)Math.Sqrt(sqrMag);

				float normalizedX = vector.x / mag;
				float normalizedY = vector.y / mag;
				float normalizedZ = vector.z / mag;

				return new Vec3(
					normalizedX * maxLength,
					normalizedY * maxLength,
					normalizedZ * maxLength
				);
			}

			return vector;
		}

		/// <summary>
		/// Devuelve la magnitud de un vector dado como parámetro.
		/// </summary>
		/// <param name="vector"></param>
		/// <returns></returns>
		public static float Magnitude(Vec3 vector)
		{
			return MathF.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
		}

		/// <summary>
		/// Devuelve un vector3 perpendicular a ambos vectores pasados como parámetros.
		/// Aislamos cada resultado del eje correspondiente para garantizar los 90 grados.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Vec3 Cross(Vec3 a, Vec3 b)
		{
			float x = (a.y * b.z) - (a.z * b.y);
			float y = (a.z * b.x) - (a.x * b.z);
			float z = (a.x * b.y) - (a.y * b.x);
			return new Vec3(x, y, z);
		}

		/// <summary>
		/// Devuelve la distancia absoluta entre dos puntos en el espacio.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static float Distance(Vec3 a, Vec3 b)
		{
			float diffX = a.x - b.x;
			float diffY = a.y - b.y;
			float diffZ = a.z - b.z;

			return (float)Math.Sqrt(diffX * diffX + diffY * diffY + diffZ * diffZ);
		}

		/// <summary>
		/// Devuelve el resultado de proyectar un vector sobre otro.
		/// En dos vectores unitarios, se toma a un vector como la base canónica de otro, y se obtiene el porcentaje
		/// de ese vector que comparte con la base canónica.
		/// Un resultado alternativo de esta operación, también con vectores unitarios, es el coseno del ángulo entre
		/// ambos vectores.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static float Dot(Vec3 a, Vec3 b)
		{
			return (a.x * b.x) + (a.y * b.y) + (a.z * b.z);
		}

		/// <summary>
		/// Devuelve un punto en el espacio resultado de una interpolación lineal entre dos puntos, dada por un valor
		/// de 0 a 1 llámese T.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public static Vec3 Lerp(Vec3 a, Vec3 b, float t)
		{
			t = Math.Clamp(t, 0.0f, 1.0f);

			return new Vec3(
				a.x + (b.x - a.x) * t,
				a.y + (b.y - a.y) * t,
				a.z + (b.z - a.z) * t
			);
		}

		/// <summary>
		/// Devuelve un punto en el espacio resultado de una interpolación (o extrapolación) lineal entre dos puntos,
		/// dada por un valor T.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public static Vec3 LerpUnclamped(Vec3 a, Vec3 b, float t)
		{
			return new Vec3(
				a.x + (b.x - a.x) * t,
				a.y + (b.y - a.y) * t,
				a.z + (b.z - a.z) * t
			);
		}

		/// <summary>
		/// Devuelve un vector con todos los componentes máximos entre dos vectores.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Vec3 Max(Vec3 a, Vec3 b)
		{
			return new Vec3(
				Math.Max(a.x, b.x),
				Math.Max(a.y, b.y),
				Math.Max(a.z, b.z)
			);
		}

		/// <summary>
		/// Devuelve un vector con todos los componente mínimos entre dos vectores.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Vec3 Min(Vec3 a, Vec3 b)
		{
			return new Vec3(
				Math.Min(a.x, b.x),
				Math.Min(a.y, b.y),
				Math.Min(a.z, b.z)
			);
		}

		/// <summary>
		/// Devuelve la magnitud al cuadrado de un vector.
		/// </summary>
		/// <param name="vector"></param>
		/// <returns></returns>
		public static float SqrMagnitude(Vec3 vector)
		{
			return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
		}

		public static Vec3 Project(Vec3 vector, Vec3 onNormal)
		{
			float sqrMag = SqrMagnitude(onNormal);

			if (sqrMag < epsilon)
			{
				return Zero;
			}

			float dot = Dot(vector, onNormal);

			return new Vec3(
				(onNormal.x * dot) / sqrMag,
				(onNormal.y * dot) / sqrMag,
				(onNormal.z * dot) / sqrMag
			);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="inDirection"></param>
		/// <param name="inNormal"></param>
		/// <returns></returns>
		public static Vec3 Reflect(Vec3 inDirection, Vec3 inNormal)
		{
			float factor = -2.0f * Dot(inDirection, inNormal);

			return new Vec3(
				inDirection.x + (inNormal.x * factor),
				inDirection.y + (inNormal.y * factor),
				inDirection.z + (inNormal.z * factor)
			);
		}

		/// <summary>
		/// Normaliza un vector dado como parámetro.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Vec3 Normalize(Vec3 value)
		{
			float mag = Magnitude(value);

			if (mag > 0.00001f)
			{
				value.x /= mag;
				value.y /= mag;
				value.z /= mag;
			}
			else
			{
				value.x = 0;
				value.y = 0;
				value.z = 0;
			}

			return value;
		}

		/// <summary>
		/// Establece los valores del vector instanciado.
		/// </summary>
		/// <param name="newX"></param>
		/// <param name="newY"></param>
		/// <param name="newZ"></param>
		public void Set(float newX, float newY, float newZ)
		{
			x = newX;
			y = newY;
			z = newZ;
		}

		/// <summary>
		/// Escala un vector por componentes de otro
		/// </summary>
		/// <param name="scale"></param>
		public void Scale(Vec3 scale)
		{
			this.x *= scale.x;
			this.y *= scale.y;
			this.z *= scale.z;
		}

		/// <summary>
		/// Devuelve un vector unitario
		/// </summary>
		public void Normalize()
		{
			float mag = magnitude;

			if (mag > 0.00001f)
			{
				this.x /= mag;
				this.y /= mag;
				this.z /= mag;
			}
			else
			{
				this.x = 0;
				this.y = 0;
				this.z = 0;
			}
		}

		#endregion

		#region Internals

		/// <summary>
		/// Devuelve un booleano que indica si el objeto pasado como parámetro
		/// es un vector equivalente al vector instanciado.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public override bool Equals(object other)
		{
			if (!(other is Vec3)) return false;
			return Equals((Vec3)other);
		}

		/// <summary>
		/// Devuelve un booleano que indica si el vector instanciado es igual, componente por componente, al vector
		/// pasado como parámetro.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(Vec3 other)
		{
			return x == other.x && y == other.y && z == other.z;
		}

		/// <summary>
		/// Devuelve un hashcode equivalente al vector instanciado.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
		}

		#endregion
	}
}