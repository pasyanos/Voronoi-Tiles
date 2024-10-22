using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public enum TileType
{
    WATER = 20,
    SHORE,
    GROUND = 72,
    MOUNTAIN
}

public class TileGeneration : MonoBehaviour
{
    private static TileGeneration _instance = null;

    public static TileGeneration instance { get { return _instance; } }

    [Header("Tiles")]
    [SerializeField] private GameObject groundTilePrefab;
    [SerializeField] private GameObject mountainTilePrefab;

    [Header("Generation Settings")]
    [SerializeField] private Transform tileParent;
    [SerializeField] private Vector2Int rowsByColumns = new Vector2Int(3, 3);
    [SerializeField] private Vector2 tileSize = new Vector2(1f, 1f);
    [SerializeField] private AnimationCurve kernelCurve;

    // Runtime Vars
    private TileType[,] _generatedTileTypes;
    private Vector3[,] _tileLocs;
    /*
     * I am referring to this losely as a kernel, because I got the idea from filter kernels
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
        _generatedTileTypes = new TileType[rows, columns];

        float minusXOffset = (float)(rows - 1)/2f*tileSize.x;
        float minusZOffset = (float)(columns - 1)/2f*tileSize.y;

        // use System datetime to seed Perlin noise
        System.DateTime curTime = System.DateTime.Now;
        int xOff = curTime.Second;
        int yOff = curTime.Millisecond;

        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < columns; ++j)
            {
                _tileLocs[i, j] = new Vector3(- minusXOffset + tileSize.x*i, 0, -minusZOffset + tileSize.y*j);

                // calculate location on 1x1 perlin texture, then sample it
                float perlX = (float)((i + xOff) % rowsByColumns.x) / rowsByColumns.x;
                float perlY = (float)((j + yOff) % rowsByColumns.y) / rowsByColumns.y;

                float perlin = Mathf.PerlinNoise(perlX, perlY);
                // modulate by kernel, then multiply by 100 to get a percentage
                perlin *= 100f * _kernel[i, j];

                // Debug.LogError("Perlin kernel is " + perlin);

                // spawn tile at posnAt, then move into a new position
                // this will be replaced later, when generating mesh from scratch
                // GameObject instantiated = null;
                
                // it's a mountain tile
                if (perlin > (int)TileType.GROUND)
                {
                    // instantiated = Instantiate(mountainTilePrefab, tileParent);
                    // instantiated.transform.localPosition = _tileLocs[i,j];
                    // instantiated.name = string.Format("Mountain Tile {0}x{1}", i, j);
                    // _instantiatedTiles.Add(instantiated);
                    _generatedTileTypes[i, j] = TileType.MOUNTAIN;
                }
                // it's a ground tile
                else if (perlin > (int)TileType.WATER)
                {
                    // instantiated = Instantiate(groundTilePrefab, tileParent);
                    // instantiated.transform.localPosition = _tileLocs[i,j];
                    // instantiated.name = string.Format("Ground Tile {0}x{1}", i, j);
                    // _instantiatedTiles.Add(instantiated);
                    _generatedTileTypes[i, j] = TileType.GROUND;
                }
                // else water
                else
                {
                    _generatedTileTypes[i, j] = TileType.WATER;
                }
            }
        }

        InstantiateAssets();
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

                TileType thisTileType = _generatedTileTypes[i, j];

                switch(thisTileType)
                {
                    case TileType.MOUNTAIN:
                        instantiated = Instantiate(mountainTilePrefab, tileParent);
                        break;
                    case TileType.GROUND:
                        instantiated = Instantiate(groundTilePrefab, tileParent);
                        break;
                    // add additional cases here as they arise
                    // if it's water, don't need to instantiate anything
                    default:
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
        float minScale = 0.01f;
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
    #endregion // Private Helper Methods

    #region Public Facing Methods
    public void StartGeneration()
    {
        ClearPrevGeneration();
        InstantiateTiles();
    }
    #endregion // Public Facing Methods 
}
