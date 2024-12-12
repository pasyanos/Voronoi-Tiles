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
    //[SerializeField] private TextMeshProUGUI _tileSizeXText;
    //[SerializeField] private Slider _sizeXSlider;
    //[SerializeField] private TextMeshProUGUI _tileSizeYText;
    //[SerializeField] private Slider _sizeYSlider;
    [SerializeField] private TextMeshProUGUI _tileSizeText;
    [SerializeField] private Slider _tileSizeSlider;
    [Space(10)]
    [SerializeField] private Toggle _offsetColumnToggle;
    [Space(10)]
    [SerializeField] private TextMeshProUGUI _perlinAmountText;
    [SerializeField] private Slider _perlinAmountSlider;
    [SerializeField] private TextMeshProUGUI _relaxationAmountText;
    [SerializeField] private Slider _relaxationAmountSlider;

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

        var curTileSize = meshGenerator.tileSize.x;
        meshGenerator.SetTileSize(curTileSize);
        //SetSizeXUIText(curTileSize.x);
        //_sizeXSlider.value = curTileSize.x;
        //SetSizeYUIText(curTileSize.y);
        //_sizeYSlider.value = curTileSize.y;
        _tileSizeSlider.value = curTileSize;
        SetTileSizeText(curTileSize);

        bool offsetCols = meshGenerator.offsetEvenColumns;

        _offsetColumnToggle.isOn = offsetCols;

        float perlinFactor = meshGenerator.randomizePointFactor;
        _perlinAmountSlider.value = perlinFactor;
        SetPerlinAmtText(perlinFactor);

        float neighborRelaxation = meshGenerator.likeNeighborRelaxation;
        _relaxationAmountSlider.value = neighborRelaxation;
        SetRelaxationAmtText(neighborRelaxation);

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
        SetRowUIText((int)val);
        meshGenerator.SetRowDimensions((int)val);
    }

    public void OnColumnSliderUpdated()
    {
        float val = _columnsSlider.value;
        SetColumnTextUI((int)val);
        meshGenerator.SetColumnDimensions((int)val);
    }

    //public void OnTileSizeXSliderUpdated()
    //{
    //    float val = _sizeXSlider.value;
    //    SetSizeXUIText(val);
    //    meshGenerator.SetTileSizeX(val);
    //}

    //public void OnTileSizeYSliderUpdated()
    //{
    //    float val = _sizeYSlider.value;
    //    SetSizeYUIText(val);
    //    meshGenerator.SetTiileSizeY(val);
    //}

    public void OnUpdateBothXYSize()
    {
        float val = _tileSizeSlider.value;
        SetTileSizeText(val);
        meshGenerator.SetTileSize(val);
    }

    public void OnToggleOffsetColumns()
    {
        bool val = _offsetColumnToggle.isOn;
        meshGenerator.SetOffsetColumns(val);
    }

    public void SetPerlinNoise()
    {
        float val = _perlinAmountSlider.value;
        meshGenerator.SetPerlinAmount(val);
        SetPerlinAmtText(val);
    }

    public void SetRelaxationAmt()
    {
        float val = _relaxationAmountSlider.value;
        meshGenerator.SetRelaxationAmt(val);
        SetRelaxationAmtText(val);
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

    //private void SetSizeXUIText(float val)
    //{
    //    _tileSizeXText.text = string.Format("X: {0:0.00}", val);
    //}

    //private void SetSizeYUIText(float val)
    //{
    //    _tileSizeYText.text = string.Format("Y: {0:0.00}", val);
    //}

    private void SetTileSizeText(float val)
    {
        _tileSizeText.text = string.Format("{0:0.00} x {0:0.00}", val);
    }

    private void SetPerlinAmtText(float val)
    {
        int percent = Mathf.RoundToInt(100 * val);
        _perlinAmountText.text = string.Format("Perlin Noise: {0}%", percent);
    }

    private void SetRelaxationAmtText(float val)
    {
        int percent = Mathf.RoundToInt(100 * val);
        _relaxationAmountText.text = string.Format("Point Relax: {0}%", percent);
    }
    #endregion
}
