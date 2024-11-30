using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiMeshUI : MonoBehaviour
{
    [SerializeField] private VoronoiMeshGeneration meshGenerator;

    #region Public Facing Methods
    public void OnClickGenerateTerrain()
    {
        meshGenerator.StartGeneration();
    }
    #endregion // Public Facing Methods 
}
