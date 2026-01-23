using UnityEngine;

public static class VectorExtension
{
    public static Vector2 LerpTo(this Vector2 from, Vector2 to, float t)
        => Vector2.Lerp(from, to, t);

    public static Vector2 LerpToUnclamped(this Vector2 from, Vector2 to, float t)
        => Vector2.LerpUnclamped(from, to, t);

    public static Vector2 MoveTowardsTo(this Vector2 from, Vector2 to, float maxDelta)
        => Vector2.MoveTowards(from, to, maxDelta);

    public static Vector3 LerpTo(this Vector3 from, Vector3 to, float t)
        => Vector3.Lerp(from, to, t);

    public static Vector3 LerpToUnclamped(this Vector3 from, Vector3 to, float t)
        => Vector3.LerpUnclamped(from, to, t);

    public static Vector3 MoveTowardsTo(this Vector3 from, Vector3 to, float maxDelta)
        => Vector3.MoveTowards(from, to, maxDelta);

    public static Vector4 LerpTo(this Vector4 from, Vector4 to, float t)
        => Vector4.Lerp(from, to, t);

    public static Vector4 LerpToUnclamped(this Vector4 from, Vector4 to, float t)
        => Vector4.LerpUnclamped(from, to, t);

    public static Vector4 MoveTowardsTo(this Vector4 from, Vector4 to, float maxDelta)
        => Vector4.MoveTowards(from, to, maxDelta);

    public static Vector2 ToVector2(this Vector3 v)
        => new Vector2(v.x, v.y);

    public static Vector2 ToVector2(this Vector4 v)
        => new Vector2(v.x, v.y);

    public static Vector3 ToVector3(this Vector2 v, float z = 0f)
        => new Vector3(v.x, v.y, z);

    public static Vector3 ToVector3(this Vector4 v)
        => new Vector3(v.x, v.y, v.z);

    public static Vector4 ToVector4(this Vector2 v, float z = 0f, float w = 0f)
        => new Vector4(v.x, v.y, z, w);

    public static Vector4 ToVector4(this Vector3 v, float w = 0f)
        => new Vector4(v.x, v.y, v.z, w);
}
