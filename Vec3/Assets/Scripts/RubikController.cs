using System.Collections;
using System.Collections.Generic;
using CustomMath;
using UnityEngine;

// Controles de un cubo de Rubik usando SOLO las clases propias Vec3, Quat y Mat4x4.
// Notacion estandar: U D R L F B giran cada cara; Shift = giro antihorario.
public class RubikController : MonoBehaviour
{
	// Cada cara es un eje local + una capa (+1 / -1)
	private enum Face
	{
		U,
		D,
		R,
		L,
		F,
		B
	}

	private class Cubie
	{
		public Transform t;
		public Vec3 grid; // coordenada logica en la grilla 3x3x3 (componentes en {-1,0,1})
	}

	public Transform cubeRoot;
	public float turnDuration = 0.2f;

	private readonly List<Cubie> cubies = new List<Cubie>();
	private Vec3 cubeCenter; // centro del cubo en mundo
	private float spacing; // separacion entre capas en unidades de mundo
	private Quat[] centerAccum; // rotacion total acumulada de cada centro
	private bool isTurning;

	private static readonly string[] faceNames = { "U", "D", "R", "L", "F", "B" };

	private void Awake()
	{
		if (cubeRoot == null)
		{
			GameObject found = GameObject.Find("CubeRubik");
			if (found != null)
				cubeRoot = found.transform;
		}
	}

	private void Start()
	{
		centerAccum = new Quat[6];
		for (int i = 0; i < centerAccum.Length; i++)
			centerAccum[i] = Quat.identity;

		if (cubeRoot != null)
			BuildCubies();
	}

	// Descubre las piezas del cubo y les asigna su coordenada de grilla
	private void BuildCubies()
	{
		cubies.Clear();

		MeshRenderer[] renderers = cubeRoot.GetComponentsInChildren<MeshRenderer>();
		if (renderers.Length == 0)
			return;

		Vec3 right = new Vec3(cubeRoot.right);
		Vec3 up = new Vec3(cubeRoot.up);
		Vec3 forward = new Vec3(cubeRoot.forward);

		// 1) Centroide de todas las piezas => centro del cubo
		Vec3 sum = Vec3.Zero;
		foreach (MeshRenderer r in renderers)
			sum += new Vec3(r.transform.position);
		cubeCenter = sum / renderers.Length;

		// 2) Rango sobre el eje X local => separacion entre capas (grilla de 3 => -1,0,1)
		float minR = float.MaxValue;
		float maxR = float.MinValue;
		foreach (MeshRenderer r in renderers)
		{
			float d = Vec3.Dot(new Vec3(r.transform.position) - cubeCenter, right);
			if (d < minR) minR = d;
			if (d > maxR) maxR = d;
		}

		spacing = (maxR - minR) * 0.5f;
		if (spacing < Vec3.epsilon)
			spacing = 1f;

		// 3) Proyecto el offset de cada pieza sobre los ejes del cubo y redondeo a {-1,0,1}
		foreach (MeshRenderer r in renderers)
		{
			Vec3 offset = new Vec3(r.transform.position) - cubeCenter;
			float gx = Mathf.Round(Vec3.Dot(offset, right) / spacing);
			float gy = Mathf.Round(Vec3.Dot(offset, up) / spacing);
			float gz = Mathf.Round(Vec3.Dot(offset, forward) / spacing);
			cubies.Add(new Cubie { t = r.transform, grid = new Vec3(gx, gy, gz) });
		}
	}

	private void Update()
	{
		if (isTurning || cubeRoot == null)
			return;

		// horario visto desde afuera = regla de la mano derecha negativa
		bool ccw = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		float dir = ccw ? 1f : -1f;

		if (Input.GetKeyDown(KeyCode.U)) StartCoroutine(TurnFace(Face.U, dir * 90f));
		else if (Input.GetKeyDown(KeyCode.D)) StartCoroutine(TurnFace(Face.D, dir * 90f));
		else if (Input.GetKeyDown(KeyCode.R)) StartCoroutine(TurnFace(Face.R, dir * 90f));
		else if (Input.GetKeyDown(KeyCode.L)) StartCoroutine(TurnFace(Face.L, dir * 90f));
		else if (Input.GetKeyDown(KeyCode.F)) StartCoroutine(TurnFace(Face.F, dir * 90f));
		else if (Input.GetKeyDown(KeyCode.B)) StartCoroutine(TurnFace(Face.B, dir * 90f));
	}

