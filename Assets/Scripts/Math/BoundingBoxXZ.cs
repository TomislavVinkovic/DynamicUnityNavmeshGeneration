// Bounding box with some useful properties
using System;
using UnityEngine;

/************************************************************************************
 * This class is used to represent a bounding box in 2D space (x, z)
 ************************************************************************************/
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

    public bool Intersects(BoundingBoxXZ other) {
        // Check for overlap on the x-axis
        bool xOverlap = this.minX <= other.maxX && this.maxX >= other.minX;

        // Check for overlap on the z-axis
        bool zOverlap = this.minZ <= other.maxZ && this.maxZ >= other.minZ;

        // They intersect if they overlap on both axes
        return xOverlap && zOverlap;
    }

    public bool Intersects(Vector3 point) {
        return point.x >= minX && point.x <= maxX && point.z >= minZ && point.z <= maxZ;
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
}