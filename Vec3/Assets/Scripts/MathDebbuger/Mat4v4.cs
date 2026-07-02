using System;
using CustomMath;
using UnityEngine;

public struct Mat4x4
{
	public float m00, m01, m02, m03;
	public float m10, m11, m12, m13;
	public float m20, m21, m22, m23;
	public float m30, m31, m32, m33;

	public static Mat4x4 Zero
	{
		get { return new Mat4x4(); }
	}

	/*
	 r	 u  f  pos
	[ 1  0  0  0 ]
	[ 0  1  0  0 ]
	[ 0  0  1  0 ]
	[ 0  0  0  1 ]

	si los ejes dejaran de tener una base ortonormal, donde todos los ejes son perpendiculares entre sí,
	veríamos un sistema de coordenadas transformado, como los que vimos en los videos de 3blue1brown
	unas rotaciones y reglas de posicionamiento más irregulares.
	 */
	public static Mat4x4 Identity
	{
		get
		{
			Mat4x4 m = new Mat4x4();
			m.m00 = 1f;
			m.m11 = 1f;
			m.m22 = 1f;
			m.m33 = 1f;
			return m;
		}
	}

	public Mat4x4 inverse
	{
		get { return Inverse(this); }
	}

	//Regla de sarrus
	//a*(ei−fh) − b*(di−fg) + c*(dh−eg)
	//m11*(m22*m33 - m23*m32) - m12*(m21*m33 - m23*m31) + m13*(m21*m32 - m22*m31)
	public float determinant
	{
		get { return Determinant(this); }
	}

	public bool isIdentity
	{
		get { return this == Identity; }
	}

	public Quat rotation
	{
		get
		{
			Vec3 col0 = new Vec3(m00, m10, m20).normalized;
			Vec3 col1 = new Vec3(m01, m11, m21).normalized;
			Vec3 col2 = new Vec3(m02, m12, m22).normalized;

			return Quat.LookRotation(col2, col1);
		}
	}

	//Es lossy porque perdés el signo, y porque si los ejes no son ortogonales, la escala y la rotación no son separables
	//en la matriz
	public Vec3 lossyScale
	{
		get
		{
			Vec3 column0 = new Vec3(m00, m10, m20);
			Vec3 column1 = new Vec3(m01, m11, m21);
			Vec3 column2 = new Vec3(m02, m12, m22);

			return new Vec3(column0.magnitude, column1.magnitude, column2.magnitude);
		}
	}

	public Mat4x4 transpose
	{
		get { return Transpose(this); }
	}

	public Mat4x4(Vector4 column0, Vector4 column1, Vector4 column2, Vector4 column3)
	{
		m00 = column0.x;
		m01 = column1.x;
		m02 = column2.x;
		m03 = column3.x;

		m10 = column0.y;
		m11 = column1.y;
		m12 = column2.y;
		m13 = column3.y;

		m20 = column0.z;
		m21 = column1.z;
		m22 = column2.z;
		m23 = column3.z;

		m30 = column0.w;
		m31 = column1.w;
		m32 = column2.w;
		m33 = column3.w;
	}

	public Mat4x4(Matrix4x4 unityMatrix)
	{
		m00 = unityMatrix.m00;
		m01 = unityMatrix.m01;
		m02 = unityMatrix.m02;
		m03 = unityMatrix.m03;
		m10 = unityMatrix.m10;
		m11 = unityMatrix.m11;
		m12 = unityMatrix.m12;
		m13 = unityMatrix.m13;
		m20 = unityMatrix.m20;
		m21 = unityMatrix.m21;
		m22 = unityMatrix.m22;
		m23 = unityMatrix.m23;
		m30 = unityMatrix.m30;
		m31 = unityMatrix.m31;
		m32 = unityMatrix.m32;
		m33 = unityMatrix.m33;
	}

