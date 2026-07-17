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
	[SerializeField] private float iterFreq;
	[SerializeField] private bool logRooms = false;

	private Camera cam;
	private int sampleCount;
	private string lastSnapshot = "";

	struct Line
	{
		public Vector3 ini;
		public Vector3 dir;
	}

	private Line[] lines;

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

		if (cam == null || renderDis <= 0.0f || iterFreq <= 0.0f)
		{
			return false;
		}

		rows = Mathf.Max(1, rows);
		columns = Mathf.Max(1, columns);

		if (lines == null || lines.Length != rows * columns)
		{
			lines = new Line[rows * columns];
		}

		sampleCount = Mathf.Max(1, Mathf.CeilToInt(renderDis / iterFreq));

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

	Vector3 SamplePoint(int line, int step)
	{
		return lines[line].ini + lines[line].dir * Mathf.Min(iterFreq * (step + 1), renderDis);
	}

	public void DrawLines()
	{
		Gizmos.color = Color.red;

		for (int i = 0; i < lines.Length; i++)
		{
			for (int j = 0; j < sampleCount; j++)
			{
				Gizmos.DrawSphere(SamplePoint(i, j), 0.1f);
			}
		}
	}

	void Update()
	{
		if (!EnsureReady())
		{
			return;
		}

		InitLines();
		checkPlayerPos();

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
			allRooms[i].isRoomVisible = false;
			allRooms[i].isChecked = false;
		}

		Vector3 playerPos = new Vector3(player.position);

		for (int i = 0; i < allRooms.Length; i++)
		{
			if (allRooms[i].isPointInside(playerPos))
			{
				currentRoomIndex = i;
				currentRoom = allRooms[i];
				currentRoom.isRoomVisible = true;
				currentRoom.isChecked = true;
				break;
			}
		}
	}

	void checkAdyacentRooms(Data adjRoom)
	{
		if (adjRoom == null || adjRoom.room == null || adjRoom.door == null || adjRoom.room.isChecked)
		{
			return;
		}

		Transform door = adjRoom.door.transform;

		Vector3 doorPos = new Vector3(door.position);
		Vector3 doorRight = new Vector3(door.right);
		Vector3 doorUp = new Vector3(door.up);

		float halfWidth = door.lossyScale.x * 0.5f;
		float halfHeight = door.lossyScale.y * 0.5f;

		Plane facing = new Plane(new Vector3(door.forward), doorPos);
		Plane left = new Plane(doorRight, doorPos - doorRight * halfWidth);
		Plane right = new Plane(-doorRight, doorPos + doorRight * halfWidth);
		Plane bottom = new Plane(doorUp, doorPos - doorUp * halfHeight);
		Plane top = new Plane(-doorUp, doorPos + doorUp * halfHeight);

		for (int i = 0; i < lines.Length; i++)
		{
			for (int j = 0; j < sampleCount; j++)
			{
				Vector3 point = SamplePoint(i, j);

				if (facing.GetSide(point))
				{
					continue;
				}

				if (!left.GetSide(point) || !right.GetSide(point))
				{
					continue;
				}

				if (!bottom.GetSide(point) || !top.GetSide(point))
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
		}

		DrawLines();
	}
}
