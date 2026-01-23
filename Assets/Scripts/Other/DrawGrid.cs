using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawGrid : MonoBehaviour
{
    public Vector3 origin = Vector3.zero;
    public int width = 10;
    public int height = 20;
    public float cellSize = 1.0f;
    public Color gridColor = Color.green;

    private void OnDrawGizmos()
    {
        Gizmos.color = gridColor;
        DrawGridCentered(origin, width, height, cellSize);
    }

    public void DrawGridCentered(Vector3 center, int width, int height, float size)
    {
        float totalWidth = width * size;
        float totalHeight = height * size;

        Vector3 bottomLeft = center - new Vector3(totalWidth / 2f, totalHeight / 2f, 0);

        for (int x = 0; x <= width; x++)
        {
            Vector3 start = bottomLeft + new Vector3(x * size, 0, 0);
            Vector3 end = start + new Vector3(0, totalHeight, 0);
            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y <= height; y++)
        {
            Vector3 start = bottomLeft + new Vector3(0, y * size, 0);
            Vector3 end = start + new Vector3(totalWidth, 0, 0);
            Gizmos.DrawLine(start, end);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(center, size * 0.1f);
    }
}