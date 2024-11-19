using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


public class TileGeneration : MonoBehaviour
{
    private static TileGeneration _instance = null;

    public static TileGeneration instance { get { return _instance; } }

    [SerializeField] private DelauneyMeshGeneration meshGeneratorInstance;

    [Header("Tile Prefabs")]
    [SerializeField] private GameObject groundTilePrefab;
    [SerializeField] private GameObject mountainTilePrefab;
    [SerializeField] private GameObject shoreTilePrefab;
    [SerializeField] private GameObject waterTilePrefab;

    [Header("Generation Settings")]
    [SerializeField] private Transform tileParent;
    [SerializeField] private Vector2Int rowsByColumns = new Vector2Int(3, 3);
    [SerializeField] private Vector2 tileSize = new Vector2(1f, 1f);
    [SerializeField] private AnimationCurve kernelCurve;
    [SerializeField] private bool offsetEvenColumns = true;
    [SerializeField] [Range(0f, 1f)] private float randomAmt = 0f;

    // Runtime Vars
    private TerrainType[,] _generatedTileTypes;
    private Vector3[,] _tileLocs;
    private Vector2[,] _terrainLocs2D;

    /*
     * I am referring to this loosely as a kernel, because I got the idea from filter kernels
     * used in computer vision. I'm not sure that's actually the correct technical term though.
    */
    private float[,] _kernel;

    private List<GameObject> _instantiatedTiles = new List<GameObject>();

    #region Unity Callbacks
    private void Awake()
    {
        if (instance == null)
        {
            _instance = this;
        }
        else 
        {
            Debug.LogError("Trying to create a second instance of a singleton class!");
        }

        ClearPrevGeneration();
    }
    #endregion // Unity Callbacks

    #region Private Helper Methods
    private void InstantiateTiles()
    {
        int rows = rowsByColumns.x;
        int columns = rowsByColumns.y;

        _tileLocs = new Vector3[rows, columns];
        _generatedTileTypes = new TerrainType[rows, columns];
        _terrainLocs2D = new Vector2[rows, columns];

        float minusXOffset = (float)(rows - 1)/2f*tileSize.x;
        float minusZOffset = (float)(columns - 1)/2f*tileSize.y;

        Vector3 columnOffset;

        // use System datetime to seed Perlin noise
        System.DateTime curTime = System.DateTime.Now;
        int xOff = curTime.Second;
        int yOff = curTime.Millisecond;

        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < columns; ++j)
            {
                columnOffset = offsetEvenColumns ? new Vector3(0, 0, (i % 2) * 0.5f * tileSize.x) 
                    : Vector3.zero; // else

                _tileLocs[i, j] = new Vector3(- minusXOffset + tileSize.x*i, 0, -minusZOffset + tileSize.y*j) + columnOffset;

                if (i > 0 && j > 0 && i < rows - 1 && j < columns - 1)
                {

                    // calculate location on 1x1 perlin texture, then sample it
                    float perlX = (float)((i + xOff) % rowsByColumns.x) / rowsByColumns.x;
                    float perlY = (float)((j + yOff) % rowsByColumns.y) / rowsByColumns.y;

                    float perlin = Mathf.PerlinNoise(perlX, perlY);
                    // modulate by kernel, then multiply by 100 to get a percentage
                    perlin *= 100f * _kernel[i, j];

                    // it's a mountain tile
                    if (perlin > (int)TerrainType.GROUND)
                    {
                        _generatedTileTypes[i, j] = TerrainType.MOUNTAIN;
                    }
                    // it's a ground tile
                    else if (perlin > (int)TerrainType.WATER)
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

        var posnOffsets2D = Generate2DOffsets(rowsByColumns, _generatedTileTypes);

        meshGeneratorInstance.Init(
            new DelauneyMeshGeneration.GenerationData(tileParent.transform.position, rowsByColumns, 
            tileSize, _generatedTileTypes, posnOffsets2D));

        // todo: comment this out once mesh generating is done
        InstantiateAssets();
    }

    private Vector2[,] Generate2DOffsets(Vector2Int size, TerrainType[,] types)
    {
        Vector2[,] ret = new Vector2[size.x, size.y];

        float xOff = UnityEngine.Random.Range(0, 1);
        float yOff = UnityEngine.Random.Range(0, 1);

        // init all to a zero vector
        for (int i = 0; i < size.x; ++i)
        {
            for(int j = 0; j < size.y; ++j)
            {
                var posn = Vector2.zero;

                var columnOffset = offsetEvenColumns ? new Vector2(0, (i % 2) * 0.5f) : Vector2.zero; // else

                // todo: fancy mods to posn here
                Vector2 rand = new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f));

                ret[i, j] = posn + randomAmt*rand + columnOffset;
            }
        }

        return ret;
    }

    private void InstantiateAssets()
    {
        int rows = rowsByColumns.x;
        int columns = rowsByColumns.y;
        
        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < columns; j++)
            {
                GameObject instantiated = null;

                TerrainType thisTileType = _generatedTileTypes[i, j];

                switch(thisTileType)
                {
                    case TerrainType.MOUNTAIN:
                        instantiated = Instantiate(mountainTilePrefab, tileParent);
                        break;
                    case TerrainType.GROUND:
                        instantiated = Instantiate(groundTilePrefab, tileParent);
                        break;
                    case TerrainType.SHORE:
                        instantiated = Instantiate(shoreTilePrefab, tileParent);
                        break;
                    // add additional cases here as they arise
                    default:
                        instantiated = Instantiate(waterTilePrefab, tileParent);
                        break;
                }

                // If we DID instantiate something, do a little extra positioning
                if (instantiated != null)
                {
                    instantiated.transform.localPosition = _tileLocs[i,j];
                    instantiated.name = string.Format("Tile {0}x{1}", i, j);
                    _instantiatedTiles.Add(instantiated);
                }
            }
        }
    }

    private void ClearPrevGeneration()
    {
        _instantiatedTiles.Clear();

        foreach (Transform child in tileParent)
        {
            Destroy(child.gameObject);
        }

        // re-generate kernel in case grid has changed size
        // there is probably a better place to move this later - for example only when the grid size changes,
        // but this is okay for now
        GenerateKernel();
    }

    private void GenerateKernel()
    {
        _kernel = new float[rowsByColumns.x, rowsByColumns.y];

        // could move these to variables if we want more control
        float minScale = 0.1f;
        float maxScale = 1.75f;

        float halfX = Mathf.Ceil(rowsByColumns.x / 2.0f);
        float halfY = Mathf.Ceil(rowsByColumns.y / 2.0f);

        for (int i = 0; i < rowsByColumns.x; ++i)
        {
            for (int j = 0; j < rowsByColumns.y; ++j)
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
        bool notBottomRow = (i < rowsByColumns.x - 1);
        bool notLeftEdgeCol = (j > 0);
        bool notRightEdgeCol = (j < rowsByColumns.y - 1);

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
    #endregion // Private Helper Methods

    #region Public Facing Methods
    public void StartGeneration()
    {
        ClearPrevGeneration();
        InstantiateTiles();
    }
    #endregion // Public Facing Methods 
}