	//Fila de m4 por columna de v4
	public static Vector4 operator *(Mat4x4 lhs, Vector4 vector)
	{
		float x = lhs.m00 * vector.x + lhs.m01 * vector.y + lhs.m02 * vector.z + lhs.m03 * vector.w;
		float y = lhs.m10 * vector.x + lhs.m11 * vector.y + lhs.m12 * vector.z + lhs.m13 * vector.w;
		float z = lhs.m20 * vector.x + lhs.m21 * vector.y + lhs.m22 * vector.z + lhs.m23 * vector.w;
		float w = lhs.m30 * vector.x + lhs.m31 * vector.y + lhs.m32 * vector.z + lhs.m33 * vector.w;
		return new Vector4(x, y, z, w);
	}

	//fila por columna. 4 veces
	public static Mat4x4 operator *(Mat4x4 lhs, Mat4x4 rhs)
	{
		Mat4x4 res = new Mat4x4();

		res.m00 = lhs.m00 * rhs.m00 + lhs.m01 * rhs.m10 + lhs.m02 * rhs.m20 + lhs.m03 * rhs.m30;
		res.m01 = lhs.m00 * rhs.m01 + lhs.m01 * rhs.m11 + lhs.m02 * rhs.m21 + lhs.m03 * rhs.m31;
		res.m02 = lhs.m00 * rhs.m02 + lhs.m01 * rhs.m12 + lhs.m02 * rhs.m22 + lhs.m03 * rhs.m32;
		res.m03 = lhs.m00 * rhs.m03 + lhs.m01 * rhs.m13 + lhs.m02 * rhs.m23 + lhs.m03 * rhs.m33;

		res.m10 = lhs.m10 * rhs.m00 + lhs.m11 * rhs.m10 + lhs.m12 * rhs.m20 + lhs.m13 * rhs.m30;
		res.m11 = lhs.m10 * rhs.m01 + lhs.m11 * rhs.m11 + lhs.m12 * rhs.m21 + lhs.m13 * rhs.m31;
		res.m12 = lhs.m10 * rhs.m02 + lhs.m11 * rhs.m12 + lhs.m12 * rhs.m22 + lhs.m13 * rhs.m32;
		res.m13 = lhs.m10 * rhs.m03 + lhs.m11 * rhs.m13 + lhs.m12 * rhs.m23 + lhs.m13 * rhs.m33;

		res.m20 = lhs.m20 * rhs.m00 + lhs.m21 * rhs.m10 + lhs.m22 * rhs.m20 + lhs.m23 * rhs.m30;
		res.m21 = lhs.m20 * rhs.m01 + lhs.m21 * rhs.m11 + lhs.m22 * rhs.m21 + lhs.m23 * rhs.m31;
		res.m22 = lhs.m20 * rhs.m02 + lhs.m21 * rhs.m12 + lhs.m22 * rhs.m22 + lhs.m23 * rhs.m32;
		res.m23 = lhs.m20 * rhs.m03 + lhs.m21 * rhs.m13 + lhs.m22 * rhs.m23 + lhs.m23 * rhs.m33;

		res.m30 = lhs.m30 * rhs.m00 + lhs.m31 * rhs.m10 + lhs.m32 * rhs.m20 + lhs.m33 * rhs.m30;
		res.m31 = lhs.m30 * rhs.m01 + lhs.m31 * rhs.m11 + lhs.m32 * rhs.m21 + lhs.m33 * rhs.m31;
		res.m32 = lhs.m30 * rhs.m02 + lhs.m31 * rhs.m12 + lhs.m32 * rhs.m22 + lhs.m33 * rhs.m32;
		res.m33 = lhs.m30 * rhs.m03 + lhs.m31 * rhs.m13 + lhs.m32 * rhs.m23 + lhs.m33 * rhs.m33;

		return res;
	}

	public static bool operator ==(Mat4x4 lhs, Mat4x4 rhs)
	{
		return Mathf.Approximately(lhs.m00, rhs.m00) && Mathf.Approximately(lhs.m01, rhs.m01) &&
		       Mathf.Approximately(lhs.m02, rhs.m02) && Mathf.Approximately(lhs.m03, rhs.m03) &&
		       Mathf.Approximately(lhs.m10, rhs.m10) && Mathf.Approximately(lhs.m11, rhs.m11) &&
		       Mathf.Approximately(lhs.m12, rhs.m12) && Mathf.Approximately(lhs.m13, rhs.m13) &&
		       Mathf.Approximately(lhs.m20, rhs.m20) && Mathf.Approximately(lhs.m21, rhs.m21) &&
		       Mathf.Approximately(lhs.m22, rhs.m22) && Mathf.Approximately(lhs.m23, rhs.m23) &&
		       Mathf.Approximately(lhs.m30, rhs.m30) && Mathf.Approximately(lhs.m31, rhs.m31) &&
		       Mathf.Approximately(lhs.m32, rhs.m32) && Mathf.Approximately(lhs.m33, rhs.m33);
	}

