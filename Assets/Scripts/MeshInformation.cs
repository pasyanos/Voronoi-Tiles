using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * Stores the information for a single face of the terrain map. A list of these should be passed
 * along to TerrainMeshInformation which will conglomerate it and then pass it to a mesh.
 */ 
public class MeshInformation
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

    public int vertexCount
    {
        get { return _vertices.Count; }
    }

    public MeshInformation(List<Vector2> polygon, float yOffset, Vector3 lowerLeftCorner)
    {
        _vertices = To3D(polygon, yOffset, lowerLeftCorner);
        _triangles = TriangulatePoly(polygon.Count);
    }

    private static List<Vector3> To3D(List<Vector2> points2D, float yVal, Vector3 lowerLeftCorner)
    {
        // list of 3d points to return
        List<Vector3> retPoints = new List<Vector3>();

        foreach (var point in points2D)
        {
            float x = lowerLeftCorner.x + point.x;
            float y = lowerLeftCorner.y + yVal;
            float z = lowerLeftCorner.z + point.y;
            retPoints.Add(new Vector3(x, y, z));
        }

        return retPoints;
    }

    // given a count of vertices, return a triangulation that properly shares vertices among triangles.
    private static List<int> TriangulatePoly(int count)
    {
        var ret = new List<int>();
        
        if (count < 3)
        {
            Debug.LogError("Trying to triangulate with fewer than 3 points. Returning null");
            return ret;
        }

        for (int i = 2; i < count; i++)
        {
            ret.Add(0);
            ret.Add(i - 1);
            ret.Add(i);
        }

        return ret;
    }
}
