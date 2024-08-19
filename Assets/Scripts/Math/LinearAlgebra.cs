using UnityEngine;

// Linear algebra constants and operations
public static class LinearAlgebra {

    public static Vector3 XAxis { get => Vector3.right; }
    public static Vector3 YAxis { get => Vector3.up; }

    public static Vector3 ZAxis { get => new Vector3(0, 0, 1); }
}