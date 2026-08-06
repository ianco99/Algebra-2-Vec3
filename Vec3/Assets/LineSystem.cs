using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Plane = CustomMath.MyPlane;
using Vector3 = CustomMath.Vec3;

public class LineSystem : MonoBehaviour
{
	public Room[] allRooms;
	public Room currentRoom;
	private Transform player;
	public int currentRoomIndex = -1;


	[SerializeField] private int rows = 5;
	[SerializeField] private int columns = 9;
	[SerializeField] private float renderDis;
	[SerializeField] private int maxSubdivisions = 7;
	[SerializeField] private float doorMargin = 1.0f;
	[SerializeField] private bool logRooms = false;

	private const float parallelTolerance = 0.0001f;
	private const float hitTolerance = 0.0001f;
	private const float faceTolerance = 0.001f;
	private const float wallOffset = 0.01f;

	private Camera cam;
	private string lastSnapshot = "";

	struct Line
	{
		public Vector3 ini;
		public Vector3 dir;
	}

	struct Sample
	{
		public Vector3 point;
		public int roomIndex;
		public int depth;
	}

	struct DoorFrame
	{
		public Vector3 left;
		public Vector3 right;
		public float threshold;
	}

	private Line[] lines;
	private List<Sample>[] raySamples;
	private bool[] rayHitWall;

	void Start()
	{
		player = transform.parent;
		EnsureReady();
	}

	bool EnsureReady()
	{
		if (cam == null)
		{
			cam = GetComponent<Camera>();
		}

		if (cam == null || renderDis <= 0.0f)
		{
			return false;
		}

		rows = Mathf.Max(1, rows);
		columns = Mathf.Max(1, columns);
		maxSubdivisions = Mathf.Clamp(maxSubdivisions, 1, 12);
		doorMargin = Mathf.Max(0.0f, doorMargin);

		int total = rows * columns;

		if (lines == null || lines.Length != total)
		{
			lines = new Line[total];
		}

		if (raySamples == null || raySamples.Length != total)
		{
			raySamples = new List<Sample>[total];

			for (int i = 0; i < total; i++)
			{
				raySamples[i] = new List<Sample>(32);
			}
		}

		if (rayHitWall == null || rayHitWall.Length != total)
		{
			rayHitWall = new bool[total];
		}

		return true;
	}

	void InitLines()
	{
		float tanHalfV = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
		float tanHalfH = tanHalfV * cam.aspect;

		Vector3 origin = new Vector3(transform.position);
		Vector3 forward = new Vector3(transform.forward);
		Vector3 right = new Vector3(transform.right);
		Vector3 up = new Vector3(transform.up);

		for (int r = 0; r < rows; r++)
		{
			float v = rows > 1 ? 2.0f * r / (rows - 1) - 1.0f : 0.0f;

			for (int c = 0; c < columns; c++)
			{
				float u = columns > 1 ? 2.0f * c / (columns - 1) - 1.0f : 0.0f;

				lines[r * columns + c].ini = origin;
				lines[r * columns + c].dir = (forward + right * (u * tanHalfH) + up * (v * tanHalfV)).normalized;
			}
		}
	}

	int RoomIndexAt(Vector3 point)
	{
		if (allRooms == null)
		{
			return -1;
		}

		for (int i = 0; i < allRooms.Length; i++)
		{
			if (allRooms[i] == null)
			{
				continue;
			}

			if (allRooms[i].isPointInside(point))
			{
				return i;
			}
		}

		return -1;
	}

	void TraceRays()
	{
		for (int i = 0; i < lines.Length; i++)
		{
			TraceRay(i);
		}
	}

	bool IsOnRoomFace(Room room, int planeIndex, Vector3 point)
	{
		for (int i = 0; i < room.roomPlanes.Length; i++)
		{
			if (i == planeIndex)
			{
				continue;
			}

			if (room.roomPlanes[i].GetDistanceToPoint(point) < -faceTolerance)
			{
				return false;
			}
		}

		return true;
	}

