using System.ComponentModel;
using CustomMath;
using UnityEngine;

// Capaz lo normalizo en un getter
public struct Quat
{
	public float x;
	public float y;
	public float z;
	public float w;

	public static Quat identity
	{
		get { return new Quat(0f, 0f, 0f, 1f); }
	}

	public Vec3 eulerAngles
	{
		get { return ToEulerAngles(this); }
	}

	public Quat normalized => Normalize(this);

	public Quat(float x, float y, float z, float w)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}

	public Quat(Quat q)
	{
		this.x = q.x;
		this.y = q.y;
		this.z = q.z;
		this.w = q.w;
	}

	public Quat(Quaternion q)
	{
		this.x = q.x;
		this.y = q.y;
		this.z = q.z;
		this.w = q.w;
	}

	public void Normalize()
	{
		Quat normalized = Normalize(this);

		x = normalized.x;
		y = normalized.y;
		z = normalized.z;
		w = normalized.w;
	}

	public void Set(float newX, float newY, float newZ, float newW)
	{
		x = newX;
		y = newY;
		z = newZ;
		w = newW;
	}

	public void SetAxisAngle(Vec3 axis, float angle)
	{
		Quat result = AngleAxis(angle, axis);
		Set(result.x, result.y, result.z, result.w);
	}

	public void SetEulerAngles(Vec3 euler)
	{
		Quat result = Euler(euler);
		Set(result.x, result.y, result.z, result.w);
	}

	public void SetEulerRotation(Vec3 euler)
	{
		Quat result = EulerRotation(euler);
		Set(result.x, result.y, result.z, result.w);
	}

	public void SetFromToRotation(Vec3 fromDirection, Vec3 toDirection)
	{
		Quat result = FromToRotation(fromDirection, toDirection);
		Set(result.x, result.y, result.z, result.w);
	}

	public void SetLookRotation(Vec3 view)
	{
		Quat result = LookRotation(view);
		Set(result.x, result.y, result.z, result.w);
	}

	public void SetLookRotation(Vec3 view, [DefaultValue("Vec3.up")] Vec3 up)
	{
		Quat result = LookRotation(view, up);
		Set(result.x, result.y, result.z, result.w);
	}

	public static bool operator ==(Quat lhs, Quat rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z && lhs.w == rhs.w;
	}

	public static bool operator !=(Quat lhs, Quat rhs)
	{
		return !(lhs == rhs);
	}

	public static Vec3 operator *(Quat rotation, Vec3 point)
	{
		// q * v * q-1
		// q * v = v' (v con w modificado)
		// v' * q-1 = v'' (v con w intacto)
		Quat pure = new Quat(point.x, point.y, point.z, 0f);
		Quat result = rotation * pure * Conjugate(rotation);

		return new Vec3(result.x, result.y, result.z);
	}

	// w + xi + yj + zk
	//
	// i = sqrt(-1)
	// i * i = -1
	//
	// j = sqrt(-1)
	// j * j = -1
	//
	// k = sqrt(-1)
	// k * k = -1
	//
	// i * j = k		j * k = i		k * i = j
	//
	// j * i = -k		k * j = -i			i * k = -j
	//
	// Distributiva agrupando terminos en base a los signos. Agrupados en w las cuentas que resultan en numeros reales
	// Agrupados en x las cuentas que dan numeros con i
	// Agrupados en y las cuentas que dan numeros con j
	// Agrupados en z las cuentas que dan numeros con k
	public static Quat operator *(Quat lhs, Quat rhs)
	{
		float w = lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z;
		float x = lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y;
		float y = lhs.w * rhs.y - lhs.x * rhs.z + lhs.y * rhs.w + lhs.z * rhs.x;
		float z = lhs.w * rhs.z + lhs.x * rhs.y - lhs.y * rhs.x + lhs.z * rhs.w;

		return new Quat(x, y, z, w);
	}

	public static implicit operator Quaternion(Quat q)
	{
		return new Quaternion(q.x, q.y, q.z, q.w);
	}

	// Angulo entre dos quaternions
	// Toma dos quaternions unitarios
	/// <summary>
	/// Dot (a, b) = |a| * |b| * cos(θ)
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns></returns>
	public static float Angle(Quat a, Quat b)
	{
		float dotProduct = Dot(a, b); //Dot product de dos quaterniones unitarios devuelve cos(θ)
		float clampedDot = Mathf.Clamp(dotProduct, -1f, 1f); //Clampeamos por posibilidad de error de punto flotante
		float angleRad =
			Mathf.Acos(Mathf.Abs(clampedDot)) * 2f; //Acos de cos = θ en radianes, duplicado para ver el ángulo real

		return angleRad * Mathf.Rad2Deg;
	}

	// Crea un quaternion que rota N grados en un eje
	//
	// Creo un quaternion con LA MITAD de la rotación para que, al multiplicarse con un vector, aplique la rotación
	// intencionada
	public static Quat AngleAxis(float angle, Vec3 axis)
	{
		Vec3 normAxis = axis.normalized; //normalizar eje
		float halfRad = angle * Mathf.Deg2Rad * 0.5f; //Pasar a radianes y conseguir mitad de ángulo
		float sin = Mathf.Sin(halfRad);

		return new Quat(normAxis.x * sin, normAxis.y * sin, normAxis.z * sin, Mathf.Cos(halfRad));
	}

	// Producto punto. Multiplicar todos los componentes entre sí
	public static float Dot(Quat a, Quat b)
	{
		return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
	}

	// 3 quaterniones. uno rota en z, otro en Y, otro en X
	public static Quat Euler(Vec3 euler)
	{
		float x = euler.x * Mathf.Deg2Rad * 0.5f;
		float y = euler.y * Mathf.Deg2Rad * 0.5f;
		float z = euler.z * Mathf.Deg2Rad * 0.5f;

		float cx = Mathf.Cos(x);
		float sx = Mathf.Sin(x);
		float cy = Mathf.Cos(y);
		float sy = Mathf.Sin(y);
		float cz = Mathf.Cos(z);
		float sz = Mathf.Sin(z);

		Quat qx = new Quat(sx, 0, 0, cx);
		Quat qy = new Quat(0, sy, 0, cy);
		Quat qz = new Quat(0, 0, sz, cz);

		return qz * qx * qy;
	}

	public static Quat EulerAngles(Vec3 euler)
	{
		return Euler(euler);
	}

	// Toma vectores en radianes en vez de grados
	public static Quat EulerRotation(Vec3 euler)
	{
		float x = euler.x * 0.5f;
		float y = euler.y * 0.5f;
		float z = euler.z * 0.5f;

		float cx = Mathf.Cos(x);
		float sx = Mathf.Sin(x);
		float cy = Mathf.Cos(y);
		float sy = Mathf.Sin(y);
		float cz = Mathf.Cos(z);
		float sz = Mathf.Sin(z);

		Quat qx = new Quat(sx, 0, 0, cx);
		Quat qy = new Quat(0, sy, 0, cy);
		Quat qz = new Quat(0, 0, sz, cz);

		return qz * qx * qy;
	}

	/// <summary>
	/// La rotación que lleva a un vector de dirección a otro.
	/// </summary>
	/// <param name="fromDirection"></param>
	/// <param name="toDirection"></param>
	/// <returns></returns>
	public static Quat FromToRotation(Vec3 fromDirection, Vec3 toDirection)
	{
		Vec3 from = fromDirection.normalized;
		Vec3 to = toDirection.normalized;

		float dot = Vec3.Dot(from, to);

		if (dot >= 1f - Mathf.Epsilon) //El ángulo entre ellos es muy chico
			return identity;

		if (dot <= -1f + Mathf.Epsilon) //El angulo entre ellos es 180°
		{
			Vec3 axis = Vec3.Cross(Vec3.Right, from); //Pruebo un eje arbitrario (porque podrían ser infinitos los ejes a elegir entre dos vectores opuestos)

			if (axis.magnitude < Mathf.Epsilon)
				axis = Vec3.Cross(Vec3.Up, from);

			return AngleAxis(180f, axis.normalized); //Rotar 180 grados en el eje obtenido
		}

		Vec3 rotationAxis = Vec3.Cross(from, to);
		float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

		return AngleAxis(angle, rotationAxis); //Rotar n grados en el eje obtenido
	}

	public static Quat Conjugate(Quat rotation)
	{
		float newX = -rotation.x;
		float newY = -rotation.y;
		float newZ = -rotation.z;

		return new Quat(newX, newY, newZ, rotation.w);
	}

	// La rotación opuesta a la representada
	// Conjugado: mismo quaternion con las tres partes imaginarias negadas
	// q * inversa(q) = 1
	// inversa = conjugado(q)/mag(q)^2
	//
	// 1 = q * conjugado(q)/mag(q)^2
	//
	// magnitud seria sqrt(w*w x*x y*y z*z)
	// magnitud^2 es w*w x*x y*y z*z
	public static Quat Inverse(Quat rotation)
	{
		float magSq = rotation.w * rotation.w + rotation.x * rotation.x + rotation.y * rotation.y +
		              rotation.z * rotation.z;
		Quat conjugate = Conjugate(rotation);

		return new Quat(conjugate.x / magSq, conjugate.y / magSq, conjugate.z / magSq, conjugate.w / magSq);
	}

	public static Quat Lerp(Quat a, Quat b, float t)
	{
		return LerpUnclamped(a, b, Mathf.Clamp01(t));
	}

	public static Quat LerpUnclamped(Quat a, Quat b, float t)
	{
		Quat result;

		if (Dot(a, b) < 0f) // camino largo o camino corto: ¿interpolo hacia b o hacia -b?
		{
			result = new Quat(
				a.x + t * (-b.x - a.x),
				a.y + t * (-b.y - a.y),
				a.z + t * (-b.z - a.z),
				a.w + t * (-b.w - a.w)
			);
		}
		else
		{
			result = new Quat(
				a.x + t * (b.x - a.x),
				a.y + t * (b.y - a.y),
				a.z + t * (b.z - a.z),
				a.w + t * (b.w - a.w)
			);
		}

		return Normalize(result);
	}

	// Genera una rotación que deje al objeto mirando al forward argumento
	public static Quat LookRotation(Vec3 forward, [DefaultValue("Vec3.Up")] Vec3 upwards)
	{
		forward = forward.normalized; //normalizo forward

		Vec3 right = Vec3.Cross(upwards, forward).normalized; //Obtengo dirección de la derecha con cross de adelante y arriba
		Vec3 up = Vec3.Cross(forward, right); //Lo recalculo para que sea exacto perpendicular

		// Construir matriz de rotación
		//
		// ┌                                            ┐
		// │ 1-2(y²+z²)    2(xy-wz)      2(xz+wy)    │
		// │ 2(xy+wz)      1-2(x²+z²)    2(yz-wx)    │
		// │ 2(xz-wy)      2(yz+wx)      1-2(x²+y²)  │
		// └                                            ┘
		//
		//               Right      Up         Forward
		float m00 = right.x, m01 = up.x, m02 = forward.x;
		float m10 = right.y, m11 = up.y, m12 = forward.y;
		float m20 = right.z, m21 = up.z, m22 = forward.z;

		float trace = m00 + m11 + m22;
		Quat q = new Quat();

		// Método de Shepperd
		// Busco el numero más grande por el cual divido para obtener un resultado más estable y preciso
		if (trace > 0f)
		{
			float s = Mathf.Sqrt(trace + 1f) * 2f;
			q.w = 0.25f * s;
			q.x = (m21 - m12) / s;
			q.y = (m02 - m20) / s;
			q.z = (m10 - m01) / s;
		}
		else if (m00 > m11 && m00 > m22)
		{
			float s = Mathf.Sqrt(1f + m00 - m11 - m22) * 2f;
			q.w = (m21 - m12) / s;
			q.x = 0.25f * s;
			q.y = (m01 + m10) / s;
			q.z = (m02 + m20) / s;
		}
		else if (m11 > m22)
		{
			float s = Mathf.Sqrt(1f + m11 - m00 - m22) * 2f;
			q.w = (m02 - m20) / s;
			q.x = (m01 + m10) / s;
			q.y = 0.25f * s;
			q.z = (m12 + m21) / s;
		}
		else
		{
			float s = Mathf.Sqrt(1f + m22 - m00 - m11) * 2f;
			q.w = (m10 - m01) / s;
			q.x = (m02 + m20) / s;
			q.y = (m12 + m21) / s;
			q.z = 0.25f * s;
		}

		return q;
	}

	public static Quat LookRotation(Vec3 forward)
	{
		return LookRotation(forward, Vec3.Up);
	}

	public static Quat Normalize(Quat q)
	{
		// Hacer "Dot" es lo mismo que hacer x*x + y*y + z*z + w*w
		// Pitagoras de 4 elementos sqrt(x2 + y2 + z2 + w2)
		float mag = Mathf.Sqrt(Dot(q, q));

		// Si la magnitud es minima, normalizar a identidad
		if (mag < Mathf.Epsilon)
			return identity;

		// Componentes del Quaternion divididos por su magnitud
		return new Quat(q.x / mag, q.y / mag, q.z / mag, q.w / mag);
	}

	// Max degrees delta: Step
	// maxDegreesDelta = 90 * Time.deltaTime
	public static Quat RotateTowards(Quat from, Quat to, float maxDegreesDelta)
	{
		float angle = Angle(from, to);

		if (angle == 0f)
			return to;

		float t = Mathf.Min(1f, maxDegreesDelta / angle); //100 / 25 = 0.25. t = 0.25 o 1/4 de camino

		return SlerpUnclamped(from, to, t);
	}

	public static Quat Slerp(Quat a, Quat b, float t)
	{
		return SlerpUnclamped(a, b, Mathf.Clamp01(t));
	}

	// Spherical lerp, siguiendo un arco guiado por la función seno en base
	// al ángulo entre ambos quaterniones
	public static Quat SlerpUnclamped(Quat a, Quat b, float t)
	{
		float dot = Dot(a, b);

		if (dot < 0f) // camino largo o camino corto para la rotación?
		{
			b = new Quat(-b.x, -b.y, -b.z, -b.w);
			dot = -dot; // necesitamos que el dot sea positivo para el acos
		}

		if (dot > 0.9995f) // si los quaterniones son muy similares
			return LerpUnclamped(a, b, t);

		float omega = Mathf.Acos(dot); // angulo entre ambos quaternions
		float sinOmega = Mathf.Sin(omega);

		float weightA = Mathf.Sin((1f - t) * omega) / sinOmega; // Lerp siguiendo el arco de la función seno
		float weightB = Mathf.Sin(t * omega) / sinOmega;

		return new Quat(
			weightA * a.x + weightB * b.x,
			weightA * a.y + weightB * b.y,
			weightA * a.z + weightB * b.z,
			weightA * a.w + weightB * b.w
		);
	}

	/// <summary>
	/// Deshacer la multiplicación de los tres quaternions qz*qx*qy
	/// para acceder a los ángulos originales
	/// </summary>
	/// <param name="rotation"></param>
	/// <returns></returns>
	public static Vec3 ToEulerAngles(Quat q)
	{
		q = Normalize(q);

		float sinPitch = 2f * (q.w * q.x + q.y * q.z);

		Vec3 euler;

		if (Mathf.Abs(sinPitch) > 0.9999f)
		{
			euler.x = Mathf.Sign(sinPitch) * 90f;
			euler.y = Mathf.Atan2(2f * (q.w * q.y + q.x * q.z), 1f - 2f * (q.y * q.y + q.z * q.z)) * Mathf.Rad2Deg;
			euler.z = 0f;
		}
		else
		{
			euler.x = Mathf.Asin(sinPitch) * Mathf.Rad2Deg;
			euler.y = Mathf.Atan2(2f * (q.w * q.y - q.x * q.z), 1f - 2f * (q.x * q.x + q.y * q.y)) * Mathf.Rad2Deg;
			euler.z = Mathf.Atan2(2f * (q.w * q.z - q.x * q.y), 1f - 2f * (q.x * q.x + q.z * q.z)) * Mathf.Rad2Deg;
		}

		euler.x = NormalizeAngle(euler.x);
		euler.y = NormalizeAngle(euler.y);
		euler.z = NormalizeAngle(euler.z);

		return euler;
	}

	private static float NormalizeAngle(float angle)
	{
		while (angle < 0f)
		{
			angle += 360f;
		}

		while (angle >= 360f)
		{
			angle -= 360f;
		}

		return angle;
	}

	// Extraer eje y ángulo de un quaternion
	public void ToAngleAxis(out float angle, out Vec3 axis)
	{
		if (Mathf.Abs(w) > 1f)
			Normalize();

		angle = 2f * Mathf.Acos(w) * Mathf.Rad2Deg; // Angulo

		float sinHalf = Mathf.Sqrt(1f - w * w); // w = cos(θ/2); sin(θ/2) = √(1 - cos²(θ/2)) = √(1 - w²)

		if (sinHalf < Mathf.Epsilon) // Si el ángulo es muy chico
		{
			axis = new Vec3(1f, 0f, 0f);
		}
		else
		{
			// obtengo el eje
			axis = new Vec3(x / sinHalf, y / sinHalf, z / sinHalf);
		}
	}

	public Vec3 ToEuler()
	{
		return ToEulerAngles(this);
	}
}