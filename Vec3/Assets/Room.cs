using System;
using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = CustomMath.Vec3;
using Plane = CustomMath.MyPlane;

[Serializable]
public class Data
{
	public Room room;
	public GameObject door;
}


public class Room : MonoBehaviour
{
	private Ray rightRoomRaycast;
	private Ray downRoomRaycast;
	public Transform rightPos;
	public Transform downPos;
	public int RoomId;

	public Transform[] planesPos;

	public Plane[] roomPlanes = new Plane[4];
	public List<Data> adyacentRooms = new List<Data>();
	public List<Culling.FrustrumObjects> roomObjects = new List<Culling.FrustrumObjects>();
	public List<GameObject> objectsToCopy = new List<GameObject>();

	public bool isRoomVisible = false;
	public bool isChecked = false;

	private MeshRenderer[] cachedRenderers;

	void Start()
	{
		rightRoomRaycast.origin = rightPos.position;
		rightRoomRaycast.direction = transform.right;
		downRoomRaycast.origin = downPos.position;
		downRoomRaycast.direction = transform.forward * -1;

		BuildPlanes();

		cachedRenderers = GetComponentsInChildren<MeshRenderer>(true);

		foreach (GameObject VARIABLE in objectsToCopy)
		{
			roomObjects.Add(CreateFrustrumObjects(VARIABLE));
		}
	}

	void BuildPlanes()
	{
		if (roomPlanes == null || roomPlanes.Length != 4)
		{
			roomPlanes = new Plane[4];
		}

		if (planesPos == null || planesPos.Length < roomPlanes.Length)
		{
			return;
		}

		for (int i = 0; i < roomPlanes.Length; i++)
		{
			if (planesPos[i] == null)
			{
				continue;
			}

			roomPlanes[i] = new Plane(new Vector3(planesPos[i].forward), new Vector3(planesPos[i].position));
		}
	}

	Culling.FrustrumObjects CreateFrustrumObjects(GameObject realObj)
	{
		Culling.FrustrumObjects fobj = new Culling.FrustrumObjects();
		fobj.gameObject = realObj;
		fobj.meshFilter = realObj.GetComponent<MeshFilter>();
		fobj.meshRenderer = realObj.GetComponent<MeshRenderer>();
		fobj.aabb = new Vector3[8];
		fobj.extents = new Vector3(fobj.meshRenderer.bounds.extents);
		fobj.scale = new Vector3(fobj.meshRenderer.bounds.size);
		fobj.isAABBInside = false;
		return fobj;
	}

	void Update()
	{
		rightRoomRaycast.origin = rightPos.position;
		rightRoomRaycast.direction = transform.right;
		downRoomRaycast.origin = downPos.position;
		downRoomRaycast.direction = transform.forward * -1;
	}

	void LateUpdate()
	{
		if (isRoomVisible)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	public void Hide()
	{
		SetRenderersEnabled(false);
	}

	public void Show()
	{
		SetRenderersEnabled(true);
	}

	void SetRenderersEnabled(bool value)
	{
		if (cachedRenderers == null)
		{
			cachedRenderers = GetComponentsInChildren<MeshRenderer>(true);
		}

		for (int i = 0; i < cachedRenderers.Length; i++)
		{
			cachedRenderers[i].enabled = value;
		}
	}

	void OnDrawGizmos()
	{
		if (!Application.isPlaying)
		{
			BuildPlanes();
		}

		Gizmos.color = Color.red;
		Gizmos.DrawRay(rightRoomRaycast);
		Gizmos.color = Color.blue;
		Gizmos.DrawRay(downRoomRaycast);

		if (planesPos == null)
		{
			return;
		}

		for (int i = 0; i < planesPos.Length; i++)
		{
			if (planesPos[i] == null)
			{
				continue;
			}

			Gizmos.DrawSphere(planesPos[i].position, 0.5f);
		}

		Gizmos.color = Color.green;

		for (int i = 0; i < roomPlanes.Length; i++)
		{
			if (i >= planesPos.Length || planesPos[i] == null)
			{
				continue;
			}

			DrawPlane(new Vector3(planesPos[i].position), roomPlanes[i].normal);
		}
	}

	public bool isPointInside(Vector3 pos)
	{
		for (int i = 0; i < roomPlanes.Length; i++)
		{
			if (!roomPlanes[i].GetSide(pos))
			{
				return false;
			}
		}

		return true;
	}

	public void DrawPlane(Vector3 position, Vector3 normal)
	{
		Vector3 v3;
		if (normal.normalized != Vector3.Forward)
			v3 = Vector3.Cross(normal, Vector3.Forward).normalized * normal.magnitude;
		else
			v3 = Vector3.Cross(normal, Vector3.Up).normalized * normal.magnitude;
		;
		var corner0 = position + v3;
		var corner2 = position - v3;
		var q = Quaternion.AngleAxis(90.0f, normal);
		v3 = q * v3;
		var corner1 = position + v3;
		var corner3 = position - v3;
		Debug.DrawLine(corner0, corner2, Color.green);
		Debug.DrawLine(corner1, corner3, Color.green);
		Debug.DrawLine(corner0, corner1, Color.green);
		Debug.DrawLine(corner1, corner2, Color.green);
		Debug.DrawLine(corner2, corner3, Color.green);
		Debug.DrawLine(corner3, corner0, Color.green);
		Debug.DrawRay(position, normal, Color.magenta);
	}
}
