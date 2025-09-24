using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SetupTiles : MonoBehaviour
{
    public List<HexagoneTile> allHexagoneTiles;
    public string dirPath;
    public List<HexagoneTile> grass;
    public List<HexagoneTile> water;
    public List<HexagoneTile> normalRoads;
    public List<HexagoneTile> strangeRoads;
    public List<HexagoneTile> normalRivers;
    public List<HexagoneTile> strangeRivers;
    public List<HexagoneTile> coast;
    List<Terrain> TERRAINS = new List<Terrain>((Terrain[])Enum.GetValues(typeof(Terrain)));

    [ContextMenu("Setup Rotated tiles")]
    public void SetupRotated()
    {
        foreach (var tile in allHexagoneTiles)
        {
            List<HexagoneTile> rotated = MakeRotatedPossibilities(tile);
            foreach (var rot in rotated)
            {
                AssetDatabase.CreateAsset(rot, $"{dirPath}/{tile.name}_{rot.rotation}.asset");
                AssetDatabase.SaveAssets();
            }
        }
    }
    [ContextMenu("Setup Adjacency lists")]
    public void SetUpAdjacencyLists()
    {
        MakeTerrainPossibilityLists(allHexagoneTiles);
    }

    public void SetupOccurenceValue(float grass, float water, float road, float coast, float river, float strange)
    {
        foreach (var tile in this.grass)
        {
            tile.occurenceValue = grass / this.grass.Count;
        }

        foreach (var tile in this.water)
        {
            tile.occurenceValue = water / this.water.Count;
        }

        foreach (var tile in this.coast)
        {
            tile.occurenceValue = coast / this.coast.Count;
        }

        foreach (var tile in this.normalRivers)
        {
            tile.occurenceValue = river * (100 - strange) / 100 / this.normalRivers.Count;
        }

        foreach (var tile in this.normalRoads)
        {
            tile.occurenceValue = road * (100 - strange) / 100 / this.normalRoads.Count;
        }

        foreach (var tile in this.strangeRivers)
        {
            tile.occurenceValue = river * strange/ 100 / this.strangeRivers.Count;
        }
        
        foreach (var tile in this.strangeRoads)
        {
            tile.occurenceValue = road*strange/100 / this.strangeRoads.Count;
        }
    }

    List<HexagoneTile> MakeRotatedPossibilities(HexagoneTile tile)
    {
        List<HexagoneTile> rotatedPossibilities = new List<HexagoneTile>();
        for (int i = 1; i < 6; i++)
        {
            HexagoneTile rot = ScriptableObject.CreateInstance<HexagoneTile>();
            rot.prefab = tile.prefab;
            rot.occurenceValue = tile.occurenceValue;
            rot.rotation = i;
            for (int j = 0; j < 6; j++)
            {
                rot.sequence[j] = tile.sequence[(6-i + j) % 6];
            }
            rotatedPossibilities.Add(rot);
        }
        return rotatedPossibilities;
    }
    
    void MakeTerrainPossibilityLists(List<HexagoneTile> allTiles)
    {
        foreach (var terrain in TERRAINS)
        {
            List<HexagoneTile> northWest = new List<HexagoneTile>();
            List<HexagoneTile> northEast = new List<HexagoneTile>();
            List<HexagoneTile> southWest = new List<HexagoneTile>();
            List<HexagoneTile> southEast = new List<HexagoneTile>();
            List<HexagoneTile> west = new List<HexagoneTile>();
            List<HexagoneTile> east = new List<HexagoneTile>();

            foreach (var tile in allTiles)
            {
                if (tile.sequence[3] == terrain)
                {
                    northWest.Add(tile);
                    tile.southEast = southEast;
                }

                if (tile.sequence[4] == terrain)
                {
                    northEast.Add(tile);
                    tile.southWest = southWest;
                }

                if (tile.sequence[5] == terrain)
                {
                    east.Add(tile);
                    tile.west = west;
                }

                if (tile.sequence[0] == terrain)
                {
                    southEast.Add(tile);
                    tile.northWest = northWest;
                }

                if (tile.sequence[1] == terrain)
                {
                    southWest.Add(tile);
                    tile.northEast = northEast;
                }

                if (tile.sequence[2] == terrain)
                {
                    west.Add(tile);
                    tile.east = east;
                }
                
                 #if UNITY_EDITOR
                EditorUtility.SetDirty(tile);   
                AssetDatabase.SaveAssets();
                #endif
            }
        }
    }
}
