using System.Collections.Generic;
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
        Init();
    }

    void Init()
    {
        Debug.Assert(possibilities != null && possibilities.Count != 0, "possibilities must be entered");
        grid = new HexagoneTile[grid_width, grid_height];
        gridBeforeSolving = new HexagoneTile[grid_width, grid_height];
        gridPossibilities = new List<HexagoneTile>[grid_width, grid_height];

        for (int i = 0; i < gridPossibilities.GetLength(0); i++)
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
        Init();

        //1) pick random number
        int rx = Random.Range(0, grid_width);
        int ry = Random.Range(0, grid_height);

        //2) go to that tile and do
        RecursiveGen(rx, ry);
        
    }

    void RecursiveGen(int x, int y)
    {
        //4) pick a random possibility
        List<HexagoneTile> currentPossibilities = gridPossibilities[x, y];
        int r = Random.Range(0, currentPossibilities.Count);
        gridPossibilities[x, y] = new List<HexagoneTile> { currentPossibilities[r] };
        //5) go through the grid (or just the neighbours) to remove every impossible tile -> UpdatePossibilities ?
        Vector2[] neighbours = GetNeighbours(x, y);
        UpdateNeighboursPossibilities(neighbours);
        //6) until every neighbour is filled call this recursive function
        for (int i = 0; i < 6; i++)
        {
            int nx = (int)neighbours[i].x;
            int ny = (int)neighbours[i].y;
            if (gridPossibilities[nx, ny].Count > 1) RecursiveGen(nx, ny);
        }
    }

    void UpdateNeighboursPossibilities(Vector2[] neighbours)
    {
        
    }

    void UpdateLocalPossibilities(int x, int y)
    {
        Vector2[] neighbours = GetNeighbours(x, y);
        

        for (int i = 0; i < 6; i++)
        {

        }
    }

    void UpdateAllPossibilities()
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

    Vector2[] GetNeighbours(int x, int y)
    {
        //ET LES BORDURES
        if (y % 2 == 0)
        {
            return new Vector2[] {  new Vector2(x, y - 1), new Vector2(x + 1, y - 1),
                                    new Vector2(x - 1, y), new Vector2(x + 1, y),
                                    new Vector2(x, y + 1), new Vector2(x + 1, y + 1) };
        }
        else
        {
            return new Vector2[] {  new Vector2(x - 1, y - 1), new Vector2(x, y - 1),
                                    new Vector2(x - 1, y), new Vector2(x + 1, y),
                                    new Vector2(x - 1, y + 1), new Vector2(x, y + 1) };
        }
    }

}