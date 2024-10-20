using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public enum TileType
{
    WATER,
    GROUND
}

public class TileGeneration : MonoBehaviour
{
    private static TileGeneration _instance = null;

    public static TileGeneration instance { get { return _instance; } }

    [Header("Tiles")]
    [SerializeField] private GameObject groundTilePrefab;

    [Header("Generation Settings")]
    [SerializeField] private Transform tileParent;
    [SerializeField] private Vector2Int rowsByColumns = new Vector2Int(3, 3);
    [SerializeField] private Vector2 tileSize = new Vector2(1f, 1f);

    // Runtime Vars
    private TileType[,] _generatedTileTypes;
    private Vector3[,] _tileLocs;

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
        _tileLocs = new Vector3[rowsByColumns.x, rowsByColumns.y];
        int rows = rowsByColumns.x;
        int columns = rowsByColumns.y;

        float minusXOffset = (float)(rows - 1)/2f*tileSize.x;
        float minusZOffset = (float)(columns - 1)/2f*tileSize.y;

        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < columns; ++j)
            {
                _tileLocs[i, j] = new Vector3(- minusXOffset + tileSize.x*i, 0, -minusZOffset + tileSize.y*j);

                // spawn tile at posnAt, then move into a new position
                // this will be replaced later, when generating mesh from scratch
                GameObject instantiated = Instantiate(groundTilePrefab, tileParent);
                instantiated.transform.localPosition = _tileLocs[i,j];
                instantiated.name = string.Format("Tile {0}x{1}", i, j);
                _instantiatedTiles.Add(instantiated);
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
    }
    #endregion // Private Helper Methods

    #region Public Facing Methods
    public void StartGeneration()
    {
        // Debug.LogError("Eventually I will do something");
        ClearPrevGeneration();

        InstantiateTiles();
    }
    #endregion // Public Facing Methods 
}
