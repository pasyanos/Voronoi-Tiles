using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorUI : MonoBehaviour
{
    [SerializeField] private MeshGeneration meshGenerator;

    #region Public Facing Methods
    public void OnClickGenerateTerrain()
    {
        meshGenerator.StartGeneration();
    }
    #endregion // Public Facing Methods 
}
