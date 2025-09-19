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
    public static List<Terrain> TERRAINS = new List<Terrain> { Terrain.WATER, Terrain.GRASS, Terrain.COAST_WL, Terrain.COAST_WR, Terrain.RIVER, Terrain.ROAD };
    public GameObject prefab;
    public Terrain[] sequence = new Terrain[6];

    void OnValidate(){
        if(sequence.Length != 6) Array.Resize(ref sequence, 6);
    }
}