	public static bool operator !=(Mat4x4 lhs, Mat4x4 rhs)
	{
		return !(lhs == rhs);
	}

	public static implicit operator Matrix4x4(Mat4x4 m)
	{
		Matrix4x4 result = new Matrix4x4();
		result.m00 = m.m00; result.m01 = m.m01; result.m02 = m.m02; result.m03 = m.m03;
		result.m10 = m.m10; result.m11 = m.m11; result.m12 = m.m12; result.m13 = m.m13;
		result.m20 = m.m20; result.m21 = m.m21; result.m22 = m.m22; result.m23 = m.m23;
		result.m30 = m.m30; result.m31 = m.m31; result.m32 = m.m32; result.m33 = m.m33;
		return result;
	}

	//Laplace
	//signo(i, j) = (-1)^(i + j)
	//+ - + -
	//- + - +
	//+ - + -
	//- + - +
	//det = m00 * M00 - m01 * M01 + m02 * M02 - m03 * M03
	public static float Determinant(Mat4x4 m)
	{
		float m00 = m.m11 * (m.m22 * m.m33 - m.m23 * m.m32) - m.m12 * (m.m21 * m.m33 - m.m23 * m.m31) +
		            m.m13 * (m.m21 * m.m32 - m.m22 * m.m31);
		float m01 = m.m10 * (m.m22 * m.m33 - m.m23 * m.m32) - m.m12 * (m.m20 * m.m33 - m.m23 * m.m30) +
		            m.m13 * (m.m20 * m.m32 - m.m22 * m.m30);
		float m02 = m.m10 * (m.m21 * m.m33 - m.m23 * m.m31) - m.m11 * (m.m20 * m.m33 - m.m23 * m.m30) +
		            m.m13 * (m.m20 * m.m31 - m.m21 * m.m30);
		float m03 = m.m10 * (m.m21 * m.m32 - m.m22 * m.m31) - m.m11 * (m.m20 * m.m32 - m.m22 * m.m30) +
		            m.m12 * (m.m20 * m.m31 - m.m21 * m.m30);

		return m.m00 * m00 - m.m01 * m01 + m.m02 * m02 - m.m03 * m03;
	}

	//M⁻¹ = (1 / det(M)) * adj(M)
	//M⁻¹ = adj(M) / 2

