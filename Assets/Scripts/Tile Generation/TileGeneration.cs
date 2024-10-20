using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGeneration : MonoBehaviour
{
    private static TileGeneration _instance = null;

    public static TileGeneration instance { get { return _instance; } }
    

    [Header("General")]
    [SerializeField]
    private Transform tileParent;

    [Header("Wave Function Collapse Settings")]
    [SerializeField] 
    private Vector2Int rowsByColumns = new Vector2Int(3, 3);

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
    }
    #endregion // Unity Callbacks

    #region Private Helper Methods
    #endregion // Private Helper Methods

    #region Public Facing Methods
    public void StartGeneration()
    {
        Debug.LogError("Eventually I will do something");
    }
    #endregion // Public Facing Methods 
}
