# Voronoi Diagram System Documentation

## Table of Contents
1. [Custom Math Foundation](#custom-math-foundation)
2. [Voronoi Diagram Theory](#voronoi-diagram-theory)
3. [Bisector Planes](#bisector-planes)
4. [Core Algorithms](#core-algorithms)
5. [Implementation Details](#implementation-details)
6. [Visualization System](#visualization-system)

---

## Custom Math Foundation

The Voronoi system relies entirely on custom mathematical primitives: `Vec3` (3D vectors) and `MyPlane` (3D planes). This pedagogical constraint ensures deep understanding of geometric operations.

### Vec3 Structure

Located in: `MathDebbuger/Vec3.cs`

**Purpose**: 3D vector representation with all geometric operations implemented from scratch.

**Core Properties**:
```csharp
public float x, y, z;           // Vector components
public float magnitude           // √(x² + y² + z²)
public float sqrMagnitude        // x² + y² + z²
public Vec3 normalized           // Unit vector in same direction
```

**Critical Operations for Voronoi**:

| Operation | Formula | Usage |
|-----------|---------|-------|
| **Distance** | √((a.x-b.x)² + (a.y-b.y)² + (a.z-b.z)²) | Point-to-point distance |
| **Dot Product** | a.x·b.x + a.y·b.y + a.z·b.z | Plane normal calculations |
| **Cross Product** | (a.y·b.z - a.z·b.y, a.z·b.x - a.x·b.z, a.x·b.y - a.y·b.x) | Plane normal from 3 points |
| **Normalize** | v / \|v\| | Ensure unit-length normals |
| **Midpoint** | (a + b) × 0.5 | Bisector plane center |

**Implicit Conversions**:
- `Vec3` → `Vector3`: Seamless bridge to Unity APIs
- `Vec3` → `Vector2`: For 2D operations (z=0)

---

### MyPlane Structure

Located in: `MathDebbuger/MyPlane.cs`

**Purpose**: 3D plane representation in normal-distance form. A plane divides 3D space into two half-spaces.

**Core Representation**:
```csharp
public Vec3 normal;      // Unit normal vector pointing to positive side
public float distance;   // Signed distance from origin: -dot(normal, point_on_plane)
```

**Plane Equation**: 
```
dot(normal, point) + distance = 0  // Point is on plane
dot(normal, point) + distance > 0  // Point on positive side
dot(normal, point) + distance < 0  // Point on negative side
```

**Construction Methods**:

1. **From Normal + Point**:
   ```csharp
   MyPlane(Vec3 normal, Vec3 point)
   // Creates plane through 'point' with given normal
   // Internal: distance = -dot(normal, point)
   ```

2. **From Normal + Distance**:
   ```csharp
   MyPlane(Vec3 normal, float distance)
   // Direct form (normal must be normalized externally)
   ```

3. **From 3 Points**:
   ```csharp
   MyPlane(Vec3 a, Vec3 b, Vec3 c)
   // Computes normal via cross product: normalize(cross(b-a, c-a))
   // Ensures consistent orientation
   ```

**Key Methods for Voronoi**:

| Method | Returns | Purpose |
|--------|---------|---------|
| `GetDistanceToPoint(point)` | float | Signed distance from point to plane |
| `GetSide(point)` | bool | Is point on positive side? |
| `SameSide(p1, p2)` | bool | Both points same side of plane? |
| `ClosestPointOnPlane(point)` | Vec3 | Project point onto plane |
| `flipped` | MyPlane | Flip normal (reverse positive side) |

---

## Voronoi Diagram Theory

### What Is a Voronoi Diagram?

A **Voronoi diagram** partitions 3D space based on proximity to a set of **nodes** (seed points). Each node N gets a **Voronoi cell**—the region of space closer to N than to any other node.

**Mathematical Definition**:
```
Cell(N_i) = {P in ℝ³ : distance(P, N_i) < distance(P, N_j) ∀j ≠ i}
```

### Cell Boundaries

The boundary between cells of adjacent nodes is the **perpendicular bisector plane**—the set of points equidistant from both nodes.

**2D Example** (easier to visualize):
```
For nodes A and B:
     Cell A  |  Cell B
             |
    A ---•---•---•--- B
       (equal distance points)
```

The dividing line consists of points where `distance(P, A) = distance(P, B)`.

### 3D Voronoi Cells

In 3D, each cell is bounded by multiple bisector planes. A point P is in the cell of node N_i if:
```
distance(P, N_i) < distance(P, N_j) for all j ≠ i
```

Equivalently:
```
P is on the correct side of ALL bisector planes for N_i
```

### Dual Structure: Delaunay Triangulation

The Voronoi diagram has a dual structure called the **Delaunay triangulation**:
- Each Voronoi node corresponds to a Delaunay vertex
- Two nodes form a Delaunay edge if their Voronoi cells share a boundary

---

## Bisector Planes

### Definition

The **bisector plane** (perpendicular bisector) between nodes A and B contains all points equidistant from A and B.

**Construction**:
1. **Midpoint**: M = (A + B) × 0.5
2. **Normal**: N = (B - A) (direction between nodes)
3. **Plane**: Pass through M with normal N

```csharp
Vec3 dir = _nodes[j] - _nodes[i];      // Direction from A to B
Vec3 mid = (_nodes[i] + _nodes[j]) * 0.5f;  // Midpoint
MyPlane plane = new MyPlane(dir, mid);  // Plane through mid with normal dir
```

### Mathematical Verification

For a point P on the bisector plane:
```
distance(P, A) = distance(P, B)
```

This is guaranteed because:
1. The normal is perpendicular to the edge AB
2. The plane passes through the midpoint
3. Any point equidistant from both nodes lies on this plane

### Signed Distance Convention

The plane stores `normal = (B - A)` normalized. This means:
- **Positive side** (distance > 0): Closer to B
- **Negative side** (distance < 0): Closer to A

When checking if P is in cell A:
```
P is in Cell(A) if getDistance(plane, P) <= 0  // P on A's side
```

---

## Core Algorithms

### 1. Build() - Construct Voronoi Diagram

**Purpose**: Create bisector planes for all node pairs and index them by node.

**Steps**:
```
1. Gather node positions from transforms
2. Create bounding box planes (6 axis-aligned planes)
3. For each pair of nodes (i, j):
   a. Create bisector plane
   b. Store plane in list
   c. Add plane index to both nodes' plane lists
```

**Complexity**: O(N²) for N² node pairs

**Output**:
- `_bisectorPlanes`: List of all bisector planes (one per node pair)
- `_nodePlaneMap`: Dictionary mapping node index → list of plane indices

**Code**:
```csharp
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
```

### 2. IsPointInCell(nodeIndex, point) - Point Membership Test

**Purpose**: Determine if a point belongs to a specific node's Voronoi cell.

**Algorithm**: Check if point is on the correct side of ALL planes associated with the node.

**Logic**:
```
For each plane P associated with node N:
  if plane was created for nodes (A, B):
    - If N == A: point must be on negative side (distance <= 0)
    - If N == B: point must be on positive side (distance >= 0)
  if point fails ANY test: return false
return true  // Passed all tests
```

**Complexity**: O(K) where K = number of planes for that node (typically O(log N) to O(N))

**Code**:
```csharp
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
```

### 3. GetCellIndex(point) - Find Containing Cell

**Purpose**: Determine which node a point is closest to (which cell it belongs to).

**Algorithm**: Linear search through all nodes.

**Complexity**: O(N × K) where K = average planes per node

**Code**:
```csharp
public int GetCellIndex(Vec3 point)
{
    for (int i = 0; i < _nodes.Length; i++)
    {
        if (IsPointInCell(i, point))
            return i;
    }
    return -1;  // Outside all cells
}
```

**Optimization Note**: For large N, use spatial hashing or KD-trees to accelerate nearest-node queries.

### 4. IsInsideBounds(point) - Bounding Box Test

**Purpose**: Check if a point is within the axis-aligned bounding box.

**Algorithm**: Test against all 6 bounding planes.

**Code**:
```csharp
public bool IsInsideBounds(Vec3 point)
{
    foreach (MyPlane p in _boundingPlanes)
    {
        if (!p.GetSide(point)) return false;
    }
    return true;
}
```

### 5. BuildBoundingBox(size) - Create Boundary Planes

**Purpose**: Create 6 axis-aligned planes forming a cube.

**Construction**:
```
For size = 10:
  h = 5 (half-size)
  
  Planes (normal, point):
  1. X-min: normal=(1,0,0), point=(-5,0,0)
  2. X-max: normal=(-1,0,0), point=(5,0,0)
  3. Y-min: normal=(0,1,0), point=(0,-5,0)
  4. Y-max: normal=(0,-1,0), point=(0,5,0)
  5. Z-min: normal=(0,0,1), point=(0,0,-5)
  6. Z-max: normal=(0,0,-1), point=(0,0,5)
```

---

## Implementation Details

### BisectorPlane Structure

```csharp
public struct BisectorPlane
{
    public int NodeA;        // First node (normal points toward B)
    public int NodeB;        // Second node
    public MyPlane Plane;    // The actual plane (normal = B - A)
    public Vec3 Midpoint;    // Center of edge (for visualization)
}
```

### _nodePlaneMap Indexing

Maps each node to its associated plane indices:
```
_nodePlaneMap[0] = [5, 12, 18]  // Node 0 has planes at indices 5, 12, 18
_nodePlaneMap[1] = [5, 7, 9, 15]
...
```

This enables fast point-in-cell tests without iterating all planes.

---

## Visualization System

### VoronoiGizmos - Debug Visualization

Located in: `VoronoiGizmos.cs`

**Purpose**: Draw Voronoi structure in Unity Scene view using Gizmos.

**Visualized Elements**:

| Element | Color | Meaning |
|---------|-------|---------|
| **Nodes** | Cyan | Seed points |
| **Bisector Planes** | Light Blue | Plane boundaries between cells |
| **Bounding Box** | Orange | Space boundaries |
| **Volume Points** | Per-cell colors | Sample points colored by cell |

**DrawVolumePoints() Algorithm**:
1. Sample grid of points across space (configurable step size)
2. For each point: determine which cell it belongs to
3. Color point according to its cell
4. Draw as small spheres in scene

**Performance**:
- Samples: ~(size/step)³ points
- Step = 2 → ~10³ = 1000 points
- Step = 0.5 → ~40³ = 64,000 points (slow but detailed)

**Caching**:
- `RebuildCache()` only runs when nodes or step size change
- `IsDirty()` checks for changes and triggers rebuild

---

### VoronoiTracker - Point Tracking

Located in: `VoronoiTracker.cs`

**Purpose**: Track which cell a moving object is in (e.g., player in game).

**Functionality**:
- Updates each frame to find containing cell
- Logs when entering a new cell
- Draws gizmos:
  - Red lines to all nodes (distances)
  - Yellow line to nearest node
  - Highlights nearest node

**Usage**:
```
1. Attach VoronoiTracker to a moving GameObject
2. Assign Voronoi3D reference
3. VoronoiTracker automatically finds containing cell each frame
```

---

## Usage Example

```csharp
// Setup
Voronoi3D voronoi = GetComponent<Voronoi3D>();
voronoi.Build();  // Usually called in Start()

// Query a point
Vec3 queryPoint = new Vec3(5, 3, 2);

// Check bounds
if (!voronoi.IsInsideBounds(queryPoint))
{
    Debug.Log("Outside bounds");
    return;
}

// Find containing cell
int cellIndex = voronoi.GetCellIndex(queryPoint);
if (cellIndex >= 0)
{
    Debug.Log($"Point is in cell {cellIndex}");
    Vec3 nearestNode = voronoi.Nodes[cellIndex];
}
else
{
    Debug.Log("Point not in any cell");
}

// Direct membership test
if (voronoi.IsPointInCell(5, queryPoint))
{
    Debug.Log("Point is definitely in cell 5");
}
```

---

## Performance Characteristics

| Operation | Complexity | Notes |
|-----------|-----------|-------|
| `Build()` | O(N²) | N² pairs, one plane per pair |
| `IsPointInCell()` | O(N) | N planes per node (all pairs) |
| `GetCellIndex()` | O(N²) | N nodes, N planes per node |
| `IsInsideBounds()` | O(1) | Fixed 6 planes |
| `DrawVolumePoints()` | O(V·N²) | V sample points, N² planes |

**Optimization Strategies**:
- Cache cell queries for static points
- Use spatial partitioning for large N
- Reduce sample density in visualization
- Pre-compute frequent queries

---

## Edge Cases & Gotchas

1. **Epsilon Comparisons**: `Vec3.epsilon = 1e-5` accounts for floating-point errors
2. **Coplanar Nodes**: Identical nodes cause degenerate planes; check for duplicates
3. **Collinear Nodes**: May result in zero-area cells
4. **Unbounded Cells**: Points outside bounding box return -1
5. **Normal Orientation**: Gabriel's test assumes consistent normal directions

---

## References

- **Gabriel Graph**: Gabriel, K. R., & Sokal, R. R. (1969). A new statistical approach to geographic variation analysis.
- **Voronoi Diagrams**: Aurenhammer, F. (1991). Voronoi diagrams—a survey of fundamental geometric data structures.
- **3D Computational Geometry**: de Berg, M., et al. (2008). Computational Geometry: Algorithms and Applications (3rd ed.).
