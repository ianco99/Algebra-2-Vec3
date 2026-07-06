using System.ComponentModel;
using CustomMath;
using UnityEngine;

/*
 Una rotación euler descompuesta en una matriz
 M =
[ cY·cZ + sY·sX·sZ   -cY·sZ + sY·sX·cZ    sY·cX ]
[ cX·sZ               cX·cZ              -sX    ]
[ -sY·cZ + cY·sX·sZ   sY·sZ + cY·sX·cZ    cY·cX ]
 */


/*
 Un quaternion descompuesto en una matriz
 R(q) =
[ 1-2(y²+z²)    2(xy-wz)      2(xz+wy)   ]
[ 2(xy+wz)     1-2(x²+z²)     2(yz-wx)   ]
[ 2(xz-wy)     2(yz+wx)      1-2(x²+y²)  ]
  */
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

	/// <summary>
	/// Normaliza los datos del quaternion instanciado
	/// </summary>
	public void Normalize()
	{
		Quat normalized = Normalize(this);

		x = normalized.x;
		y = normalized.y;
		z = normalized.z;
		w = normalized.w;
	}

	/// <summary>
	/// Establece los valores de x, y, z, w en el quaternion instanciado
	/// </summary>
	/// <param name="newX"></param>
	/// <param name="newY"></param>
	/// <param name="newZ"></param>
	/// <param name="newW"></param>
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

	//ROTAR un VECTOR3
	/// <summary>
	/// Rota un punto 3D con un quaternion.
	/// </summary>
	/// <param name="rotation"></param>
	/// <param name="point"></param>
	/// <returns></returns>
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
	//Producto cruz entre i & j me da k
	//i*j*k = -1
	//-1 + -1 + -1
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

	// Toma dos quaternions unitarios
	/// <summary>
	/// Devuelve el ángulo entre dos quaternions unitarios.
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

	//
	// Creo un quaternion con LA MITAD de la rotación para que, al multiplicarse con un vector, aplique la rotación
	// intencionada
	/// <summary>
	/// Crea un quaternion que rota N grados en un eje
	/// </summary>
	/// <param name="angle"></param>
	/// <param name="axis"></param>
	/// <returns></returns>
	public static Quat AngleAxis(float angle, Vec3 axis)
	{
		Vec3 normAxis = axis.normalized; //normalizar eje
		float halfRad = angle * Mathf.Deg2Rad * 0.5f; //Pasar a radianes y conseguir mitad de ángulo
		float sin = Mathf.Sin(halfRad);

		return new Quat(normAxis.x * sin, normAxis.y * sin, normAxis.z * sin, Mathf.Cos(halfRad));
		
		//Un quaternion que representa una rotación de ángulo θ alrededor de un eje N se representa como:
		
		//q = cos(θ/2) + sin(θ/2) * (n_x * i + n_y * j + n_z * k)
		//q = cos(θ/2) + (sin(θ/2) * n_x * i, sin(θ/2) * n_y * j, sin(θ/2) * n_z * k)
	}

	// Producto punto. Multiplicar todos los componentes entre sí
	//	En dos quaterniones unitarios, devuelve el coseno del ángulo entre ellos
	/// <summary>
	/// Producto punto, multiplicación de todos los componentes entre sí en ambos quaterniones. En quaterniones
	/// unitarios, devuelve el coseno del ángulo entre ambos quaterniones.
	/// Definción alternativa: La magnitud de la proyección de un quaternion sobre otro.
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns></returns>
	public static float Dot(Quat a, Quat b)
	{
		return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
	}

	// 3 quaterniones. uno rota en z, otro en Y, otro en X
	// Los ángulos euler representan 3 rotaciones sucesivas alrededor de los 3 ejes del objeto
	// Genero 3 quaternions, uno por cada rotación, usando la formula de eje-ángulo de quaternion
	/// <summary>
	/// Devuelve el quaternion equivalente a la rotación Euler representada por el vector3
	/// </summary>
	/// <param name="euler"></param>
	/// <returns></returns>
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
	
	/// <summary>
	/// Misma funcionalidad que "Euler", solo que asume radianes en el vector3
	/// </summary>
	/// <param name="euler"></param>
	/// <returns></returns>
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
	/// Genera un quaternion que rota desde un vector a otro.
	/// Con dot saco el ángulo, con cross el eje por el cual rotar
	/// El resultado es un quaternion que rota θ sobre el eje de cross product
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
			Vec3 axis = Vec3.Cross(Vec3.Right, from); 

			if (axis.magnitude < Mathf.Epsilon)
				axis = Vec3.Cross(Vec3.Up, from);

			return AngleAxis(180f, axis.normalized); 
		}

		Vec3 rotationAxis = Vec3.Cross(from, to);
		float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

		return AngleAxis(angle, rotationAxis); //Rotar n grados en el eje obtenido
	}
	
	/// <summary>
	/// Devuelve un quaternion con sus componentes imaginarios negados
	/// </summary>
	/// <param name="rotation"></param>
	/// <returns></returns>
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
	
	/// <summary>
 	/// Devuelve un quaternion unitario, producto de interpolar linealmente entre
 	/// 2 quaterniones dado un valor de 0 a 1 (t).
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <param name="t"></param>
	/// <returns></returns>
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
	/// <summary>
	/// Obtengo tres ejes perpendiculares que reflejan mi rotación
	///	Necesito construir una matriz de rotación, y luego usar el método de Shepperd para transformarlo a un 
	///	Quaternion
	/// </summary>
	/// <param name="forward"></param>
	/// <param name="upwards"></param>
	/// <returns></returns>
	public static Quat LookRotation(Vec3 forward, [DefaultValue("Vec3.Up")] Vec3 upwards)
	{
		forward = forward.normalized; //normalizo forward

		Vec3 right = Vec3.Cross(upwards, forward).normalized; //Obtengo dirección de la derecha con cross de adelante y arriba
		Vec3 up = Vec3.Cross(forward, right); //Lo recalculo para que sea exacto perpendicular
		
		
		// Construir matriz de rotación
		//
		//               Right      Up         Forward
		float m00 = right.x, m01 = up.x, m02 = forward.x;
		float m10 = right.y, m11 = up.y, m12 = forward.y;
		float m20 = right.z, m21 = up.z, m22 = forward.z;

		float trace = m00 + m11 + m22;	//Traza: SUMA DE LA DIAGONAL
		Quat q = new Quat();

		// Método de Shepperd
		// Busco el componente del quaternion más grande, y ajusto la formula de conversión para evitar dividir
		// por números chicos.
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

	/// <summary>
	/// Divide todos los componentes del quaternion por su magnitud para normalizarlo.
	/// El resultado es un quaternion unitario.
	/// </summary>
	/// <param name="q"></param>
	/// <returns></returns>
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
	/// <summary>
	/// Crea un quaternion resultante de la rotación de un quaternion a otro, con un máximo de grados por rotación.  
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <param name="maxDegreesDelta"></param>
	/// <returns></returns>
	public static Quat RotateTowards(Quat from, Quat to, float maxDegreesDelta)
	{
		float angle = Angle(from, to);

		if (angle == 0f)
			return to;

		float t = Mathf.Min(1f, maxDegreesDelta / angle); //100 / 25 = 0.25. t = 0.25 o 1/4 de camino

		return SlerpUnclamped(from, to, t);
	}

	/// <summary>
	/// Interpolación hiperbólica entre dos quaterniones, dada por un valor de 0 a 1 llamado "t".
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <param name="t"></param>
	/// <returns></returns>
	public static Quat Slerp(Quat a, Quat b, float t)
	{
		return SlerpUnclamped(a, b, Mathf.Clamp01(t));
	}

	// Spherical lerp, siguiendo un arco guiado por la función seno en base
	// al ángulo entre ambos quaterniones
	/// <summary>
	/// Interpolación hiperbólica entre dos quaterniones, dado por un valor de T.
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <param name="t"></param>
	/// <returns></returns>
	public static Quat SlerpUnclamped(Quat a, Quat b, float t)
	{
		//el coseno del ángulo entre ambos quaterniones
		float dot = Dot(a, b);

		if (dot < 0f) // camino largo o camino corto para la rotación?
						//si el ángulo es más agudo quie obtuso
		{				//Si es obtuso, niego al quaternion y al producto punto para volver al camino más corto
			b = new Quat(-b.x, -b.y, -b.z, -b.w);
			dot = -dot; // necesitamos que el dot sea positivo para el acos
		}

		if (dot > 0.9995f) // si los quaterniones son muy similares, sin(omega) es un resultado muy cercano a 0,
							//lo cual puede devolver un resultado gigante o NaN
			return LerpUnclamped(a, b, t);

		float omega = Mathf.Acos(dot); // angulo entre ambos quaternions
										//con ACOS obtengo el ángulo en grados a partir del coseno del ángulo
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
	/// Convierte un quaternion a una rotación euler en tres ejes.
	/// 
	/// Deshacer la multiplicación de los tres quaternions qz*qx*qy
	/// para acceder a los ángulos originales
	/// </summary>
	/// <param name="rotation"></param>
	/// <returns></returns>
	public static Vec3 ToEulerAngles(Quat q)
	{
		//1.
		q = Normalize(q);

		//2.
		float sinPitch = 2f * (q.w * q.x + q.y * q.z);

		Vec3 euler;

		//3.	
		//gimball lock. Cuando el seno del eje X se acerca a 90° los ejes Z, Y se alinean sobre el mismo eje.
		if (Mathf.Abs(sinPitch) > 0.9999f)		
		{
			//Toda la rotación en X y Z establecido en cero.
			euler.x = Mathf.Sign(sinPitch) * 90f;
			euler.y = Mathf.Atan2(2f * (q.w * q.y + q.x * q.z), 1f - 2f * (q.y * q.y + q.z * q.z)) * Mathf.Rad2Deg;
			euler.z = 0f;
		}
		else
		{
			//Distribución de la rotación por eje
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