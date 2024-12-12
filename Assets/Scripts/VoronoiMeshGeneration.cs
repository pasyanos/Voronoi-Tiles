using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;


public class VoronoiMeshGeneration : MonoBehaviour
{
    [Header("Mesh Generation")]
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private Vector3 meshOrigin = Vector3.zero;

    [Header("Generation Settings")]
    [SerializeField] private Vector2Int _rowsByColumns = new Vector2Int(3, 3);
    public Vector2Int rowsByColumns { get { return _rowsByColumns; } }
    [SerializeField] private Vector2 _tileSize = new Vector2(1f, 1f);
    public Vector2 tileSize { get { return _tileSize; } }
    [SerializeField] private AnimationCurve kernelCurve;
    [SerializeField] private bool _offsetEvenColumns = true;
    public bool offsetEvenColumns { get { return _offsetEvenColumns; } }
    [SerializeField][Range(0f, 0.5f)] private float _randomizePointFactor = 0.2f;
    public float randomizePointFactor { get { return _randomizePointFactor; } }
    [SerializeField][Range(0f, 0.5f)] private float _likeNeighborsRelaxationAmt = 0.3f;
    public float likeNeighborRelaxation { get { return _likeNeighborsRelaxationAmt; } }


    [Space(10)]
    [SerializeField] private Color wallColor = Color.gray;

    [Header("Terrain Type Settings")]
    [SerializeField] private TerrainSetting waterSetting;
    [SerializeField] private TerrainSetting shoreSetting;
    [SerializeField] private TerrainSetting groundSetting;
    [SerializeField] private TerrainSetting lowMountSetting;
    [SerializeField] private TerrainSetting highMountSetting;

    // Runtime Vars
    private TerrainType[,] _generatedTileTypes;
    // expose terrain types for UI
    // public TerrainType[,] generatedTileTypes {  get { return _generatedTileTypes; } }

    private Vector3[,] _tileLocs;
    private Vector2[,] _terrainLocs2D;

    [HideInInspector] public UnityEvent mapUpdatedEvent = new UnityEvent();

    /*
     * I am referring to this loosely as a kernel, because I got the idea from filter kernels
     * used in computer vision. I'm not sure that's actually the correct technical term though.
    */
    private float[,] _kernel;

    private List<Vector2> _points2D;
    private List<TerrainType> _terrainTypes;

    // the 3D point of the lower left corner of the voronoi diagram
    // this is used to convert 2D diagram points to 3D points in worldspace
    private Vector3 lowerLeftCorner;
    private Rect bounds;
    private int numPoints;
    private Delaunay.Voronoi _voronoi;

    #region Unity Callbacks
    private void Start()
    {
        // generate a new mesh on start
        StartGeneration();
    }

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
    #endregion // Unity Callbacks

    #region Public Facing Methods
    public void StartGeneration()
    {
        GenerateKernel();

        InstantiateTiles();

        mapUpdatedEvent.Invoke();
    }
    public Color[,] GetTerrainColors()
    {
        int rows = rowsByColumns.x;
        int cols = rowsByColumns.y;
        Color[,] ret = new Color[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                ret[i, j] = Setting(_generatedTileTypes[i,j]).GetColor();
            }
        }

