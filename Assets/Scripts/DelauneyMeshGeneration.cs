using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DelauneyMeshGeneration : MonoBehaviour
{
    public class GenerationData
    {
        public Vector3 _origin;
        public Vector2 _dimensions;
        public Vector2 _tileSize;

        // these are used by the rectangle class for generation
        public Vector2 _lowerLeftPt;
        public float _lenX;
        public float _lenY;

        public GenerationData(Vector3 origin, Vector2 dimensions, Vector2 tileSize)
        {
            _origin = origin;
            _dimensions = dimensions;
            _tileSize = tileSize;
            
            _lenX = dimensions.x * tileSize.x;
            _lenY = dimensions.y * tileSize.y;
            _lowerLeftPt = new Vector2(origin.x - _lenX/2f, origin.y - _lenY/2f);
        }
    }
    
    [Header("Terrain Type Settings")]
    [SerializeField] private TerrainSetting waterSetting;
    [SerializeField] private TerrainSetting shoreSetting;
    [SerializeField] private TerrainSetting groundSetting;
    [SerializeField] private TerrainSetting mountainSetting;

    // Runtime vars
    private bool wasInit = false;
    private GenerationData genData;

    private void OnDrawGizmos()
    {
        if (wasInit)
        {
            // Debug.LogError("yo");
            Gizmos.color = Color.magenta;

            var lowerLeft = Vector3.zero;
            var upperLeft = new Vector3(0, genData._lenY, 0);
            var upperRight = new Vector3(genData._lenX, genData._lenY, 0);
            var lowerRight = new Vector3(genData._lenX, 0, 0);

            Gizmos.DrawLine(upperLeft, upperRight);
            Gizmos.DrawLine(upperRight, lowerRight);
            Gizmos.DrawLine(lowerRight, lowerLeft);
            Gizmos.DrawLine(lowerLeft, upperLeft);
        }
    }

    public void Init(GenerationData data)
    {
        // un-initialize
        wasInit = false;

        genData = data;

        // todo: add mesh generation here
        GenerateMesh();

        wasInit = true;
    }

    private void GenerateMesh()
    {

    }
}