	bool IsDoorwayPoint(Room room, Vector3 point)
	{
		for (int i = 0; i < room.adyacentRooms.Count; i++)
		{
			Data link = room.adyacentRooms[i];

			if (link == null || link.door == null)
			{
				continue;
			}

			if (IsPointInsideDoorFrame(BuildDoorFrame(link.door.transform), point))
			{
				return true;
			}
		}

		return false;
	}

	bool RaycastWalls(Vector3 origin, Vector3 dir, out Vector3 hit)
	{
		float best = renderDis;
		bool found = false;

		for (int r = 0; allRooms != null && r < allRooms.Length; r++)
		{
			Room room = allRooms[r];

			if (room == null || room.roomPlanes == null)
			{
				continue;
			}

			for (int i = 0; i < room.roomPlanes.Length; i++)
			{
				Plane plane = room.roomPlanes[i];

				float denom = Vector3.Dot(plane.normal, dir);

				if (Mathf.Abs(denom) < parallelTolerance)
				{
					continue;
				}

				float t = -plane.GetDistanceToPoint(origin) / denom;

				if (t <= hitTolerance || t >= best)
				{
					continue;
				}

				Vector3 point = origin + dir * t;

				if (!IsOnRoomFace(room, i, point))
				{
					continue;
				}

				if (IsDoorwayPoint(room, point))
				{
					continue;
				}

				best = t;
				found = true;
			}
		}

		if (found)
		{
			best = Mathf.Max(0.0f, best - wallOffset);
		}

		hit = origin + dir * best;

		return found;
	}

	void TraceRay(int line)
	{
		List<Sample> samples = raySamples[line];
		samples.Clear();

		Vector3 start = lines[line].ini;
		Vector3 end;

		rayHitWall[line] = RaycastWalls(start, lines[line].dir, out end);

		int roomStart = RoomIndexAt(start);
		int roomEnd = RoomIndexAt(end);

		AddSample(samples, start, roomStart, 0);
		Subdivide(samples, start, end, roomStart, roomEnd, 0);
		AddSample(samples, end, roomEnd, 0);
	}

	void Subdivide(List<Sample> samples, Vector3 a, Vector3 b, int roomA, int roomB, int depth)
	{
		if (roomA == roomB)
		{
			return;
		}

		if (depth >= maxSubdivisions)
		{
			return;
		}

		Vector3 mid = Vector3.Lerp(a, b, 0.5f);
		int roomMid = RoomIndexAt(mid);

		Subdivide(samples, a, mid, roomA, roomMid, depth + 1);
		AddSample(samples, mid, roomMid, depth + 1);
		Subdivide(samples, mid, b, roomMid, roomB, depth + 1);
	}

	void AddSample(List<Sample> samples, Vector3 point, int roomIndex, int depth)
	{
		Sample sample;
		sample.point = point;
		sample.roomIndex = roomIndex;
		sample.depth = depth;
		samples.Add(sample);
	}

