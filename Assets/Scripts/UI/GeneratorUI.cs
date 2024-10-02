using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorUI : MonoBehaviour
{
    private WaveFunctionCollapse wfcInstance;
    
    #region Unity Callbacks
    private void Start()
    {
        wfcInstance = WaveFunctionCollapse.instance;
    }
    #endregion // Unity Callbacks

    #region Public Facing Methods
    public void OnClickGenerateTerrain()
    {
        wfcInstance.StartGeneration();
    }
    #endregion // Public Facing Methods 
}
