using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainSetting
{
    [SerializeField] private TerrainType type;
    [SerializeField] private float yValue;
    [SerializeField] private Color color;
}