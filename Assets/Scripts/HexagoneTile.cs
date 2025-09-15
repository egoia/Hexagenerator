using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum Terrain
{
    WATER, GRASS, COAST, CLIFF, RIVER, ROAD, NOTHING
}

[System.Serializable]
public struct TerrainProbability
{
    public Terrain terrain;
    [Range(0, 100)] public int probability;
}

[System.Serializable]
public class TerrainProbabilityList
{
    public List<TerrainProbability> probabilities = new List<TerrainProbability>();
}

[CreateAssetMenu(menuName = "Procedural Generation/HexagoneTile")]
public class HexagoneTile : ScriptableObject
{
    public List<TerrainProbabilityList> validUpTerrain = new List<TerrainProbabilityList>(6);
    public List<TerrainProbabilityList> validDownTerrain = new List<TerrainProbabilityList>(6);


    private void OnValidate()
    {
        if (validUpTerrain.Count != 6)
        {
            while (validUpTerrain.Count < 6)
                validUpTerrain.Add(new TerrainProbabilityList());
            while (validUpTerrain.Count > 6)
                validUpTerrain.RemoveAt(validUpTerrain.Count - 1);
        }

        if (validDownTerrain.Count != 6)
        {
            while (validDownTerrain.Count < 6)
                validDownTerrain.Add(new TerrainProbabilityList());
            while (validDownTerrain.Count > 6)
                validDownTerrain.RemoveAt(validDownTerrain.Count - 1);
        }
        
        if(validUpTerrain!=null && validDownTerrain !=null)NormalizeProbabilities();
    }

    void NormalizeProbabilities()
    {
        for (int j = 0; j < validUpTerrain.Count; j++)
        {
            List<TerrainProbability> item = validUpTerrain[j].probabilities;
            int sum = 0;
            foreach (var item2 in item)
            {
                sum += item2.probability;
            }

            if (sum != 100)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    TerrainProbability normalized = new TerrainProbability();
                    normalized.terrain = item[i].terrain;
                    if (sum == 0) normalized.probability = 100 / item.Count;
                    else normalized.probability = item[i].probability / sum * 100;
                    item[i] = normalized;
                }
            }
        }

        for (int j = 0; j < validDownTerrain.Count; j++)
        {
            List<TerrainProbability> item = validDownTerrain[j].probabilities;
            int sum = 0;
            foreach (var item2 in item)
            {
                sum += item2.probability;
            }

            if (sum != 100)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    TerrainProbability normalized = new TerrainProbability();
                    normalized.terrain = item[i].terrain;
                    if (sum == 0) normalized.probability = 100 / item.Count;
                    else normalized.probability = item[i].probability / sum * 100;
                    item[i] = normalized;
                }
            }
        }
    }
}