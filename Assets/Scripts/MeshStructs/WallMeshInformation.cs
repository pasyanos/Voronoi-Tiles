using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WallMeshInformation
{
    // storing vertices, tris, etc, as lists privately, so I can easily add to them.
    // publicly exposed getters use 1D arrays because this is what the mesh needs.
    private List<Vector3> _vertices = new List<Vector3>();
    public Vector3[] vertices
    {
        get { return _vertices.ToArray(); }
    }

    private List<int> _triangles = new List<int>();
    public int[] triangles
    {
        get { return _triangles.ToArray(); }
    }

    private List<Color> _colors = new List<Color>();
    public Color[] colors
    {
        get { return _colors.ToArray(); }
    }

    private int _vertexCount = 0;
    public int vertexCount
    {
        get { return _vertexCount; }
    }

    public WallMeshInformation(Vector2 fromPt, Vector2 toPoint, float topY, float bottomY, Color upColor, Color downColor, Vector3 lowerLeftCorner)
    {
        // form wall points
        Vector3 upFrom = new Vector3(fromPt.x, topY, fromPt.y) + lowerLeftCorner;
        Vector3 upTo = new Vector3(toPoint.x, topY, toPoint.y) + lowerLeftCorner;
        Vector3 downTo = new Vector3(toPoint.x, bottomY, toPoint.y) + lowerLeftCorner;
        Vector3 downFrom = new Vector3(fromPt.x, bottomY, fromPt.y) + lowerLeftCorner;

        //_vertices.Add(upFrom);
        //_vertices.Add(upTo);
        //_vertices.Add(downTo);
        //_vertices.Add(downFrom);
        _vertices.Add(upTo);
        _vertices.Add(upFrom);
        _vertices.Add(downFrom);
        _vertices.Add(downTo);

        _vertexCount = 4;

        _triangles.AddRange(new int[] { 0, 1, 2, 0, 2, 3 });
        _colors.AddRange(new Color[] { upColor, upColor, downColor, downColor});
    }
}
