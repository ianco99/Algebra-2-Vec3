using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = CustomMath.Vec3;
using Plane = CustomMath.MyPlane;

public class Culling : MonoBehaviour
{
	#region Variables

	private const int maxPlanes = 6;
	private const int AABBPoints = 8;

	[SerializeField] bool drawVertices = false;
	[SerializeField] bool logCulling = false;

	private string lastSnapshot = "";

	#endregion

	Camera camera;
	[SerializeField] LineSystem lineSystem;

	#region PlanePoints

	Plane[] plane = new Plane[maxPlanes];
	[SerializeField] Vector3 nearTopLeft;
	[SerializeField] Vector3 nearTopRight;
	[SerializeField] Vector3 nearDownLeft;
	[SerializeField] Vector3 nearDownRight;
	[SerializeField] Vector3 farTopLeft;
	[SerializeField] Vector3 farTopRight;
	[SerializeField] Vector3 farDownLeft;
	[SerializeField] Vector3 farDownRight;

	#endregion

	public class FrustrumObjects
	{
		public GameObject gameObject;
		public MeshFilter meshFilter;
		public MeshRenderer meshRenderer;
		public Vector3[] aabb;
		public Vector3 scale;
		public Vector3 extents;
		public bool isAABBInside;
	}

	private void Awake()
	{
		camera = Camera.main;
	}

	void Start()
	{
		for (int i = 0; i < maxPlanes; i++)
		{
			plane[i] = new Plane();
		}
	}

	private void FixedUpdate()
	{
		if (lineSystem == null || camera == null)
		{
			return;
		}

		UpdateFrustrumPlanes();

		foreach (Room room in lineSystem.allRooms)
		{
			foreach (FrustrumObjects roomObject in room.roomObjects)
			{
				ObjectCollision(room, roomObject);
			}
		}

		LogSnapshot();
	}

	void UpdateFrustrumPlanes()
	{
		var nearPlanePos = new Vector3(camera.transform.position + camera.transform.forward * camera.nearClipPlane);
		var farPlanePos = new Vector3(camera.transform.position + camera.transform.forward * camera.farClipPlane);
		plane[0].SetNormalAndPosition(new Vector3(camera.transform.forward), nearPlanePos);
		plane[1].SetNormalAndPosition(new Vector3(camera.transform.forward * -1), farPlanePos);

		SetFarPoints(farPlanePos);
		SetNearPoints(nearPlanePos);

		plane[2].Set3Points(new Vector3(camera.transform.position), farDownLeft, farTopLeft);
		plane[3].Set3Points(new Vector3(camera.transform.position), farTopRight, farDownRight);
		plane[4].Set3Points(new Vector3(camera.transform.position), farTopLeft, farTopRight);
		plane[5].Set3Points(new Vector3(camera.transform.position), farDownRight, farDownLeft);

		for (int i = 2; i < maxPlanes; i++)
		{
			plane[i].Flip();
		}
	}


	public void SetFarPoints(Vector3 farPlanePos)
	{
		float halfVSide = Mathf.Tan((camera.fieldOfView / 2) * Mathf.Deg2Rad) * camera.farClipPlane;
		float halfHSide = (camera.aspect * halfVSide);

		var farPlaneDistance = camera.transform.position + camera.transform.forward * camera.farClipPlane;

		farTopLeft = new Vector3(farPlaneDistance + (camera.transform.up * halfVSide) -
		                         (camera.transform.right * halfHSide));

		farTopRight = new Vector3(farPlaneDistance + (camera.transform.up * halfVSide) +
		                          (camera.transform.right * halfHSide));

		farDownLeft = new Vector3(farPlaneDistance - (camera.transform.up * halfVSide) -
		                          (camera.transform.right * halfHSide));

		farDownRight = new Vector3(farPlaneDistance - (camera.transform.up * halfVSide) +
		                           (camera.transform.right * halfHSide));
	}


	public void SetNearPoints(Vector3 nearPlanePos)
	{
		float halfVSide = Mathf.Tan((camera.fieldOfView / 2) * Mathf.Deg2Rad) * camera.nearClipPlane;
		float halfHSide = (camera.aspect * halfVSide);

		var nearPlaneDistance = camera.transform.position + (camera.transform.forward * camera.nearClipPlane);

		nearTopLeft = new Vector3(nearPlaneDistance + (camera.transform.up * halfVSide) -
		                          (camera.transform.right * halfHSide));

		nearTopRight = new Vector3(nearPlaneDistance + (camera.transform.up * halfVSide) +
		                           (camera.transform.right * halfHSide));

		nearDownLeft = new Vector3(nearPlaneDistance - (camera.transform.up * halfVSide) -
		                           (camera.transform.right * halfHSide));

		nearDownRight = new Vector3(nearPlaneDistance - (camera.transform.up * halfVSide) +
		                            (camera.transform.right * halfHSide));
	}


	public void SetAABB(FrustrumObjects actualObj)
	{
		Bounds bounds = actualObj.meshRenderer.bounds;

		Vector3 center = new Vector3(bounds.center);
		Vector3 size = new Vector3(bounds.extents);

		actualObj.extents = size;
		actualObj.scale = new Vector3(bounds.size);

		actualObj.aabb[0] = new Vector3(center.x - size.x, center.y + size.y, center.z - size.z);
		actualObj.aabb[1] = new Vector3(center.x + size.x, center.y + size.y, center.z - size.z);
		actualObj.aabb[2] = new Vector3(center.x - size.x, center.y - size.y, center.z - size.z);
		actualObj.aabb[3] = new Vector3(center.x + size.x, center.y - size.y, center.z - size.z);
		actualObj.aabb[4] = new Vector3(center.x - size.x, center.y + size.y, center.z + size.z);
		actualObj.aabb[5] = new Vector3(center.x + size.x, center.y + size.y, center.z + size.z);
		actualObj.aabb[6] = new Vector3(center.x - size.x, center.y - size.y, center.z + size.z);
		actualObj.aabb[7] = new Vector3(center.x + size.x, center.y - size.y, center.z + size.z);
	}