	//adj = cof(m) -> transpuesta
	//cof(m) -> mat(det(i,j))
	//transpuesta = espejada sobre la diagonal
	//Esto lo tenés anotado en el cuaderno igual
	public static Mat4x4 Inverse(Mat4x4 m)
	{
		float det = Determinant(m);

		if (Mathf.Approximately(det, 0f))
		{
			return Zero;
		}

		float invDet = 1f / det;
		Mat4x4 result = new Mat4x4();

		result.m00 = (m.m11 * (m.m22 * m.m33 - m.m23 * m.m32) - m.m12 * (m.m21 * m.m33 - m.m23 * m.m31) +
		              m.m13 * (m.m21 * m.m32 - m.m22 * m.m31)) * invDet;
		result.m01 = -(m.m01 * (m.m22 * m.m33 - m.m23 * m.m32) - m.m02 * (m.m21 * m.m33 - m.m23 * m.m31) +
		               m.m03 * (m.m21 * m.m32 - m.m22 * m.m31)) * invDet;
		result.m02 = (m.m01 * (m.m12 * m.m33 - m.m13 * m.m32) - m.m02 * (m.m11 * m.m33 - m.m13 * m.m31) +
		              m.m03 * (m.m11 * m.m32 - m.m12 * m.m31)) * invDet;
		result.m03 = -(m.m01 * (m.m12 * m.m23 - m.m13 * m.m22) - m.m02 * (m.m11 * m.m23 - m.m13 * m.m21) +
		               m.m03 * (m.m11 * m.m22 - m.m12 * m.m21)) * invDet;

		result.m10 = -(m.m10 * (m.m22 * m.m33 - m.m23 * m.m32) - m.m12 * (m.m20 * m.m33 - m.m23 * m.m30) +
		               m.m13 * (m.m20 * m.m32 - m.m22 * m.m30)) * invDet;
		result.m11 = (m.m00 * (m.m22 * m.m33 - m.m23 * m.m32) - m.m02 * (m.m20 * m.m33 - m.m23 * m.m30) +
		              m.m03 * (m.m20 * m.m32 - m.m22 * m.m30)) * invDet;
		result.m12 = -(m.m00 * (m.m12 * m.m33 - m.m13 * m.m32) - m.m02 * (m.m10 * m.m33 - m.m13 * m.m30) +
		               m.m03 * (m.m10 * m.m32 - m.m12 * m.m30)) * invDet;
		result.m13 = (m.m00 * (m.m12 * m.m23 - m.m13 * m.m22) - m.m02 * (m.m10 * m.m23 - m.m13 * m.m20) +
		              m.m03 * (m.m10 * m.m22 - m.m12 * m.m20)) * invDet;

		result.m20 = (m.m10 * (m.m21 * m.m33 - m.m23 * m.m31) - m.m11 * (m.m20 * m.m33 - m.m23 * m.m30) +
		              m.m13 * (m.m20 * m.m31 - m.m21 * m.m30)) * invDet;
		result.m21 = -(m.m00 * (m.m21 * m.m33 - m.m23 * m.m31) - m.m01 * (m.m20 * m.m33 - m.m23 * m.m30) +
		               m.m03 * (m.m20 * m.m31 - m.m21 * m.m30)) * invDet;
		result.m22 = (m.m00 * (m.m11 * m.m33 - m.m13 * m.m31) - m.m01 * (m.m10 * m.m33 - m.m13 * m.m30) +
		              m.m03 * (m.m10 * m.m31 - m.m11 * m.m30)) * invDet;
		result.m23 = -(m.m00 * (m.m11 * m.m23 - m.m13 * m.m21) - m.m01 * (m.m10 * m.m23 - m.m13 * m.m20) +
		               m.m03 * (m.m10 * m.m21 - m.m11 * m.m20)) * invDet;

		result.m30 = -(m.m10 * (m.m21 * m.m32 - m.m22 * m.m31) - m.m11 * (m.m20 * m.m32 - m.m22 * m.m30) +
		               m.m12 * (m.m20 * m.m31 - m.m21 * m.m30)) * invDet;
		result.m31 = (m.m00 * (m.m21 * m.m32 - m.m22 * m.m31) - m.m01 * (m.m20 * m.m32 - m.m22 * m.m30) +
		              m.m02 * (m.m20 * m.m31 - m.m21 * m.m30)) * invDet;
		result.m32 = -(m.m00 * (m.m11 * m.m32 - m.m12 * m.m31) - m.m01 * (m.m10 * m.m32 - m.m12 * m.m30) +
		               m.m02 * (m.m10 * m.m31 - m.m11 * m.m30)) * invDet;
		result.m33 = (m.m00 * (m.m11 * m.m22 - m.m12 * m.m21) - m.m01 * (m.m10 * m.m22 - m.m12 * m.m20) +
		              m.m02 * (m.m10 * m.m21 - m.m11 * m.m20)) * invDet;

		return result;
	}


	//genero 3 ejes perpendiculares
	public static Mat4x4 LookAt(Vec3 from, Vec3 to, Vec3 up)
	{
		Vec3 forward = (to - from).normalized;
		Vec3 right = Vec3.Cross(up, forward).normalized;
		Vec3 realUp = Vec3.Cross(forward, right);

		Mat4x4 m = Identity;

		//X					Y					Z
		m.m00 = right.x; m.m01 = realUp.x; m.m02 = forward.x; m.m03 = from.x;
		m.m10 = right.y; m.m11 = realUp.y; m.m12 = forward.y; m.m13 = from.y;
		m.m20 = right.z; m.m21 = realUp.z; m.m22 = forward.z; m.m23 = from.z;

		return m;
	}

