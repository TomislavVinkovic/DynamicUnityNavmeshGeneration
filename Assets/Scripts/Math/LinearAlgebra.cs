using System;
using System.Collections.Generic;
using UnityEngine;

// Linear algebra constants and operations
public static class LinearAlgebra {

    public static Vector3 XAxis { get => Vector3.right; }
    public static Vector3 YAxis { get => Vector3.up; }

    public static Vector3 ZAxis { get => new Vector3(0, 0, 1); }

    public static Vector3 GetMeanInSpace(ICollection<Vector3> points) {
        float sumX = 0;
        float sumY = 0;
        float sumZ = 0;

        foreach (var point in points) {
            sumX += point.x;
            sumY += point.y;
            sumZ += point.z;
        }

        return new Vector3(sumX / points.Count, sumY / points.Count, sumZ / points.Count);
    }
}

// Bounding box with some useful properties
public class BoundingBoxXZ {
    float AGENT_NAVMESH_BOUNDS_Y = 5f;
    public float minX;
    public float maxX;

    public float minZ;
    public float maxZ;

    public Vector3 BottomLeft {
        get => new Vector3(minX, 0, minZ);
    }
    public Vector3 TopLeft {
        get => new Vector3(minX, 0, maxZ);
    }
    public Vector3 BottomRight {
        get => new Vector3(maxX, 0, minZ);
    }
    public Vector3 TopRight {
        get => new Vector3(maxX, 0, maxZ);
    }
    public Vector3 center {
        get => Vector3.Scale(BottomLeft + TopRight, new Vector3(.5f, 0, .5f));
    }
    public Vector3 size {
        get => new Vector3(Math.Abs(maxX - minX), AGENT_NAVMESH_BOUNDS_Y, Math.Abs(maxZ - minZ));
    }

    public BoundingBoxXZ () {
        minX = 1e9f;
        maxX = -1e9f;
        minZ = 1e9f;
        maxZ = -1e9f;
    }
    public BoundingBoxXZ (
        float minX,
        float maxX,
        float minZ,
        float maxZ
    ) {
        this.minX = minX;
        this.maxX = maxX;
        this.minZ = minZ;
        this.maxZ = maxZ;
    }

    public Bounds ToBounds() {
        return new Bounds(center, size);
    }
}