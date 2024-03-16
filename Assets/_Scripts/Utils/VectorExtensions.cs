using UnityEngine;

public static class VectorExtensions
{
    public static Vector3 XZPlane(this Vector3 vec, float y = 0) => new Vector3(vec.x,y,vec.z);

    public static Vector3 YZPlane(this Vector3 vec) => new Vector3(0,vec.y,vec.z);

    public static Vector3 XYPlane(this Vector3 vec) => new Vector3(vec.x,vec.y,0);

    public static Vector3 SetX(this Vector3 vec, float x) => new Vector3(x,vec.y,vec.z);

    public static Vector3 SetY(this Vector3 vec, float y) => new Vector3(vec.x,y,vec.z);

    public static Vector3 SetZ(this Vector3 vec, float z) => new Vector3(vec.x,vec.y,z);
}