	/*
	r00 = 1 - 2(y² + z²)      r01 = 2(xy - wz)          r02 = 2(xz + wy)
	r10 = 2(xy + wz)          r11 = 1 - 2(x² + z²)      r12 = 2(yz - wx)
	r20 = 2(xz - wy)          r21 = 2(yz + wx)          r22 = 1 - 2(x² + y²)

	=

	q*v*q⁻¹
	 */
	//El quaternion tiene que ser unitario. De lo contrario, modifica escala
	public static Mat4x4 Rotate(Quat q)
	{
		Mat4x4 m = Identity;

		float x = q.x;
		float y = q.y;
		float z = q.z;
		float w = q.w;
		float x2 = x + x;
		float y2 = y + y;
		float z2 = z + z;
		float xx = x * x2;
		float yy = y * y2;
		float zz = z * z2;
		float xy = x * y2;
		float xz = x * z2;
		float yz = y * z2;
		float wx = w * x2;
		float wy = w * y2;
		float wz = w * z2;

		m.m00 = 1f - (yy + zz);
		m.m01 = xy - wz;
		m.m02 = xz + wy;
		m.m10 = xy + wz;
		m.m11 = 1f - (xx + zz);
		m.m12 = yz - wx;
		m.m20 = xz - wy;
		m.m21 = yz + wx;
		m.m22 = 1f - (xx + yy);

		return m;
	}

	/*
	[ sx  0   0   0 ]   [ x ]   [ sx * x ]
	[ 0   sy  0   0 ] * [ y ] = [ sy * y ]
	[ 0   0   sz  0 ]   [ z ]   [ sz * z ]
	[ 0   0   0   1 ]   [ 1 ]   [   1    ]
	 */
	public static Mat4x4 Scale(Vec3 vector)
	{
		Mat4x4 m = Identity;
		m.m00 = vector.x;
		m.m11 = vector.y;
		m.m22 = vector.z;
		return m;
	}

	public static Mat4x4 Translate(Vec3 vector)
	{
		Mat4x4 m = Identity;
		m.m03 = vector.x;
		m.m13 = vector.y;
		m.m23 = vector.z;
		return m;
	}

	public static Mat4x4 Transpose(Mat4x4 m)
	{
		Mat4x4 result = new Mat4x4();
		result.m00 = m.m00;
		result.m01 = m.m10;
		result.m02 = m.m20;
		result.m03 = m.m30;
		result.m10 = m.m01;
		result.m11 = m.m11;
		result.m12 = m.m21;
		result.m13 = m.m31;
		result.m20 = m.m02;
		result.m21 = m.m12;
		result.m22 = m.m22;
		result.m23 = m.m32;
		result.m30 = m.m03;
		result.m31 = m.m13;
		result.m32 = m.m23;
		result.m33 = m.m33;
		return result;
	}

	public static Mat4x4 TRS(Vec3 pos, Quat q, Vec3 s)
	{
		return Translate(pos) * Rotate(q) * Scale(s);
	}

	public Vec3 GetPosition()
	{
		return new Vec3(m03, m13, m23);
	}

	public Vector4 GetRow(int index)
	{
		switch (index)
		{
			case 0: return new Vector4(m00, m01, m02, m03);
			case 1: return new Vector4(m10, m11, m12, m13);
			case 2: return new Vector4(m20, m21, m22, m23);
			case 3: return new Vector4(m30, m31, m32, m33);
			default: throw new IndexOutOfRangeException("Invalid row index!");
		}
	}

	public Vec3 MultiplyPoint(Vec3 point)
	{
		Vector4 result = this * new Vector4(point.x, point.y, point.z, 1f);
		float w = result.w;

		//TODO: consulta lean
		if (w != 0f) // si la matriz tiene una componente de proyección (como una matriz de cámara en perspectiva), la w resultante puede no ser 1, 
		{
			w = 1f / w;
		}

		return new Vec3(result.x * w, result.y * w, result.z * w);
	}

