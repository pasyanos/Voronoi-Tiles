using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VoronoiMeshUI : MonoBehaviour
{
    [SerializeField] private VoronoiMeshGeneration meshGenerator;

    [Space(10)]
    [SerializeField] private List<CanvasGroup> _toggleableCGList;

    [Header("Tile Map")]
    [SerializeField] private CanvasGroup _tileMapParent;
    [SerializeField] private RectTransform _tileMapRectTransform;
    [SerializeField] private GridLayoutGroup _tileMapGrid;
    private Transform _gridTransform;
    [SerializeField] private Image _tileUIPrefab;
    [SerializeField] private int _tileHeight = 5;

    [Header("Settings Dropdown")]
    [SerializeField] private TextMeshProUGUI _rowsSettingText;
    [SerializeField] private Slider _rowsSlider;
    [SerializeField] private TextMeshProUGUI _columnsSettingText;
    [SerializeField] private Slider _columnsSlider;
    [Space(10)]
    [SerializeField] private TextMeshProUGUI _tileSizeXText;
    [SerializeField] private Slider _sizeXSlider;
    [SerializeField] private TextMeshProUGUI _tileSizeYText;
    [SerializeField] private Slider _sizeYSlider;
    [Space(10)]
    [SerializeField] private Toggle _offsetColumnToggle;

    private void Start()
    {
        meshGenerator.mapUpdatedEvent.AddListener(UpdateUI);

        // do some setup for UI stuff
        _gridTransform = _tileMapGrid.transform;

        foreach (Transform child in _gridTransform)
            Destroy(child.gameObject);

        var curDimensions = meshGenerator.rowsByColumns;

        SetRowUIText(curDimensions.x);
        _rowsSlider.value = curDimensions.x;
        SetColumnTextUI(curDimensions.y);
        _columnsSlider.value = curDimensions.y;

        var curTileSize = meshGenerator.tileSize;
        SetSizeXUIText(curTileSize.x);
        _sizeXSlider.value = curTileSize.x;
        SetSizeYUIText(curTileSize.y);
        _sizeYSlider.value = curTileSize.y;

        bool offsetCols = meshGenerator.offsetEvenColumns;

        _offsetColumnToggle.isOn = offsetCols;

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

    public void OnRowSliderUpdated()
    {
        float val = _rowsSlider.value;
        // Debug.LogErrorFormat("Rows set to {0}", val);
        SetRowUIText((int)val);
        meshGenerator.SetRowDimensions((int)val);
    }

    public void OnColumnSliderUpdated()
    {
        float val = _columnsSlider.value;
        // Debug.LogErrorFormat("Cols set to {0}", val);
        SetColumnTextUI((int)val);
        meshGenerator.SetColumnDimensions((int)val);
    }

    public void OnTileSizeXSliderUpdated()
    {
        float val = _sizeXSlider.value;
        SetSizeXUIText(val);
        meshGenerator.SetTileSizeX(val);
    }

    public void OnTileSizeYSliderUpdated()
    {
        float val = _sizeYSlider.value;
        SetSizeYUIText(val);
        meshGenerator.SetTiileSizeY(val);
    }

    public void OnToggleOffsetColumns()
    {
        bool val = _offsetColumnToggle.isOn;
        // Debug.LogErrorFormat("offset: {0}", val);
        meshGenerator.SetOffsetColumns(val);
    }
    #endregion // Public Facing Methods 

    #region private methods and helpers
    private void UpdateUI()
    {
        // clear previous map
        foreach (Transform child in _gridTransform)
            Destroy(child.gameObject);

        Vector2Int mapDimensions = meshGenerator.rowsByColumns;
        var colors = meshGenerator.GetTerrainColors();

        _tileMapRectTransform.sizeDelta = new Vector2(mapDimensions.y, mapDimensions.x)*_tileHeight;
            // mapDimensions*_tileHeight;

        for (int i = 0; i < mapDimensions.x; i++)
        {
            for (int j = 0; j < mapDimensions.y; j++)
            {
                Image tile = Instantiate(_tileUIPrefab, _gridTransform);
                tile.color = colors[i,j];
            }
        }
    }

    private void SetRowUIText(int val)
    {
        _rowsSettingText.text = string.Format("Rows: {0}", val);
    }
    private void SetColumnTextUI(int val)
    {
        _columnsSettingText.text = string.Format("Cols: {0}", val);
    }

    private void SetSizeXUIText(float val)
    {
        _tileSizeXText.text = string.Format("X: {0:0.00}", val);
    }

    private void SetSizeYUIText(float val)
    {
        _tileSizeYText.text = string.Format("Y: {0:0.00}", val);
    }
    #endregion
}