        return ret;
    }
    #endregion // Public Facing Methods

    #region Mesh Generation Functions
    private void Init(Vector2Int gridDimensions, Vector2 tileSize, TerrainType[,] terrainGrid, Vector2[,] posnOffsets)
    {
        float lenX = (gridDimensions.x + 1) * tileSize.x;
        float lenY = (gridDimensions.y + 1) * tileSize.y;
        bounds = new Rect(0, 0, lenX, lenY);

        lowerLeftCorner = new Vector3(meshOrigin.x - lenX * 0.5f, meshOrigin.y, meshOrigin.z - lenY * 0.5f);

        // translate 2D arrays to lists for Voronoi generation
        _points2D = new List<Vector2>();
        _terrainTypes = new List<TerrainType>();

        Vector2 startingPt = new Vector2(tileSize.x * 0.5f, tileSize.y * 0.5f);
        Vector2 thisOffset;

        for (int i = 0; i < gridDimensions.x; i++)
        {
            for (int j = 0; j < gridDimensions.y; j++)
            {
                thisOffset = new Vector2(i * tileSize.x, j * tileSize.y);

                _points2D.Add(startingPt + thisOffset + posnOffsets[i, j]);
                _terrainTypes.Add(terrainGrid[i, j]);
            }
        }

        numPoints = _points2D.Count;
        _voronoi = new Delaunay.Voronoi(_points2D, null, bounds);

        GenerateMeshVoronoi();
    }

    private void GenerateMeshVoronoi()
    {
        TerrainMeshInformation terrain = new TerrainMeshInformation();
        float bottomWallY = meshOrigin.y - 0.1f;

        if (_voronoi != null)
        {
            for (int i = 0; i < numPoints; i++)
            {
                TerrainType curType = _terrainTypes[i];

                // don't instantiate for water
                if (curType > TerrainType.WATER)
                {
                    float yOffset = Setting(curType).GetYValue();
                    Color color = Setting(curType).GetColor();
                    Vector2 curLocation = _points2D[i];

                    List<Vector2> polygonForSite = SortClockwise(_voronoi.Region(curLocation), curLocation);

                    TopFaceMeshInformation newInfo = new TopFaceMeshInformation(polygonForSite, yOffset, color,
                        lowerLeftCorner);

                    terrain.AddTopFaceMeshInfo(newInfo);

                    Vector2 from = polygonForSite[polygonForSite.Count - 1];

                    for (int j = 0; j < polygonForSite.Count; j++)
                    {
                        Vector2 to = polygonForSite[j];

                        WallMeshInformation newWall = new WallMeshInformation(from, to, yOffset, bottomWallY, 
                            wallColor, wallColor, lowerLeftCorner);
                        terrain.AddWallMeshInfo(newWall);

                        from = to;
                    }
                }
            }
        }

        var generatedMesh = new Mesh() { name = "Procedural Mesh" };
        meshFilter.mesh = generatedMesh;
        generatedMesh.vertices = terrain.vertices;
        generatedMesh.triangles = terrain.triangles;
        generatedMesh.SetColors(terrain.colors);
    }
    #endregion // mesh generation functions

    #region UI Hooks
    public void SetRowDimensions(int newRows)
    {
        _rowsByColumns.x = newRows;
    }

    public void SetColumnDimensions(int newColumns)
    {
        _rowsByColumns.y = newColumns;
    }

    public void SetTileSizeX(float newSizeX)
    {
        _tileSize.x = newSizeX;
    }

    public void SetTileSizeY(float newSizeY)
    {
        _tileSize.y = newSizeY;
    }

    public void SetTileSize(float newSize)
    {
        SetTileSizeX(newSize);
        SetTileSizeY(newSize);
    }

    public void SetOffsetColumns(bool offset)
    {
        _offsetEvenColumns = offset;
    }

    public void SetPerlinAmount(float amount)
    {
        _randomizePointFactor = amount;
    }

    public void SetRelaxationAmt(float amount)
    {
        _likeNeighborsRelaxationAmt = amount;
    }
    #endregion

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

    private static List<Vector2> SortClockwise(List<Vector2> points, Vector2 center)
    {
        // todo:
        // return points;
        return points.OrderByDescending(p => System.Math.Atan2(p.y - center.y, p.x - center.x)).ToList();
    }

    private void InstantiateTiles()
    {
        int rows = _rowsByColumns.x;
        int columns = _rowsByColumns.y;

        _tileLocs = new Vector3[rows, columns];
        _generatedTileTypes = new TerrainType[rows, columns];
        _terrainLocs2D = new Vector2[rows, columns];

        float minusXOffset = (float)(rows - 1) / 2f * _tileSize.x;
        float minusZOffset = (float)(columns - 1) / 2f * _tileSize.y;

        Vector3 columnOffset;

        // use System datetime to seed Perlin noise
        System.DateTime curTime = System.DateTime.Now;
        int xOff = curTime.Second;
        int yOff = curTime.Millisecond;

        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < columns; ++j)
            {
                columnOffset = _offsetEvenColumns ? new Vector3(0, 0, (i % 2) * 0.5f * _tileSize.x)
                    : Vector3.zero; // else

                _tileLocs[i, j] = new Vector3(-minusXOffset + _tileSize.x * i, 0, -minusZOffset + _tileSize.y * j) + columnOffset;

                if (i > 0 && j > 0 && i < rows - 1 && j < columns - 1)
                {

                    // calculate location on 1x1 perlin texture, then sample it
                    float perlX = (float)((i + xOff) % _rowsByColumns.x) / _rowsByColumns.x;
                    float perlY = (float)((j + yOff) % _rowsByColumns.y) / _rowsByColumns.y;

                    float perlin = Mathf.PerlinNoise(perlX, perlY);
                    // modulate by kernel, then multiply by 100 to get a percentage
                    perlin *= 100f * _kernel[i, j];

                    // it's a high mountain tile
                    if (perlin > (int)TerrainType.MOUNTAIN)
                    {
                        _generatedTileTypes[i, j] = TerrainType.MOUNTAIN;
                    }
                    // it's a low mountain tile
                    else if (perlin > (int)TerrainType.LOWMOUNTAIN)
                    {
                        _generatedTileTypes[i, j] = TerrainType.LOWMOUNTAIN;
                    }
                    // it's a ground tile
                    else if (perlin > (int)TerrainType.GROUND)
                    {
                        _generatedTileTypes[i, j] = TerrainType.GROUND;
                    }
                    // else water
                    else
                    {
                        _generatedTileTypes[i, j] = TerrainType.WATER;
                    }
                }
                // force all edge tiles to watter
                else
                {
                    _generatedTileTypes[i, j] = TerrainType.WATER;
                }
            }
        }

        // If the width x height is more than 8, make any ground tile that is next to water a shore tile instead
        if (rows > 8 && columns > 8)
        {
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < columns; ++j)
                {
                    if (_generatedTileTypes[i, j] == TerrainType.GROUND)
                    {
                        var neighbors = GetNeighborTypes(i, j);

                        if (neighbors.Contains(TerrainType.WATER))
                            _generatedTileTypes[i, j] = TerrainType.SHORE;

                    }
                }
            }
        }

        var posnOffsets2D = Generate2DOffsets(_rowsByColumns, _generatedTileTypes);

        Init(_rowsByColumns, _tileSize, _generatedTileTypes, posnOffsets2D);
    }

    private Vector2[,] Generate2DOffsets(Vector2Int size, TerrainType[,] types)
    {
        Vector2[,] ret = new Vector2[size.x, size.y];

        float xOff = UnityEngine.Random.Range(0, 1);
        float yOff = UnityEngine.Random.Range(0, 1);

        TerrainType currentType;
        Vector2 posn;

        // init all to a zero vector
        for (int i = 0; i < size.x; ++i)
        {
            for (int j = 0; j < size.y; ++j)
            {
                currentType = types[i, j];
                posn = Vector2.zero;

                var columnOffset = _offsetEvenColumns ? new Vector2(0, (i % 2) * 0.5f) : Vector2.zero; // else

                // todo: fancy mods to posn here
                Vector2 rand = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));

                Vector2 likeNeigborOffset = Vector2.zero;
                TerrainType up = GetNorthType(i, j);
                if (currentType == up)
                    likeNeigborOffset += Vector2.up * 0.5f;

                TerrainType down = GetSouthType(i, j);
                if (currentType == down)
                    likeNeigborOffset += Vector2.down * 0.5f;

                TerrainType left = GetWestType(i, j);
                if (currentType == left)
                    likeNeigborOffset += Vector2.left * 0.5f;

                TerrainType right = GetEastType(i, j);
                if (currentType == right)
                    likeNeigborOffset += Vector2.right * 0.5f;

                ret[i, j] = posn + _randomizePointFactor * rand 
                    + _likeNeighborsRelaxationAmt*likeNeigborOffset 
                    + columnOffset;
            }
        }

        return ret;
    }

    private void GenerateKernel()
    {
        _kernel = new float[_rowsByColumns.x, _rowsByColumns.y];

        // could move these to variables if we want more control
        float minScale = 0.1f;
        float maxScale = 1.1f;

        float halfX = Mathf.Ceil(_rowsByColumns.x / 2.0f);
        float halfY = Mathf.Ceil(_rowsByColumns.y / 2.0f);

        for (int i = 0; i < _rowsByColumns.x; ++i)
        {
            for (int j = 0; j < _rowsByColumns.y; ++j)
            {
                float locX = Mathf.Clamp01(1.0f - Mathf.Abs(halfX - (i + 1)) / halfX);
                float locY = Mathf.Clamp01(1.0f - Mathf.Abs(halfY - (j + 1)) / halfY);

                _kernel[i, j] = Mathf.Lerp(minScale, maxScale, kernelCurve.Evaluate(locX))
                * Mathf.Lerp(minScale, maxScale, kernelCurve.Evaluate(locY));
            }
        }
        // Debug.Log("Re-generating filter kernel");
    }

    // I only care about whether there's an instance of a terrain type, not how many
    // so I'm using a hashset and not allowing duplicates.
    private HashSet<TerrainType> GetNeighborTypes(int i, int j)
    {
        HashSet<TerrainType> ret = new HashSet<TerrainType>();
        bool notTopRow = (i > 0);
        bool notBottomRow = (i < _rowsByColumns.x - 1);
        bool notLeftEdgeCol = (j > 0);
        bool notRightEdgeCol = (j < _rowsByColumns.y - 1);

        if (notTopRow)
        {
            // upper neighbor
            ret.Add(_generatedTileTypes[i - 1, j]);

            // upper left neighbor
            if (notLeftEdgeCol)
                ret.Add(_generatedTileTypes[i - 1, j - 1]);

            // upper right neighbor
            if (notRightEdgeCol)
                ret.Add(_generatedTileTypes[i - 1, j + 1]);
        }

        // middle left neigbor
        if (notLeftEdgeCol)
            ret.Add(_generatedTileTypes[i, j - 1]);

        // middle right neighbor
        if (notRightEdgeCol)
            ret.Add(_generatedTileTypes[i, j + 1]);

        if (notBottomRow)
        {
            // lower neighbor
            ret.Add(_generatedTileTypes[i + 1, j]);

            // lower left neighbor
            if (notLeftEdgeCol)
                ret.Add(_generatedTileTypes[i + 1, j - 1]);

            // lower right neighbor
            if (notRightEdgeCol)
                ret.Add(_generatedTileTypes[i + 1, j + 1]);
        }

        return ret;
    }

    private TerrainType GetWestType(int i, int j)
    {
        if (i > 0)
            return _generatedTileTypes[i - 1, j];

        // if we are on the edge, just assume that the neighbor would be the same terrain type
        return _generatedTileTypes[i, j];
    }
    
    private TerrainType GetEastType(int i, int j)
    {
        if (i < _rowsByColumns.x - 1)
            return _generatedTileTypes[i + 1, j];

        // if we are on the edge, just assume that the neighbor would be the same terrain type
        return _generatedTileTypes[i, j];
    }

    private TerrainType GetNorthType(int i, int j)
    {
        if (j > 0)
            return _generatedTileTypes[i, j - 1];

        // if we are on the edge, just assume that the neighbor would be the same terrain type
        return _generatedTileTypes[i, j];
    }

    private TerrainType GetSouthType(int i, int j)
    {
        if (j < _rowsByColumns.y - 1)
            return _generatedTileTypes[i, j + 1];
        
        // if we are on the edge, just assume that the neighbor would be the same terrain type
        return _generatedTileTypes[i, j];
    }
    #endregion
}