	//para matrices sin proyección (TRS)
	public Vec3 MultiplyPoint3x4(Vec3 point)
	{
		float x = m00 * point.x + m01 * point.y + m02 * point.z + m03;
		float y = m10 * point.x + m11 * point.y + m12 * point.z + m13;
		float z = m20 * point.x + m21 * point.y + m22 * point.z + m23;
		return new Vec3(x, y, z);
	}

	//w = 0 = anula traslación (porque no le importa)
	public Vec3 MultiplyVector(Vec3 vector)
	{
		Vector4 result = this * new Vector4(vector.x, vector.y, vector.z, 0f);
		return new Vec3(result.x, result.y, result.z);
	}

	public void SetColumn(int index, Vector4 column)
	{
		switch (index)
		{
			case 0:
				m00 = column.x;
				m10 = column.y;
				m20 = column.z;
				m30 = column.w;
				break;
			case 1:
				m01 = column.x;
				m11 = column.y;
				m21 = column.z;
				m31 = column.w;
				break;
			case 2:
				m02 = column.x;
				m12 = column.y;
				m22 = column.z;
				m32 = column.w;
				break;
			case 3:
				m03 = column.x;
				m13 = column.y;
				m23 = column.z;
				m33 = column.w;
				break;
			default: throw new IndexOutOfRangeException("Invalid column index!");
		}
	}

	public void SetRow(int index, Vector4 row)
	{
		switch (index)
		{
			case 0:
				m00 = row.x;
				m01 = row.y;
				m02 = row.z;
				m03 = row.w;
				break;
			case 1:
				m10 = row.x;
				m11 = row.y;
				m12 = row.z;
				m13 = row.w;
				break;
			case 2:
				m20 = row.x;
				m21 = row.y;
				m22 = row.z;
				m23 = row.w;
				break;
			case 3:
				m30 = row.x;
				m31 = row.y;
				m32 = row.z;
				m33 = row.w;
				break;
			default: throw new IndexOutOfRangeException("Invalid row index!");
		}
	}

	public void SetTRS(Vec3 pos, Quat q, Vec3 s)
	{
		Mat4x4 result = TRS(pos, q, s);
		m00 = result.m00; m01 = result.m01; m02 = result.m02; m03 = result.m03;
		m10 = result.m10; m11 = result.m11; m12 = result.m12; m13 = result.m13;
		m20 = result.m20; m21 = result.m21; m22 = result.m22; m23 = result.m23;
		m30 = result.m30; m31 = result.m31; m32 = result.m32; m33 = result.m33;
	}

	public bool ValidTRS()
	{
		Vec3 col0 = new Vec3(m00, m10, m20);
		Vec3 col1 = new Vec3(m01, m11, m21);
		Vec3 col2 = new Vec3(m02, m12, m22);

		//3 ejes perpendiculares
		bool orthogonal = Mathf.Approximately(Vec3.Dot(col0, col1), 0f) &&
		                  Mathf.Approximately(Vec3.Dot(col0, col2), 0f) &&
		                  Mathf.Approximately(Vec3.Dot(col1, col2), 0f);

		bool nonSingular = !Mathf.Approximately(Determinant(this), 0f);

		return orthogonal && nonSingular;
	}

	public override string ToString()
	{
		return $"{m00}\t{m01}\t{m02}\t{m03}\n{m10}\t{m11}\t{m12}\t{m13}\n{m20}\t{m21}\t{m22}\t{m23}\n{m30}\t{m31}\t{m32}\t{m33}";
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Mat4x4))
		{
			return false;
		}
		return this == (Mat4x4)obj;
	}

	public override int GetHashCode()
	{
		return m00.GetHashCode() ^ m01.GetHashCode() ^ m02.GetHashCode() ^ m03.GetHashCode() ^
		       m10.GetHashCode() ^ m11.GetHashCode() ^ m12.GetHashCode() ^ m13.GetHashCode() ^
		       m20.GetHashCode() ^ m21.GetHashCode() ^ m22.GetHashCode() ^ m23.GetHashCode() ^
		       m30.GetHashCode() ^ m31.GetHashCode() ^ m32.GetHashCode() ^ m33.GetHashCode();
	}
}