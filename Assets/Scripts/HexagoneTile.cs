using System.Collections.Generic;
using UnityEngine;
using System;

public enum Terrain
{
    WATER, GRASS, COAST_WL, COAST_WR, RIVER, ROAD, ANYTHING
}

[CreateAssetMenu(menuName = "Procedural Generation/HexagoneTile")]
public class HexagoneTile : ScriptableObject
{
    
    public GameObject prefab;
    public Terrain[] sequence = new Terrain[6];

    void OnValidate(){
        if(sequence.Length != 6) Array.Resize(ref sequence, 6);
    }
}