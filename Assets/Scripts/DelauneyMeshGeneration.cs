using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Delaunay.Geo;


public class DelauneyMeshGeneration : MonoBehaviour
{
    public class GenerationData
    {
        public Vector3 _origin;
        public Vector2Int _dimensions;
        public Vector2 _tileSize;

        // these are used by the rectangle class for generation
        public float _lenX;
        public float _lenY;
        // public Vector2[,] points2D;
        public List<Vector2> pointLocs;
        public Rect rect;

        public GenerationData(Vector3 origin, Vector2Int dimensions, Vector2 tileSize, 
            TerrainType[,] terrainTypes, Vector2[,] posnOffsets)
        {
            _origin = origin;
            _dimensions = dimensions;
            _tileSize = tileSize;
            
            // adding 2 to the dimensions gives just a bit of extra wiggle room
            _lenX = (dimensions.x + 2) * tileSize.x;
            _lenY = (dimensions.y + 2) * tileSize.y;

            // points2D = new Vector2[dimensions.x, dimensions.y];
            pointLocs = new List<Vector2>();

            rect = new Rect(0, 0, _lenX, _lenY);

            Vector2 startingPt = new Vector2(_tileSize.x*1.5f, _tileSize.y*1.5f);
            Vector2 thisOffset;

            // translate offsets to 2D positions
            for (int i = 0; i < _dimensions.x; ++i)
            {
                for (int j = 0; j < _dimensions.y; ++j)
                {
                    thisOffset = new Vector2(i * tileSize.x, j * tileSize.y);
                    // points2D[i, j] = startingPt + thisOffset + posnOffsets[i, j];
                    pointLocs.Add(startingPt + thisOffset + posnOffsets[i, j]);
                }
            }
        }
    }
    
    [Header("Terrain Type Settings")]
    [SerializeField] private TerrainSetting waterSetting;
    [SerializeField] private TerrainSetting shoreSetting;
    [SerializeField] private TerrainSetting groundSetting;
    [SerializeField] private TerrainSetting lowMountSetting;
    [SerializeField] private TerrainSetting highMountSetting;

    // Runtime vars
    private bool wasInit = false;
    private GenerationData genData;
    private List<LineSegment> _voronoiEdges = null;
    private List<LineSegment> _triangulation = null;
    private Delaunay.Voronoi _voronoi;

    private void OnDrawGizmos()
    {
        if (wasInit)
        {
            Gizmos.color = Color.magenta;

            float offsetX = genData._lenX;
            float offsetY = genData._lenY;

            Vector3 lowerLeft = new Vector3(genData._origin.x - offsetX/2f, 
                genData._origin.y, genData._origin.z - offsetY / 2f);

            Vector3 upperLeft = ProjectToXZPlane(new Vector2(0, offsetY), lowerLeft);
                // new Vector3(lowerLeft.x, lowerLeft.y, lowerLeft.z + offsetY);
            Vector3 upperRight = ProjectToXZPlane(new Vector2(offsetX, offsetY), lowerLeft);
                // new Vector3(lowerLeft.x + offsetX, lowerLeft.y, lowerLeft.z + offsetY);
            Vector3 lowerRight = ProjectToXZPlane(new Vector2(offsetX, 0), lowerLeft);
                // new Vector3(lowerLeft.x + offsetX, lowerLeft.y, lowerLeft.z);

            Gizmos.DrawLine(lowerLeft, upperLeft);
            Gizmos.DrawLine(upperLeft, upperRight);
            Gizmos.DrawLine(upperRight, lowerRight);
            Gizmos.DrawLine(lowerRight, lowerLeft);

            if (_voronoiEdges != null)
            {
                Gizmos.color = Color.white;
                for (int i = 0; i < _voronoiEdges.Count; i++)
                {
                    var left3D = ProjectToXZPlane((Vector2)_voronoiEdges[i].p0, lowerLeft);
                    var right3D = ProjectToXZPlane((Vector2)_voronoiEdges[i].p1, lowerLeft);
                    Gizmos.DrawLine(left3D, right3D);
                }
            }

            if (_triangulation != null)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < _triangulation.Count; i++)
                {
                    //Vector2 left = (Vector2)_triangulation[i].p0;
                    var left3D = ProjectToXZPlane((Vector2)_triangulation[i].p0, lowerLeft);
                    //Vector2 right = (Vector2)_triangulation[i].p1;
                    var right3D = ProjectToXZPlane((Vector2)_triangulation[i].p1, lowerLeft);
                    Gizmos.DrawLine((Vector3)left3D, (Vector3)right3D);
                }
            }

            Gizmos.color = Color.magenta;

            foreach (var posn in genData.pointLocs)
            {
                var posn3D = ProjectToXZPlane(posn, lowerLeft);
                Gizmos.DrawSphere(posn3D, 0.05f);
            }
        }
    }

    public void Init(GenerationData data)
    {
        // un-initialize
        wasInit = false;

        genData = data;

        // todo: add mesh generation here
        _voronoi = new Delaunay.Voronoi(genData.pointLocs, null, genData.rect);
        _voronoiEdges = _voronoi.VoronoiDiagram();
        _triangulation = _voronoi.DelaunayTriangulation();

        GenerateMesh();

        wasInit = true;
    }

    private void GenerateMesh()
    {

    }

    #region Misc Helpers
    private static Vector3 ProjectToXZPlane(Vector2 rectPosn, Vector3 lowerLeftPoint)
    {
        float x = lowerLeftPoint.x + rectPosn.x;
        float y = lowerLeftPoint.y;
        float z = lowerLeftPoint.z + rectPosn.y;
        return new Vector3(x, y, z);
    }
    #endregion
}
