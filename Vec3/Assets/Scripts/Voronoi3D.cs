using System.Collections.Generic;
using UnityEngine;
using CustomMath;

public class Voronoi3D : MonoBehaviour
{
    [SerializeField] private Transform[] nodeTransforms;
    [SerializeField] private float boundingBoxSize = 20f;

    private Vec3[] _nodes;
    private List<BisectorPlane> _bisectorPlanes = new();
    private Dictionary<int, List<int>> _nodePlaneMap = new();
    private MyPlane[] _boundingPlanes;

    public Vec3[] Nodes => _nodes;
    public List<BisectorPlane> BisectorPlanes => _bisectorPlanes;
    public MyPlane[] BoundingPlanes => _boundingPlanes;

    void Start()
    {
        Build();
    }

    public void Build()
    {
        _bisectorPlanes.Clear();
        _nodePlaneMap.Clear();
        _nodes = GatherNodes();
        _boundingPlanes = BuildBoundingBox(boundingBoxSize);

        for (int i = 0; i < _nodes.Length; i++)
            _nodePlaneMap[i] = new List<int>();

        for (int i = 0; i < _nodes.Length; i++)
        {
            for (int j = i + 1; j < _nodes.Length; j++)
            {
                Vec3 dir = _nodes[j] - _nodes[i];
                Vec3 mid = (_nodes[i] + _nodes[j]) * 0.5f;
                MyPlane plane = new MyPlane(dir, mid);

                int id = _bisectorPlanes.Count;
                _bisectorPlanes.Add(new BisectorPlane(i, j, plane, mid));
                _nodePlaneMap[i].Add(id);
                _nodePlaneMap[j].Add(id);
            }
        }
    }

    private Vec3[] GatherNodes()
    {
        if (nodeTransforms == null) return new Vec3[0];
        Vec3[] arr = new Vec3[nodeTransforms.Length];
        for (int i = 0; i < nodeTransforms.Length; i++)
            arr[i] = new Vec3(nodeTransforms[i].position);
        return arr;
    }

    public bool IsPointInCell(int nodeIndex, Vec3 point)
    {
        if (!_nodePlaneMap.ContainsKey(nodeIndex)) return false;

        foreach (int pi in _nodePlaneMap[nodeIndex])
        {
            BisectorPlane bp = _bisectorPlanes[pi];
            float dist = bp.Plane.GetDistanceToPoint(point);
            bool onNodeSide = nodeIndex == bp.NodeA ? dist <= 0f : dist >= 0f;
            if (!onNodeSide) return false;
        }
        return true;
    }

    public int GetCellIndex(Vec3 point)
    {
        for (int i = 0; i < _nodes.Length; i++)
        {
            if (IsPointInCell(i, point))
                return i;
        }
        return -1;
    }

    public bool IsInsideBounds(Vec3 point)
    {
        foreach (MyPlane p in _boundingPlanes)
        {
            if (!p.GetSide(point)) return false;
        }
        return true;
    }

    public static MyPlane[] BuildBoundingBox(float size)
    {
        float h = size * 0.5f;
        return new MyPlane[]
        {
            new MyPlane(new Vec3( 1, 0, 0), new Vec3(-h, 0, 0)),
            new MyPlane(new Vec3(-1, 0, 0), new Vec3( h, 0, 0)),
            new MyPlane(new Vec3(0,  1, 0), new Vec3(0, -h, 0)),
            new MyPlane(new Vec3(0, -1, 0), new Vec3(0,  h, 0)),
            new MyPlane(new Vec3(0, 0,  1), new Vec3(0, 0, -h)),
            new MyPlane(new Vec3(0, 0, -1), new Vec3(0, 0,  h)),
        };
    }
}

public struct BisectorPlane
{
    public int NodeA;
    public int NodeB;
    public MyPlane Plane;
    public Vec3 Midpoint;

    public BisectorPlane(int a, int b, MyPlane plane, Vec3 midpoint)
    {
        NodeA = a;
        NodeB = b;
        Plane = plane;
        Midpoint = midpoint;
    }
}