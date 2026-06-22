using System.Collections.Generic;
using UnityEngine;

public class BezierCurve : MonoBehaviour
{
    public Vector3 start;      
    public Vector3 end;         
    public Vector3 control;    
    public int segments = 20;   

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 previousPoint = start;
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 pointOnCurve = CalculateQuadraticBezierPoint(t, start, control, end);
            Gizmos.DrawLine(previousPoint, pointOnCurve);
            previousPoint = pointOnCurve;
        }
    }

    private static Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        return (u * u * p0) + (2 * u * t * p1) + (t * t * p2);
    }
    public static List<Vector3> GetPath(Vector3 start,Vector3 end, Vector3 control,int segments)
    {
        List<Vector3> paths = new List<Vector3>();
        Vector3 previousPoint = start;
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 pointOnCurve = CalculateQuadraticBezierPoint(t, start, control, end);
            previousPoint = pointOnCurve;
            paths.Add(pointOnCurve);
        }
        return paths;
    }
}
