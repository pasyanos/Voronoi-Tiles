using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Delaunay.Geo;
using Unity.Properties;


public class MeshGeneration : MonoBehaviour
{
    public class GenerationData
    {
        public Vector3 _origin;
        public Vector2Int _dimensions;
        public Vector2 _tileSize;
        public int length;

        // these are used by the rectangle class for generation
        public float _lenX;
        public float _lenY;
   
        public List<Vector2> pointLocs;
        public List<TerrainType> terrain1D;
        public Rect rect;

        public GenerationData(Vector3 origin, Vector2Int dimensions, Vector2 tileSize, 
            TerrainType[,] terrainTypes, Vector2[,] posnOffsets)
        {
            _origin = origin;
            _dimensions = dimensions;
            _tileSize = tileSize;
            
            // adding on to the dimensions gives just a bit of wiggle room
            _lenX = (dimensions.x + 1) * tileSize.x;
            _lenY = (dimensions.y + 1) * tileSize.y;

            // points2D = new Vector2[dimensions.x, dimensions.y];
            pointLocs = new List<Vector2>();
            terrain1D = new List<TerrainType>();

            rect = new Rect(0, 0, _lenX, _lenY);

            Vector2 startingPt = new Vector2(_tileSize.x*0.75f, _tileSize.y*0.75f);
            Vector2 thisOffset;

            // translate offsets to 2D positions
            for (int i = 0; i < _dimensions.x; ++i)
            {
                for (int j = 0; j < _dimensions.y; ++j)
                {
                    thisOffset = new Vector2(i * tileSize.x, j * tileSize.y);
                    // points2D[i, j] = startingPt + thisOffset + posnOffsets[i, j];
                    pointLocs.Add(startingPt + thisOffset + posnOffsets[i, j]);
                    terrain1D.Add(terrainTypes[i, j]);
                }
            }

            length = pointLocs.Count;
        }
    }

    [Header("Debug Stuff")]
    [SerializeField] private bool showTriangulation = true;
    [SerializeField] private bool delauneyMesh = false;

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
            Vector3 upperRight = ProjectToXZPlane(new Vector2(offsetX, offsetY), lowerLeft);
            Vector3 lowerRight = ProjectToXZPlane(new Vector2(offsetX, 0), lowerLeft);

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

            if (_triangulation != null && showTriangulation)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < _triangulation.Count; i++)
                {
                    var left3D = ProjectToXZPlane((Vector2)_triangulation[i].p0, lowerLeft);
                    var right3D = ProjectToXZPlane((Vector2)_triangulation[i].p1, lowerLeft);
                    Gizmos.DrawLine(left3D, right3D);
                }
            }

            //Gizmos.color = Color.magenta;

            //foreach (var posn in genData.pointLocs)
            //{
            //    var posn3D = ProjectToXZPlane(posn, lowerLeft);
            //    Gizmos.DrawSphere(posn3D, 0.05f);
            //}
        }
    }

    private void OnDrawGizmosSelected()
    {

    }

    public void Init(GenerationData data)
    {
        // un-initialize
        wasInit = false;

        genData = data;

        _voronoi = new Delaunay.Voronoi(genData.pointLocs, null, genData.rect);
        _voronoiEdges = _voronoi.VoronoiDiagram();
        // not currently needed
        // _triangulation = _voronoi.DelaunayTriangulation();

        GenerateMeshVoronoi();

        wasInit = true;
    }

    private void GenerateMeshVoronoi()
    {
        if (_voronoi != null)
        {
            for (int i = 0; i < genData.length; i++)
            {
                TerrainType curType = genData.terrain1D[i];
                Vector2 curLocation = genData.pointLocs[i];

                var voronoiSiteInfo = _voronoi.VoronoiBoundaryForSite(curLocation);
                Debug.LogErrorFormat("Site at ({0}, {1}) has {2} line segments", 
                    curLocation.x, curLocation.y, voronoiSiteInfo.Count);

                // todo: figure out if we need additional line segments if the area is bound by the bounding box
                // as opposed to other line segments
            }
        }
    }

    private void GenerateMeshDelauney()
    {
        // todo:
    }

    #region Misc Helpers
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
