using UnityEngine;
using CustomMath;

public class VoronoiTracker : MonoBehaviour
{
    [SerializeField] private Voronoi3D voronoi;

    private int _currentCell = -1;

    void Update()
    {
        if (voronoi == null) return;

        Vec3 pos = new Vec3(transform.position);

        if (!voronoi.IsInsideBounds(pos))
        {
            Debug.Log("[VoronoiTracker] Fuera del espacio valido.");
            return;
        }

        int cell = voronoi.GetCellIndex(pos);

        if (cell != _currentCell)
        {
            _currentCell = cell;
            Debug.Log($"[VoronoiTracker] Punto de interes mas cercano: nodo {_currentCell}");
        }
    }

    void OnDrawGizmos()
    {
        if (voronoi == null || voronoi.Nodes == null) return;

        Vec3 pos = new Vec3(transform.position);
        int nearest = -1;
        float minDist = float.MaxValue;

        for (int i = 0; i < voronoi.Nodes.Length; i++)
        {
            float d = Vec3.Distance(pos, voronoi.Nodes[i]);
            if (d < minDist)
            {
                minDist = d;
                nearest = i;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, voronoi.Nodes[i]);
        }

        if (nearest >= 0)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, voronoi.Nodes[nearest]);
            Gizmos.DrawSphere(voronoi.Nodes[nearest], 0.12f);
        }
    }
}
