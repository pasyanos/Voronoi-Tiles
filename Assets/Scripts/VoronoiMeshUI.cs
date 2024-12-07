using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VoronoiMeshUI : MonoBehaviour
{
    [SerializeField] private VoronoiMeshGeneration meshGenerator;

    [Header("Tile Map")]
    [SerializeField] private CanvasGroup _tileMapParent;
    [SerializeField] private RectTransform _tileMapRectTransform;
    [SerializeField] private GridLayoutGroup _tileMapGrid;
    private Transform _gridTransform;
    [SerializeField] private Image _tileUIPrefab;
    [SerializeField] private int _tileHeight = 5;

    private void Start()
    {
        meshGenerator.mapUpdatedEvent.AddListener(UpdateUI);

        // do some setup for UI stuff
        _gridTransform = _tileMapGrid.transform;

        foreach (Transform child in _gridTransform)
            Destroy(child.gameObject);

        _tileMapGrid.cellSize = new Vector2(_tileHeight, _tileHeight);
    }

    private void OnDestroy()
    {
        meshGenerator.mapUpdatedEvent.RemoveListener(UpdateUI);
    }

    #region Public Facing Methods
    public void OnClickGenerateTerrain()
    {
        meshGenerator.StartGeneration();
    }
    #endregion // Public Facing Methods 

    #region private methods and helpers
    private void UpdateUI()
    {
        // clear previous
        foreach (Transform child in _gridTransform)
            Destroy(child.gameObject);

        Vector2Int mapDimensions = meshGenerator.rowsByColumns;
        var colors = meshGenerator.GetTerrainColors();

        _tileMapRectTransform.sizeDelta = mapDimensions*_tileHeight;

        for (int i = 0; i < mapDimensions.x; i++)
        {
            for (int j = 0; j < mapDimensions.y; j++)
            {
                Image tile = Instantiate(_tileUIPrefab, _gridTransform);
                tile.color = colors[i,j];
            }
        }
    }
    #endregion
}
