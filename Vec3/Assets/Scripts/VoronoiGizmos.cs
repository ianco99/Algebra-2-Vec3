using System.Collections.Generic;
using UnityEngine;
using CustomMath;

[RequireComponent(typeof(Voronoi3D))]
public class VoronoiGizmos : MonoBehaviour
{
    [SerializeField] private float planeDrawSize = 3f;
    [SerializeField] private Color bisectorColor = new Color(0.2f, 0.6f, 1f, 0.5f);
    [SerializeField] private Color boundingColor = new Color(1f, 0.4f, 0f, 0.4f);
    [SerializeField] private Color nodeColor = Color.cyan;

    [Header("Volume Points")]
    [SerializeField] private bool showVolumePoints = true;
    [SerializeField] private float pointStep = 2f;
    [SerializeField] private float pointRadius = 0.08f;

    private Voronoi3D _voronoi;

    private struct CachedPoint
    {
        public Vector3 position;
        public Color color;
    }

    private List<CachedPoint> _cachedPoints = new();
    private Color[] _cellColors;
    private int _cachedNodeCount = -1;
    private float _cachedStep = -1f;
    private Vec3[] _cachedNodePositions;

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
                DrawPlaneGizmo(bp.Plane, bp.Midpoint, planeDrawSize);
        }

        if (_voronoi.BoundingPlanes != null)
        {
            Gizmos.color = boundingColor;
            foreach (MyPlane p in _voronoi.BoundingPlanes)
                DrawBoundingPlaneGizmo(p, planeDrawSize * 2f);
        }

        if (showVolumePoints)
            DrawVolumePoints();
    }

    private void DrawVolumePoints()
    {
        if (_voronoi.Nodes == null || _voronoi.Nodes.Length == 0) return;

        if (IsDirty())
            RebuildCache();

        foreach (CachedPoint cp in _cachedPoints)
        {
            Gizmos.color = cp.color;
            Gizmos.DrawSphere(cp.position, pointRadius);
        }
    }

    private bool IsDirty()
    {
        if (_cachedNodeCount != _voronoi.Nodes.Length) return true;
        if (!Mathf.Approximately(_cachedStep, pointStep)) return true;
        if (_cachedNodePositions == null) return true;

        for (int i = 0; i < _voronoi.Nodes.Length; i++)
        {
            if (_voronoi.Nodes[i] != _cachedNodePositions[i])
                return true;
        }

        return false;
    }

    private void RebuildCache()
    {
        _cachedPoints.Clear();

        int nodeCount = _voronoi.Nodes.Length;
        EnsureColors(nodeCount);

        _cachedNodePositions = new Vec3[nodeCount];
        for (int i = 0; i < nodeCount; i++)
            _cachedNodePositions[i] = _voronoi.Nodes[i];

        MyPlane[] bounds = _voronoi.BoundingPlanes;
        if (bounds == null) return;

        float h = 0f;
        foreach (MyPlane p in bounds)
            h = Mathf.Max(h, Mathf.Abs(p.distance));

        float step = Mathf.Max(0.1f, pointStep);

        for (float x = -h; x <= h; x += step)
        {
            for (float y = -h; y <= h; y += step)
            {
                for (float z = -h; z <= h; z += step)
                {
                    Vec3 pos = new Vec3(x, y, z);
                    if (!_voronoi.IsInsideBounds(pos)) continue;

                    int cell = _voronoi.GetCellIndex(pos);
                    Color c = cell >= 0 ? _cellColors[cell] : Color.gray;

                    _cachedPoints.Add(new CachedPoint
                    {
                        position = new Vector3(x, y, z),
                        color = c
                    });
                }
            }
        }

        _cachedNodeCount = nodeCount;
        _cachedStep = pointStep;
    }

    private void EnsureColors(int count)
    {
        if (_cellColors != null && _cellColors.Length == count) return;

        _cellColors = new Color[count];
        for (int i = 0; i < count; i++)
            _cellColors[i] = Color.HSVToRGB((float)i / count, 0.85f, 0.95f);
    }

    private void DrawPlaneGizmo(MyPlane plane, Vec3 midpoint, float size)
    {
        Vector3 n = plane.normal;
        Vector3 center = midpoint;

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

    private void DrawBoundingPlaneGizmo(MyPlane plane, float size)
    {
        Vector3 n = plane.normal;
        Vector3 center = -n * plane.distance;

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