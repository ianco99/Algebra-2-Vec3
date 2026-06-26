using System.ComponentModel;
using CustomMath;
using UnityEngine;

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
		get { return new Vec3(x, y, z); }
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
	}

	public void SetEulerAngles(Vec3 euler)
	{
	}

	public void SetEulerRotation(Vec3 euler)
	{
	}

	public void SetFromToRotation(Vec3 fromDirection, Vec3 toDirection)
	{
	}

	public void SetLookRotation(Vec3 view)
	{
	}

	public void SetLookRotation(Vec3 view, [DefaultValue("Vec3.up")] Vec3 up)
	{
	}

	public static bool operator ==(Quat lhs, Quat rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z && lhs.w == rhs.w;
	}

	public static bool operator !=(Quat lhs, Quat rhs)
	{
		return !(lhs == rhs);
	}

	//TODO:
	public static Quat operator *(Quat rotation, Vec3 point)
	{
		return new Quat();
	}


	//w + xi + yj + zk
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
	//
	// j * i = -k		k * j = -i			i * k = -j
	//
	//
	//Distributiva agrupando terminos en base a los signos. Agrupados en w las cuentas que resultan en numeros reales
	//Agrupados en x las cuentas que dan numeros con i
	//Agrupados en y las cuentas que dan numeros con j
	//Agrupados en z las cuentas que dan numeros con k
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

	//TODO:
	public static float Angle(Quat a, Quat b)
	{
		return 0;
	}

	//TODO:
	public static Quat AngleAxis(float angle, Vec3 axis)
	{
		return new Quat(angle, axis.x, axis.y, axis.z);
	}

	//Producto punto. Multiplicar todos los componentes entre sí
	public static float Dot(Quat a, Quat b)
	{
		return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
	}


	//TODO:
	public static Quat Euler(Vec3 euler)
	{
		return new Quat();
	}

	//TODO:
	public static Quat EulerAngles(Vec3 euler)
	{
		return new Quat();
	}

	//TODO:
	public static Quat EulerRotation(Vec3 euler)
	{
		return new Quat();
	}

	//TODO:
	public static Quat FromToRotation(Vec3 fromDirection, Vec3 toDirection)
	{
		return new Quat();
	}

	public static Quat Conjugate(Quat rotation)
	{
		float newX = -rotation.x;
		float newY = -rotation.y;
		float newZ = -rotation.z;
		
		return new Quat(newX, newY, newZ, rotation.w);
	}
	
	//La rotación opuesta a la representada
	// Conjugado: mismo quaternion con las tres partes imaginarias negadas
	//q * inversa(q) = 1
	//inversa = conjugado(q)/mag(q)^2
	
	//1 = q * conjugado(q)/mag(q)^2
	
	//magnitud seria sqrt(w*w x*x y*y z*z)
	//magnitud^2 es w*w x*x y*y z*z
	public static Quat Inverse(Quat rotation)
	{
		float magSq = rotation.w * rotation.w + rotation.x * rotation.x + rotation.y * rotation.y + rotation.z * rotation.z;
		Quat conjugate = Conjugate(rotation);
		
		return new Quat(conjugate.x / magSq, conjugate.y / magSq, conjugate.z / magSq, conjugate.w / magSq);
	}

	//TODO:
	public static Quat Lerp(Quat a, Quat b, float t)
	{
		return new Quat();
	}

	//TODO:
	public static Quat LerpUnclamped(Quat a, Quat b, float t)
	{
		return new Quat();
	}

	//TODO:
	public static Quat LookRotation(Vec3 forward)
	{
		return new Quat();
	}

	//TODO:
	public static Quat LookRotation(Vec3 forward, [DefaultValue("Vec3.up")] Vec3 upwards)
	{
		return new Quat();
	}

	//TODO:
	public static Quat Normalize(Quat q)
	{
		//Hacer "Dot" es lo mismo que hacer x*x + y*y + z*z + w*w
		//Pitagoras de 4 elementos sqrt(x2 + y2 + z2 + w2)
		float mag = Mathf.Sqrt(Dot(q, q));

		//Si la magnitud es minima, normalizar a identidad
		if (mag < Mathf.Epsilon)
			return identity;

		//Componentes del Quaternion divididos por su magnitud
		return new Quat(q.x / mag, q.y / mag, q.z / mag, q.w / mag);
	}

	//TODO:
	public static Quat RotateTowards(Quat from, Quat to, float maxDegreesDelta)
	{
		return new Quat();
	}

	//TODO:
	public static Quat Slerp(Quat a, Quat b, float t)
	{
		return new Quat();
	}

	//TODO:
	public static Quat SlerpUnclamped(Quat a, Quat b, float t)
	{
		return new Quat();
	}

	//TODO:
	public static Vec3 ToEulerAngles(Quat rotation)
	{
		return new Vec3();
	}

//TODO:
	public void ToAngleAxis(out float angle, out Vec3 axis)
	{
		angle = 0f;
		axis = Vec3.Zero;
	}

//TODO:
	public Vec3 ToEuler()
	{
		return Vec3.Zero;
	}
}