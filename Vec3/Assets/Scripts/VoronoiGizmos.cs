using UnityEngine;
using CustomMath;

[RequireComponent(typeof(Voronoi3D))]
public class VoronoiGizmos : MonoBehaviour
{
    [SerializeField] private float planeDrawSize = 3f;
    [SerializeField] private Color bisectorColor = new Color(0.2f, 0.6f, 1f, 0.5f);
    [SerializeField] private Color boundingColor = new Color(1f, 0.4f, 0f, 0.4f);
    [SerializeField] private Color nodeColor = Color.cyan;

    private Voronoi3D _voronoi;

    void Awake()
    {
        _voronoi = GetComponent<Voronoi3D>();
    }

    void OnDrawGizmos()
    {
        if (_voronoi == null) _voronoi = GetComponent<Voronoi3D>();
        if (_voronoi == null) return;

        if (_voronoi.Nodes != null)
        {
            Gizmos.color = nodeColor;
            foreach (Vec3 n in _voronoi.Nodes)
                Gizmos.DrawSphere(n, 0.18f);
        }

        if (_voronoi.BisectorPlanes != null)
        {
            Gizmos.color = bisectorColor;
            foreach (BisectorPlane bp in _voronoi.BisectorPlanes)
                DrawPlaneGizmo(bp.Plane, planeDrawSize);
        }

        if (_voronoi.BoundingPlanes != null)
        {
            Gizmos.color = boundingColor;
            foreach (MyPlane p in _voronoi.BoundingPlanes)
                DrawPlaneGizmo(p, planeDrawSize * 2f);
        }
    }

    private void DrawPlaneGizmo(MyPlane plane, float size)
    {
        Vec3 normal = plane.normal;
        Vector3 n = normal;
        Vector3 center = n * plane.distance;

        Vector3 tangent = Mathf.Abs(Vector3.Dot(n, Vector3.up)) < 0.99f
            ? Vector3.Cross(n, Vector3.up).normalized
            : Vector3.Cross(n, Vector3.right).normalized;

        Vector3 bitangent = Vector3.Cross(n, tangent);

        Vector3 c0 = center + (tangent + bitangent) * size;
        Vector3 c1 = center + (tangent - bitangent) * size;
        Vector3 c2 = center + (-tangent - bitangent) * size;
        Vector3 c3 = center + (-tangent + bitangent) * size;

        Gizmos.DrawLine(c0, c1);
        Gizmos.DrawLine(c1, c2);
        Gizmos.DrawLine(c2, c3);
        Gizmos.DrawLine(c3, c0);
        Gizmos.DrawLine(center, center + n * size * 0.4f);
    }
}