	private IEnumerator TurnFace(Face face, float angle)
	{
		isTurning = true;

		Vec3 localAxis = FaceAxis(face);
		int layer = FaceLayer(face);
		Vec3 worldAxis = new Vec3(cubeRoot.TransformDirection(localAxis));

		// Junto las piezas de la capa y guardo su estado inicial
		List<Cubie> layerCubies = new List<Cubie>();
		List<Vec3> startOffsets = new List<Vec3>();
		List<Quat> startRots = new List<Quat>();

		foreach (Cubie c in cubies)
		{
			//Selecciono todas las piezas cuyas posiciones estén proyectadas sobre la dirección de la capa que quiero
			//mover
			if (Mathf.RoundToInt(Vec3.Dot(c.grid, localAxis)) == layer)
			{
				layerCubies.Add(c);
				startOffsets.Add(new Vec3(c.t.position) - cubeCenter);
				startRots.Add(new Quat(c.t.rotation));
			}
		}

		// Animacion suave del giro de 90 grados
		Quat target = Quat.AngleAxis(angle, worldAxis);
		float elapsed = 0f;
		while (elapsed < turnDuration)
		{
			elapsed += Time.deltaTime;
			float u = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / turnDuration));
			Quat step = Quat.Slerp(Quat.identity, target, u);

			for (int i = 0; i < layerCubies.Count; i++)
			{
				layerCubies[i].t.position = cubeCenter + step * startOffsets[i];
				layerCubies[i].t.rotation = step * startRots[i];
			}

			yield return null;
		}

		// Giro exacto final + re-snap a la grilla para eliminar el drift de punto flotante
		Quat finalStep = Quat.AngleAxis(angle, worldAxis);
		Quat gridStep = Quat.AngleAxis(angle, localAxis);
		Vec3 right = new Vec3(cubeRoot.right);
		Vec3 up = new Vec3(cubeRoot.up);
		Vec3 forward = new Vec3(cubeRoot.forward);

		for (int i = 0; i < layerCubies.Count; i++)
		{
			Cubie c = layerCubies[i];

			// roto la coordenada logica 90 grados y la vuelvo a enteros
			Vec3 rotated = gridStep * c.grid;
			c.grid = new Vec3(Mathf.Round(rotated.x), Mathf.Round(rotated.y), Mathf.Round(rotated.z));

			c.t.position = cubeCenter + spacing * (c.grid.x * right + c.grid.y * up + c.grid.z * forward);
			c.t.rotation = finalStep * startRots[i];
		}

		// Requisito 3: acumulo la rotacion del centro de la cara girada
		centerAccum[(int)face] = gridStep * centerAccum[(int)face];

		isTurning = false;
	}

	private static Vec3 FaceAxis(Face face)
	{
		switch (face)
		{
			case Face.U:
			case Face.D: return Vec3.Up;
			case Face.R:
			case Face.L: return Vec3.Right;
			default: return Vec3.Forward; // F, B
		}
	}

	private static int FaceLayer(Face face)
	{
		switch (face)
		{
			case Face.U:
			case Face.R:
			case Face.F: return 1;
			default: return -1; // D, L, B
		}
	}

	// Requisito 3: cada centro guarda su rotacion acumulada + la posicion y escala
	// del cubo en una Mat4x4, siempre visible en pantalla.
	private void OnGUI()
	{
		if (cubeRoot == null || centerAccum == null)
			return;

		Vec3 pos = new Vec3(cubeRoot.position);
		Vec3 scale = new Vec3(cubeRoot.localScale);

		GUILayout.BeginArea(new Rect(10, 10, 480, Screen.height - 20));
		GUILayout.Label("Controles:  U D R L F B   (+Shift = antihorario)");

		for (int i = 0; i < 6; i++)
		{
			Mat4x4 m = Mat4x4.TRS(pos, centerAccum[i], scale);
			GUILayout.Label($"Centro {faceNames[i]}  (Mat4x4 = T * R * S):\n{m}");
		}

		GUILayout.EndArea();
	}
}