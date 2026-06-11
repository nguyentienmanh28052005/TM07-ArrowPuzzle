using System.Collections.Generic;
using UnityEngine;

public readonly struct PathWarp
{
    public readonly float rawDistFromHead0;
    public readonly Vector3 teleportOffset;
    public readonly ArrowDir exitDir;
    public readonly Vector3 portalWorldPos;
    public readonly Vector3 exitWorldPos;
    public readonly bool isPortal;
    public readonly GridDeflector deflector;

    public PathWarp(
        float rawDistFromHead0,
        Vector3 teleportOffset,
        ArrowDir exitDir,
        Vector3 portalWorldPos,
        Vector3 exitWorldPos,
        bool isPortal,
        GridDeflector deflector)
    {
        this.rawDistFromHead0 = rawDistFromHead0;
        this.teleportOffset = teleportOffset;
        this.exitDir = exitDir;
        this.portalWorldPos = portalWorldPos;
        this.exitWorldPos = exitWorldPos;
        this.isPortal = isPortal;
        this.deflector = deflector;
    }
}

public readonly struct PathSegment
{
    public readonly Vector3 startWorld;
    public readonly ArrowDir dir;
    public readonly int steps;
    public readonly bool startsFromPortal;
    public readonly bool endsInPortal;

    public PathSegment(Vector3 startWorld, ArrowDir dir, int steps, bool startsFromPortal, bool endsInPortal)
    {
        this.startWorld = startWorld;
        this.dir = dir;
        this.steps = steps;
        this.startsFromPortal = startsFromPortal;
        this.endsInPortal = endsInPortal;
    }
}

public sealed class MoveResult
{
    public static readonly MoveResult Clear = new MoveResult(float.MaxValue, ObstacleHit.None, new List<PathWarp>(0));

    public float DistanceToObstacle { get; }
    public ObstacleHit Hit { get; }
    public List<PathWarp> Warps { get; }

    public bool IsClear => Hit.Type == ObstacleHitType.None;
    public bool CanRelease => DistanceToObstacle == float.MaxValue || Hit.Type == ObstacleHitType.BlackHole;

    public MoveResult(float distanceToObstacle, ObstacleHit hit, List<PathWarp> warps)
    {
        DistanceToObstacle = distanceToObstacle;
        Hit = hit;
        Warps = warps ?? new List<PathWarp>(0);
    }
}
