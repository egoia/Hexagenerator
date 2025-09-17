using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public List<HexagoneTile> possibilities;
    List<HexagoneTile>[,] gridPossibilities;
    HexagoneTile[,] grid;
    HexagoneTile[,] gridBeforeSolving;
    public int grid_width = 2;
    public int grid_height = 2;
    public int seed;

    void Start()
    {
        Debug.Assert(possibilities != null && possibilities.Count != 0, "possibilities must be entered");
        grid = new HexagoneTile[grid_width, grid_height];
        gridBeforeSolving = new HexagoneTile[grid_width, grid_height];
        gridPossibilities = new List<HexagoneTile>[grid_width, grid_height];
        
        for(int i = 0; i< gridPossibilities.GetLength(0); i++)
        {
            for (int j = 0; j < gridPossibilities.GetLength(1); j++)
            {
                gridPossibilities[i, j] = new List<HexagoneTile>(possibilities);
            }
        }
    }

    void Solve()
    {
        InitSeed();
        gridBeforeSolving = (HexagoneTile[,])grid.Clone();
    }

    void Generate()
    {
        GenerateNewSeed();
        InitSeed();
        //1) pick random number
        //2) go to that tile and do
        //4) pick a random possibilitie
        //5) go through the grid (or just the neighbours) to remove every impossible tile -> UpdatePossibilities ?
        //3) until every neighbour is filled call this recursive function
    }

    void UpdatePossibilities()
    {
        
    }

    void GenerateNewSeed()
    {
        seed = Random.Range(0, int.MaxValue);
    }

    void InitSeed()
    {
        Random.InitState(seed);
    }

    void SolveUnsolvable()
    {
        //find a random picked tile in the neighbours and change it etc (propagate)
    }

    void TestUnsolvableOccurrence(int echantillonSize)
    {
        //start n generation and gives the number of them that went unsolvable
    }

}