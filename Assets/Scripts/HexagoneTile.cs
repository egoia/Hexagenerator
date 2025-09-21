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
    public float occurenceValue;
    [Range(0, 5)] public int rotation = 0;
    public List<HexagoneTile> northWest;
    public List<HexagoneTile> northEast;
    public List<HexagoneTile> east;
    public List<HexagoneTile> southEast;
    public List<HexagoneTile> southWest;
    public List<HexagoneTile> west;

    

    void OnValidate()
    {
        if (sequence.Length != 6) Array.Resize(ref sequence, 6);
    }

    public GameObject Spawn(Vector3 pos, Transform transform)
    {
        return Instantiate(prefab, pos, Quaternion.Euler(0, 0, rotation * 60), transform);
    }
    
}