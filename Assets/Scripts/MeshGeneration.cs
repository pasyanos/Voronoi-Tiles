using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Delaunay.Geo;
using Unity.Properties;


public class MeshGeneration : MonoBehaviour
{
    [Header("Debug Stuff")]
    [SerializeField] private bool showTriangulation = true;
    [SerializeField] private bool delauneyMesh = false;

    [Header("Mesh Generation")]
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private Vector3 meshOrigin = Vector3.zero;

    [Header("Terrain Type Settings")]
    [SerializeField] private TerrainSetting waterSetting;
    [SerializeField] private TerrainSetting shoreSetting;
    [SerializeField] private TerrainSetting groundSetting;
    [SerializeField] private TerrainSetting lowMountSetting;
    [SerializeField] private TerrainSetting highMountSetting;

    // Runtime vars
    private bool wasInit = false;
    // private GenerationData genData;

    // information for generation 
    private List<Vector2> points2D;
    private List<TerrainType> terrainTypes;

    // the 3D point of the lower left corner of the voronoi diagram
    // this is used to convert 2D diagram points to 3D points in worldspace
    private Vector3 lowerLeftCorner;
    private Rect bounds;
    private int numPoints;

    private List<LineSegment> _voronoiEdges = null;
    private List<LineSegment> _triangulation = null;
    private Delaunay.Voronoi _voronoi;

    // Commenting this out because it's expensive - can comment back in if more debug is needed
    //private void OnDrawGizmos()
    //{
    //    if (wasInit)
    //    {
    //        Gizmos.color = Color.magenta;

    //        Vector3 upperLeft = ProjectToXZPlane(new Vector2(0, bounds.y), lowerLeftCorner);
    //        Vector3 upperRight = ProjectToXZPlane(new Vector2(bounds.x, bounds.y), lowerLeftCorner);
    //        Vector3 lowerRight = ProjectToXZPlane(new Vector2(bounds.x, 0), lowerLeftCorner);

    //        Gizmos.DrawLine(lowerLeftCorner, upperLeft);
    //        Gizmos.DrawLine(upperLeft, upperRight);
    //        Gizmos.DrawLine(upperRight, lowerRight);
    //        Gizmos.DrawLine(lowerRight, lowerLeftCorner);

    //        if (_voronoiEdges != null)
    //        {
    //            Gizmos.color = Color.white;

    //            for (int i = 0; i < _voronoiEdges.Count; i++)
    //            {
    //                var left3D = ProjectToXZPlane((Vector2)_voronoiEdges[i].p0, lowerLeftCorner);
    //                var right3D = ProjectToXZPlane((Vector2)_voronoiEdges[i].p1, lowerLeftCorner);
    //                Gizmos.DrawLine(left3D, right3D);
    //            }
    //        }

    //        //if (_triangulation != null && showTriangulation)
    //        //{
    //        //    Gizmos.color = Color.green;
    //        //    for (int i = 0; i < _triangulation.Count; i++)
    //        //    {
    //        //        var left3D = ProjectToXZPlane((Vector2)_triangulation[i].p0, lowerLeft);
    //        //        var right3D = ProjectToXZPlane((Vector2)_triangulation[i].p1, lowerLeft);
    //        //        Gizmos.DrawLine(left3D, right3D);
    //        //    }
    //        //}

    //        Gizmos.color = Color.magenta;

    //        foreach (var posn in points2D)
    //        {
    //            var posn3D = ProjectToXZPlane(posn, lowerLeftCorner);
    //            Gizmos.DrawSphere(posn3D, 0.05f);
    //        }
    //    }
    //}

    public void Init(Vector2Int gridDimensions, Vector2 tileSize, TerrainType[,] terrainGrid, Vector2[,] posnOffsets)
    {
        wasInit = false;

        // genData = data;
        float lenX = (gridDimensions.x + 1) * tileSize.x;
        float lenY = (gridDimensions.y + 1) * tileSize.y;
        bounds = new Rect(0, 0, lenX, lenY);

        lowerLeftCorner = new Vector3(meshOrigin.x - lenX * 0.5f, meshOrigin.y, meshOrigin.z - lenY * 0.5f);

        // translate 2D arrays to lists
        points2D = new List<Vector2>();
        terrainTypes = new List<TerrainType>();

        Vector2 startingPt = new Vector2(tileSize.x * 0.5f, tileSize.y * 0.5f);
        Vector2 thisOffset;

        for (int i = 0; i < gridDimensions.x; i++)
        {
            for (int j = 0; j < gridDimensions.y; j++)
            {
                thisOffset = new Vector2(i * tileSize.x, j * tileSize.y);

                points2D.Add(startingPt + thisOffset + posnOffsets[i, j]);
                terrainTypes.Add(terrainGrid[i, j]);
            }
        }

        numPoints = points2D.Count;

        _voronoi = new Delaunay.Voronoi(points2D, null, bounds);
        _voronoiEdges = _voronoi.VoronoiDiagram();

        // This is not currently needed
        // _triangulation = _voronoi.DelaunayTriangulation();

        GenerateMeshVoronoi();

        wasInit = true;
    }

    private void GenerateMeshVoronoi()
    {
        TerrainMeshInformation terrain = new TerrainMeshInformation();

        if (_voronoi != null)
        {
            for (int i = 0; i < numPoints; i++)
            {
                TerrainType curType = terrainTypes[i];
                float yOffset = Setting(curType).GetYValue();
                Vector2 curLocation = points2D[i];

                List<Vector2> polygonForSite = _voronoi.Region(curLocation);

                // voronoi.region returns points in counterclockwise order, need clockwise order for meshes
                // is there a less stupid way to do this?
                polygonForSite.Reverse();

                MeshInformation newInfo = new MeshInformation(polygonForSite, yOffset, lowerLeftCorner);
                
                terrain.AddMeshInfo(newInfo);
            }
        }

        var generatedMesh = new Mesh() { name = "Procedural Mesh" };
        meshFilter.mesh = generatedMesh;
        generatedMesh.vertices = terrain.vertices;
        generatedMesh.triangles = terrain.triangles;
    }

    private void GenerateMeshDelauney()
    {
        // todo:
    }

    #region Misc Helpers
    // This is used by the gizmos function
    private static Vector3 ProjectToXZPlane(Vector2 rectPosn, Vector3 lowerLeftPoint)
    {
        float x = lowerLeftPoint.x + rectPosn.x;
        float y = lowerLeftPoint.y;
        float z = lowerLeftPoint.z + rectPosn.y;
        return new Vector3(x, y, z);
    }

    private TerrainSetting Setting(TerrainType tType)
    {
        switch (tType)
        {
            case TerrainType.MOUNTAIN:
                return highMountSetting;
            case TerrainType.LOWMOUNTAIN:
                return lowMountSetting;
            case TerrainType.GROUND:
                return groundSetting;
            case TerrainType.SHORE:
                return shoreSetting;
            default:
                return waterSetting;
        }
    }
    #endregion
}