	public void DrawLines()
	{
		if (raySamples == null)
		{
			return;
		}

		for (int i = 0; i < raySamples.Length; i++)
		{
			List<Sample> samples = raySamples[i];

			if (samples == null || samples.Count == 0)
			{
				continue;
			}

			Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.12f);
			Gizmos.DrawLine(samples[0].point, samples[samples.Count - 1].point);

			for (int j = 0; j < samples.Count; j++)
			{
				Gizmos.color = RoomColor(samples[j].roomIndex);
				Gizmos.DrawSphere(samples[j].point, 0.02f + 0.06f / (1 + samples[j].depth));
			}

			if (rayHitWall != null && i < rayHitWall.Length && rayHitWall[i])
			{
				Gizmos.color = Color.white;
				Gizmos.DrawWireCube(samples[samples.Count - 1].point, new Vector3(0.1f, 0.1f, 0.1f));
			}
		}
	}

	Color RoomColor(int roomIndex)
	{
		if (roomIndex < 0)
		{
			return new Color(0.35f, 0.35f, 0.35f, 0.6f);
		}

		return Color.HSVToRGB((roomIndex * 0.618034f) % 1.0f, 0.85f, 1.0f);
	}

	void Update()
	{
		if (!EnsureReady())
		{
			return;
		}

		InitLines();
		checkPlayerPos();
		TraceRays();

		if (currentRoom == null)
		{
			LogSnapshot();
			return;
		}

		currentRoom.isRoomVisible = true;
		currentRoom.isChecked = true;

		foreach (Data adRoom in currentRoom.adyacentRooms)
		{
			checkAdyacentRooms(adRoom);
		}

		LogSnapshot();
	}

	void LogSnapshot()
	{
		if (!logRooms)
		{
			return;
		}

		System.Text.StringBuilder sb = new System.Text.StringBuilder();

		sb.Append("player is in ").Append(currentRoom != null ? currentRoom.name : "NO ROOM");
		sb.Append(" | rooms lit by lines:");

		for (int i = 0; i < allRooms.Length; i++)
		{
			if (allRooms[i].isRoomVisible)
			{
				sb.Append(' ').Append(allRooms[i].name);
			}
		}

		string snapshot = sb.ToString();

		if (snapshot == lastSnapshot)
		{
			return;
		}

		lastSnapshot = snapshot;
		Debug.Log("[LineSystem] " + snapshot);
	}

	void checkPlayerPos()
	{
		currentRoom = null;
		currentRoomIndex = -1;

		for (int i = 0; i < allRooms.Length; i++)
		{
			if (allRooms[i] == null)
			{
				continue;
			}

			allRooms[i].isRoomVisible = false;
			allRooms[i].isChecked = false;
		}

		if (player == null)
		{
			return;
		}

		currentRoomIndex = RoomIndexAt(new Vector3(player.position));

		if (currentRoomIndex < 0)
		{
			return;
		}

		currentRoom = allRooms[currentRoomIndex];
		currentRoom.isRoomVisible = true;
		currentRoom.isChecked = true;
	}

	DoorFrame BuildDoorFrame(Transform door)
	{
		Vector3 center = new Vector3(door.position);
		Vector3 doorRight = new Vector3(door.right);

		float halfWidth = door.lossyScale.x * 0.5f;

		DoorFrame frame;
		frame.left = center - doorRight * halfWidth;
		frame.right = center + doorRight * halfWidth;
		frame.threshold = Vector3.Distance(frame.left, frame.right) + doorMargin;

		return frame;
	}

	bool IsPointInsideDoorFrame(DoorFrame frame, Vector3 point)
	{
		float sum = Vector3.Distance(point, frame.left) + Vector3.Distance(point, frame.right);

		return sum <= frame.threshold;
	}

	void checkAdyacentRooms(Data adjRoom)
	{
		if (adjRoom == null || adjRoom.room == null || adjRoom.door == null || adjRoom.room.isChecked)
		{
			return;
		}

		DoorFrame frame = BuildDoorFrame(adjRoom.door.transform);

		for (int i = 0; i < raySamples.Length; i++)
		{
			List<Sample> samples = raySamples[i];

			for (int j = 0; j < samples.Count; j++)
			{
				if (samples[j].roomIndex < 0)
				{
					continue;
				}

				Vector3 point = samples[j].point;

				if (!IsPointInsideDoorFrame(frame, point))
				{
					continue;
				}

				if (!adjRoom.room.isPointInside(point))
				{
					continue;
				}

				adjRoom.room.isRoomVisible = true;
				adjRoom.room.isChecked = true;

				foreach (Data adRoom in adjRoom.room.adyacentRooms)
				{
					checkAdyacentRooms(adRoom);
				}

				return;
			}
		}
	}

	void OnDrawGizmos()
	{
		if (!EnsureReady())
		{
			return;
		}

		if (!Application.isPlaying)
		{
			InitLines();
			TraceRays();
		}

		DrawLines();
	}
}