	bool IsAABBInFrustrum(FrustrumObjects actualObj)
	{
		for (int j = 0; j < maxPlanes; j++)
		{
			bool allCornersOutside = true;

			for (int i = 0; i < AABBPoints; i++)
			{
				if (plane[j].GetSide(actualObj.aabb[i]))
				{
					allCornersOutside = false;
					break;
				}
			}

			if (allCornersOutside)
			{
				return false;
			}
		}

		return true;
	}


	public void ObjectCollision(Room room, FrustrumObjects actualObj)
	{
		SetAABB(actualObj);

		actualObj.isAABBInside = IsAABBInFrustrum(actualObj);

		bool shouldBeActive = room.isRoomVisible && actualObj.isAABBInside;

		if (actualObj.gameObject.activeSelf != shouldBeActive)
		{
			actualObj.gameObject.SetActive(shouldBeActive);
		}
	}


	void LogSnapshot()
	{
		if (!logCulling)
		{
			return;
		}

		System.Text.StringBuilder sb = new System.Text.StringBuilder();

		foreach (Room room in lineSystem.allRooms)
		{
			sb.Append(room.name).Append(room.isRoomVisible ? "[VISIBLE]" : "[hidden]");

			foreach (FrustrumObjects roomObject in room.roomObjects)
			{
				sb.Append(' ').Append(roomObject.gameObject.name)
					.Append(roomObject.gameObject.activeSelf ? "=ON" : "=OFF")
					.Append("(inFrustrum=").Append(roomObject.isAABBInside).Append(')');
			}

			sb.Append(" | ");
		}

		string snapshot = sb.ToString();

		if (snapshot == lastSnapshot)
		{
			return;
		}

		lastSnapshot = snapshot;
		Debug.Log("[Culling] " + snapshot);
	}


	public void OnDrawGizmos()
	{
		if (!Application.isPlaying)
		{
			return;
		}

		Gizmos.color = Color.green;
		DrawPlane(farTopRight, farDownRight, farDownLeft, farTopLeft);
		DrawPlane(nearTopRight, nearDownRight, nearDownLeft, nearTopLeft);
		DrawPlane(nearTopLeft, farTopLeft, farDownLeft, nearDownLeft);
		DrawPlane(nearTopRight, farTopRight, farDownRight, nearDownRight);
		DrawPlane(nearTopLeft, farTopLeft, farTopRight, nearTopRight);
		DrawPlane(nearDownLeft, farDownLeft, farDownRight, nearDownRight);

		if (lineSystem == null)
		{
			return;
		}

		foreach (Room room in lineSystem.allRooms)
		{
			foreach (FrustrumObjects roomObject in room.roomObjects)
			{
				DrawAABB(roomObject);

				if (drawVertices)
				{
					DrawVert(roomObject);
				}
			}
		}
	}


	public void DrawPlane(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
	{
		Gizmos.DrawLine(p1, p2);
		Gizmos.DrawLine(p2, p3);
		Gizmos.DrawLine(p3, p4);
		Gizmos.DrawLine(p4, p1);

		Gizmos.color = Color.red;
	}


	public void DrawAABB(FrustrumObjects actualObj)
	{
		Gizmos.color = actualObj.isAABBInside ? Color.magenta : Color.grey;

		for (int i = 0; i < AABBPoints; i++)
		{
			Gizmos.DrawSphere(actualObj.aabb[i], 0.05f);
		}

		Gizmos.DrawLine(actualObj.aabb[0], actualObj.aabb[1]);
		Gizmos.DrawLine(actualObj.aabb[0], actualObj.aabb[4]);
		Gizmos.DrawLine(actualObj.aabb[1], actualObj.aabb[3]);
		Gizmos.DrawLine(actualObj.aabb[2], actualObj.aabb[0]);
		Gizmos.DrawLine(actualObj.aabb[3], actualObj.aabb[2]);
		Gizmos.DrawLine(actualObj.aabb[4], actualObj.aabb[5]);
		Gizmos.DrawLine(actualObj.aabb[5], actualObj.aabb[7]);
		Gizmos.DrawLine(actualObj.aabb[5], actualObj.aabb[1]);
		Gizmos.DrawLine(actualObj.aabb[6], actualObj.aabb[2]);
		Gizmos.DrawLine(actualObj.aabb[6], actualObj.aabb[4]);
		Gizmos.DrawLine(actualObj.aabb[7], actualObj.aabb[3]);
		Gizmos.DrawLine(actualObj.aabb[7], actualObj.aabb[6]);

		Gizmos.color = Color.green;
	}

	public void DrawVert(FrustrumObjects currentObject)
	{
		Gizmos.color = Color.red;

		Mesh mesh = currentObject.meshFilter.sharedMesh;

		if (mesh == null)
		{
			return;
		}

		UnityEngine.Vector3[] vertices = mesh.vertices;

		for (int i = 0; i < vertices.Length; i++)
		{
			Gizmos.DrawSphere(currentObject.gameObject.transform.TransformPoint(vertices[i]), 0.05f);
		}

		Gizmos.color = Color.blue;
	}
}
