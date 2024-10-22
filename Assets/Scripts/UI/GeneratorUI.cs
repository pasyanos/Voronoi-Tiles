using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorUI : MonoBehaviour
{
    private TileGeneration generatorInstance;
    
    #region Unity Callbacks
    private void Start()
    {
        generatorInstance = TileGeneration.instance;
    }
    #endregion // Unity Callbacks

    #region Public Facing Methods
    public void OnClickGenerateTerrain()
    {
        generatorInstance.StartGeneration();
    }
    #endregion // Public Facing Methods 
}
