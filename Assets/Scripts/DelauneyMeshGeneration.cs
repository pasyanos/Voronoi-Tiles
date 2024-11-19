using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class DelauneyMeshGeneration : MonoBehaviour
{
    public class GenerationData
    {
        public Vector3 _origin;
        public Vector2Int _dimensions;
        public Vector2 _tileSize;

        // these are used by the rectangle class for generation
        public float _lenX;
        public float _lenY;
        public Vector2[,] points2D;

        public GenerationData(Vector3 origin, Vector2Int dimensions, Vector2 tileSize, 
            TerrainType[,] terrainTypes, Vector2[,] posnOffsets)
        {
            _origin = origin;
            _dimensions = dimensions;
            _tileSize = tileSize;
            
            _lenX = dimensions.x * tileSize.x;
            _lenY = dimensions.y * tileSize.y;

            points2D = new Vector2[dimensions.x, dimensions.y];

            Vector2 startingPt = new Vector2(_tileSize.x/2f, _tileSize.y/2f);
            Vector2 thisOffset;

            // translate offsets to 2D positions
            for (int i = 0; i < _dimensions.x; ++i)
            {
                for (int j = 0; j < _dimensions.y; ++j)
                {
                    thisOffset = new Vector2(i * tileSize.x, j * tileSize.y);
                    points2D[i, j] = startingPt + thisOffset + posnOffsets[i, j];
                }
            }
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

            float offsetX = genData._lenX;
            float offsetY = genData._lenY;

            Vector3 lowerLeft = new Vector3(genData._origin.x - offsetX/2f, 
                genData._origin.y, genData._origin.z - offsetY / 2f);

            Vector3 upperLeft = new Vector3(lowerLeft.x, lowerLeft.y, lowerLeft.z + offsetY);
            Vector3 upperRight = new Vector3(lowerLeft.x + offsetX, lowerLeft.y, lowerLeft.z + offsetY);
            Vector3 lowerRight = new Vector3(lowerLeft.x + offsetX, lowerLeft.y, lowerLeft.z);

            Gizmos.DrawLine(lowerLeft, upperLeft);
            Gizmos.DrawLine(upperLeft, upperRight);
            Gizmos.DrawLine(upperRight, lowerRight);
            Gizmos.DrawLine(lowerRight, lowerLeft);

            Gizmos.color = Color.green;

            for (int i = 0; i < genData._dimensions.x; i++)
            {
                for (int j = 0; j < genData._dimensions.y; j++)
                {
                    var pt = lowerLeft + new Vector3(genData.points2D[i, j].x, 0, genData.points2D[i, j].y);
                    Gizmos.DrawCube(pt, new Vector3(0.1f, 0.1f, 0.1f));
                }
            }
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
