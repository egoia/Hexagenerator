using System.Collections.Generic;
using UnityEngine;
using System;

public enum Terrain
{
    WATER, GRASS, COAST_WL, COAST_WR, RIVER, ROAD, ANYTHING, FOREST
}

[CreateAssetMenu(menuName = "Procedural Generation/HexagoneTile")]
public class HexagoneTile : ScriptableObject
{
    public GameObject prefab;
    public Terrain[] sequence = new Terrain[6];
    public float occurenceValue;
    [Range(0, 5)] public int rotation = 0;
    public List<HexagoneTile> northWest;
    public List<HexagoneTile> northEast;
    public List<HexagoneTile> east;
    public List<HexagoneTile> southEast;
    public List<HexagoneTile> southWest;
    public List<HexagoneTile> west;
    public List<List<HexagoneTile>> _adjacencyPossibilities = null;

    public List<List<HexagoneTile>> AdjacencyPossibilities()
    {
        if (_adjacencyPossibilities == null)
        {
            _adjacencyPossibilities = new List<List<HexagoneTile>> { northWest, northEast, west, east, southWest, southEast};
        }
        return _adjacencyPossibilities;
    }

    public static Terrain getCompatible(Terrain t)
    {
        switch (t)
        {
            case Terrain.COAST_WL: return Terrain.COAST_WR;
            case Terrain.COAST_WR: return Terrain.COAST_WL ;
            default: return  t ;
        }
    }

    void OnValidate()
    {
        if (sequence.Length != 6) Array.Resize(ref sequence, 6);
    }

    public GameObject Spawn(Vector3 pos, Transform transform)
    {
        prefab.GetComponent<HexaContainer>().scriptable = this;
        return Instantiate(prefab, pos, Quaternion.Euler(0, rotation * 60, 0), transform);
    }
    
}