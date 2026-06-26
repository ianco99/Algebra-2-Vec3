# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Unity 6.0.3.3f1 project using URP. All source lives under `Vec3/Assets/Scripts/`. Open and run through the Unity Editor — there is no CLI build command used in development.

**Scenes:**
- `Assets/Scenes/3DVoronoi.unity` — Voronoi diagram demo

## Architecture

### Custom Math Layer (`CustomMath` namespace)
`MathDebbuger/Vec3.cs` and `MathDebbuger/MyPlane.cs` are custom re-implementations of Unity's `Vector3` and `Plane`. All geometric algorithms in this project use these types instead of Unity builtins — this is intentional for pedagogical reasons. `Vec3` has implicit casts to `Vector3`/`Vector2` for Unity API interop.

### Visualization Infrastructure (`MathDebbuger` namespace)
`Vector3Debugger.cs` is a static API for drawing labeled vector arrows in the scene. It delegates to `CameraDebugger` (GL rendering + cone meshes for arrowheads) and `VectorHandles` (2D label overlays). Vectors are identified by string keys. `Tester.cs` shows usage.

### Voronoi System
`Voronoi3D.cs` builds a 3D Voronoi diagram from `nodeTransforms[]` and creates a `BisectorPlane` (midplane) for each pair of nodes. `IsPointInCell(point, nodeIndex)` tests a point against all planes associated with that node. `VoronoiGizmos.cs` and `VoronoiTracker.cs` are visualization/query helpers that depend on a `Voronoi3D` reference.

## Key Conventions

- No comments in code (project convention).
- Use `CustomMath.Vec3` and `CustomMath.MyPlane` for all geometry, not `UnityEngine.Vector3` or `UnityEngine.Plane`.
- Gizmo drawing (`OnDrawGizmos`) is the primary debugging/visualization tool — use it freely.
- `Vector3Debugger` string IDs must be unique per vector/sequence; reuse the same ID to update rather than re-add.
