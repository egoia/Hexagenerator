using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Generator : MonoBehaviour
{

    public class WFCError : Exception
    {
        public WFCError() { }

        public WFCError(string message) 
            : base(message) { }

        public WFCError(string message, Exception inner) 
            : base(message, inner) { }
    }
    List<HexagoneTile>[,] gridPossibilities;
    HexagoneTile[,] grid;
    HexagoneTile[,] gridBeforeSolving;
    public int grid_width = 2;
    public int grid_height = 2;
    public int seed;

    public SetupTiles setup;
    public BaseGenerator basegen;

    [Range(0, 100)] public float waterProbability;
    [Range(0,100)]public float grassProbability;
    [Range(0,100)]public float roadProbability;
    [Range(0,100)]public float coastProbability;
    [Range(0,100)]public float riverProbability;
    [Range(0, 100)] public float strangeProbability;

    void Start()
    {
        Generate();
    }

    void Init()
    {
        Debug.Assert(setup.allHexagoneTiles != null && setup.allHexagoneTiles.Count != 0, "possibilities must be entered");
        grid = new HexagoneTile[grid_width, grid_height];
        gridBeforeSolving = new HexagoneTile[grid_width, grid_height];
        gridPossibilities = new List<HexagoneTile>[grid_width, grid_height];

        for (int i = 0; i < gridPossibilities.GetLength(0); i++)
        {
            for (int j = 0; j < gridPossibilities.GetLength(1); j++)
            {
                gridPossibilities[i, j] = new List<HexagoneTile>(setup.allHexagoneTiles);
            }
        }
        ChangeOccurenceValues();
        InitBase();
    }

    void Solve()
    {
        InitSeed();
        gridBeforeSolving = (HexagoneTile[,])grid.Clone();
    }

    [ContextMenu("Generate")]
    void Generate()
    {
        GenerateNewSeed();
        InitSeed();
        Init();

        //1) pick random number
        int rx = UnityEngine.Random.Range(0, grid_width);
        int ry = UnityEngine.Random.Range(0, grid_height);

        List<Vector2Int> todo = new List<Vector2Int>();
        for (int i = 0; i < grid_width; i++)
        {
            for (int j = 0; j < grid_height; j++)
            {
                todo.Add(new Vector2Int(i, j));
            }
        }

        //2) go to that tile and do
        Collapse(new Vector2Int(rx, ry), new HashSet<Vector2Int>(), todo);

        for (int i = 0; i < grid_width; i++)
        {
            for (int j = 0; j < grid_height; j++)
            {
                Debug.Assert(gridPossibilities[i, j].Count == 1, "WFC has failed");
                grid[i, j] = gridPossibilities[i, j][0];
            }
        }
        basegen.Show(grid);
    }

    void ChangeOccurenceValues()
    {
        setup.SetupOccurenceValue(grassProbability, waterProbability, roadProbability, coastProbability, riverProbability, strangeProbability);
    }

    void InitBase()
    {
        basegen.size = new Vector2Int(grid_width, grid_height);
    }

    void Propagate(Vector2Int target, HashSet<Vector2Int> updated, HashSet<Vector2Int> collapsed)
    {
        Vector2Int[] neighbours = GetNeighbours(target);

        //merge all possibilities
        HashSet<HexagoneTile>[] adjacencyPossibilities = new HashSet<HexagoneTile>[6]{  new HashSet<HexagoneTile>(),
                                                                                        new HashSet<HexagoneTile>(),
                                                                                        new HashSet<HexagoneTile>(),
                                                                                        new HashSet<HexagoneTile>(),
                                                                                        new HashSet<HexagoneTile>(),
                                                                                        new HashSet<HexagoneTile>(), };
        if (!updated.Contains(neighbours[0]) && !collapsed.Contains(neighbours[0]))
        {
            foreach (var possibility in gridPossibilities[target.x, target.y])
            {
                adjacencyPossibilities[0].UnionWith(possibility.northWest);
            }
        }

        if (!updated.Contains(neighbours[1]) && !collapsed.Contains(neighbours[1]))
        {
            foreach (var possibility in gridPossibilities[target.x, target.y])
            {
                adjacencyPossibilities[1].UnionWith(possibility.northEast);
            }
        }

        if (!updated.Contains(neighbours[2]) && !collapsed.Contains(neighbours[2]))
        {
            foreach (var possibility in gridPossibilities[target.x, target.y])
            {
                adjacencyPossibilities[2].UnionWith(possibility.west);
            }
        }

        if (!updated.Contains(neighbours[3]) && !collapsed.Contains(neighbours[3]))
        {
            foreach (var possibility in gridPossibilities[target.x, target.y])
            {
                adjacencyPossibilities[3].UnionWith(possibility.east);
            }
        }

        if (!updated.Contains(neighbours[4]) && !collapsed.Contains(neighbours[4]))
        {
            foreach (var possibility in gridPossibilities[target.x, target.y])
            {
                adjacencyPossibilities[4].UnionWith(possibility.southEast);
            }
        }

        if (!updated.Contains(neighbours[5]) && !collapsed.Contains(neighbours[5]))
        {
            foreach (var possibility in gridPossibilities[target.x, target.y])
            {
                adjacencyPossibilities[5].UnionWith(possibility.southWest);
            }
        }

        //update
        List<Vector2Int> nextToPropagate = new List<Vector2Int>();
        for (int i = 0; i < 6; i++)
        {
            if (!updated.Contains(neighbours[i]) && !collapsed.Contains(neighbours[i]) && !(neighbours[i].x >= grid_width || neighbours[i].y >= grid_height || neighbours[i].x < 0 || neighbours[i].y < 0))// si pas en dehors de la grid
            {
                bool asChanged = false;
                List<HexagoneTile> updatedPossibilities = new List<HexagoneTile>(gridPossibilities[neighbours[i].x, neighbours[i].y]);
                foreach (var neighbourPossibility in gridPossibilities[neighbours[i].x, neighbours[i].y])
                {
                    if (!adjacencyPossibilities[i].Contains(neighbourPossibility))
                    {
                        updatedPossibilities.Remove(neighbourPossibility);
                        asChanged = true;
                    } 
                }
                gridPossibilities[neighbours[i].x, neighbours[i].y] = updatedPossibilities;

                if(asChanged)nextToPropagate.Add(neighbours[i]); // only propagate if the possibilities have changed
                updated.Add(neighbours[i]);
            }
        }

        Debug.Log(nextToPropagate);

        foreach (var next in nextToPropagate)
        {
            Propagate(next, updated, collapsed);
        }
    }

    void Collapse(Vector2Int target, HashSet<Vector2Int> collapsed, List<Vector2Int> todo)
    {
        HexagoneTile pick = RandomPick(gridPossibilities[target.x, target.y]);
        if (pick == null) throw new WFCError($"find a tile with 0 possibilities : {target}");
        gridPossibilities[target.x, target.y] = new List<HexagoneTile> { pick };
        collapsed.Add(target);
        todo.Remove(target);
        Debug.Log("before propagate");
        Propagate(target, new HashSet<Vector2Int>{target}, collapsed);
        Debug.Log("after propagate");

        if (todo.Count == 0) return;
        float min = float.PositiveInfinity;
        Vector2Int realNext = new Vector2Int();
        foreach (var next in todo)
        {
            if (gridPossibilities[next.x, next.y].Count < min)
            {
                min = gridPossibilities[next.x, next.y].Count;
                realNext = next;
            }
        }
        Collapse(realNext, collapsed, todo);
    }

    HexagoneTile RandomPick(List<HexagoneTile> possibilities)
    {
        float entropy = 0;
        foreach (var possibility in possibilities)
        {
            entropy += possibility.occurenceValue;
        }

        float sum = 0;
        float r = UnityEngine.Random.Range(0, entropy);
        foreach (var possibility in possibilities)
        {
            sum += possibility.occurenceValue;
            if (r <= sum) return possibility;
        }
        return null;
    }


    void GenerateNewSeed()
    {
        seed = UnityEngine.Random.Range(0, int.MaxValue);
    }

    void InitSeed()
    {
        UnityEngine.Random.InitState(seed);
    }

    Vector2Int[] GetNeighbours(Vector2Int v)
    {
        int y = v.y;
        int x = v.x;
        if (y % 2 == 0)
        {
            return new Vector2Int[] {  new Vector2Int(x, y - 1), new Vector2Int(x + 1, y - 1),
                                    new Vector2Int(x - 1, y), new Vector2Int(x + 1, y),
                                    new Vector2Int(x, y + 1), new Vector2Int(x + 1, y + 1) };
        }
        else
        {
            return new Vector2Int[] {  new Vector2Int(x - 1, y - 1), new Vector2Int(x, y - 1),
                                    new Vector2Int(x - 1, y), new Vector2Int(x + 1, y),
                                    new Vector2Int(x - 1, y + 1), new Vector2Int(x, y + 1) };
        }

    }

}