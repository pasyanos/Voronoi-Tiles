using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainMeshInformation 
{
    private List<Vector3> _vertices;
    public Vector3[] vertices
    {
        get { return _vertices.ToArray(); }
    }

    private List<int> _triangles;
    public int[] triangles
    {
        get { return _triangles.ToArray(); }
    }

    private int _count;
    public int triangleCount
    {
        get { return _count / 3; }
    }

    private List<Color> _colors;
    public Color[] colors
    {
        get { return _colors.ToArray(); }
    }
    
    public TerrainMeshInformation()
    {
        _vertices = new List<Vector3>();
        _triangles = new List<int>();
        _colors = new List<Color>();
        _count = 0;
    }

    public void AddTopFaceMeshInfo(TopFaceMeshInformation meshInfo)
    {
        // first, add all new verts to vertex list
        Vector3[] newVerts = meshInfo.vertices;
        int[] triIndices = meshInfo.triangles;

        _vertices.AddRange(meshInfo.vertices);
        _colors.AddRange(meshInfo.colors);

        // then add triangle indexing, but offset it by the count so far
        for (int i = 0; i < meshInfo.triangles.Length; i++)
        {
            _triangles.Add(triIndices[i] + _count);
        }

        // finally, update the count
        _count += meshInfo.vertexCount;
    }
}
