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