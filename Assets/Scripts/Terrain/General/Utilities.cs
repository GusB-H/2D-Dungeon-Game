using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static Vector2 SnapToLineSegment(Vector2 point, Vector2 endpoint1, Vector2 endpoint2)
    {
        if(endpoint1 == endpoint2) return endpoint1;
        float dotProduct = Vector2.Dot(endpoint2 - endpoint1, point - endpoint1);
        float snapPoint = Mathf.Clamp01(dotProduct / (endpoint2 - endpoint1).sqrMagnitude);
        return endpoint1 + snapPoint * (endpoint2 - endpoint1);
    }

    public static Vector2 OffsetFromLineSegment(Vector2 point, Vector2 endpoint1, Vector2 endpoint2)
    {
        return point - SnapToLineSegment(point, endpoint1, endpoint2);
    }

    public static float SqrDistanceFromLineSegment(Vector2 point, Vector2 endpoint1, Vector2 endpoint2)
    {
        return OffsetFromLineSegment(point, endpoint1, endpoint2).sqrMagnitude;
    }

    public static float DistanceFromLineSegment(Vector2 point, Vector2 endpoint1, Vector2 endpoint2)
    {
        return Mathf.Sqrt(DistanceFromLineSegment(point, endpoint1, endpoint2));
    }

    public static float GridDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    }

    public static Vector2 LockToRectangle(Vector2 input)
    {
        return input / Mathf.Max(Mathf.Abs(input.x), Mathf.Abs(input.y));
    }
    public static Vector2 LockToRectangle(Vector2 input, Vector2 rectangle)
    {
        return input / Mathf.Max(Mathf.Abs(input.x) / rectangle.x, Mathf.Abs(input.y) / rectangle.y);
    }


    private static System.Random rng = new System.Random();
    public static void Shuffle<T>(T[] array)
    {
        rng = new System.Random();
        int n = array.Length;
        while (n > 1)
        {
            int k = rng.Next(n);
            n--;
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }
}
