using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
    [Range(0, 100)] public float forestProbability;
    [Range(0, 100)] public float strangeProbability;



    bool jobRunning = false;
    JobHandle jobHandle;

    List<Vector2Int> collapsed;


    void Start()
    {
        basegen.size = new Vector2Int(grid_width, grid_height);
        Generate();
    }

    void Update()
    {
        if(jobRunning && jobHandle.IsCompleted)
        {
            jobRunning = false;
            jobHandle.Complete();

            OnComplete();
        }
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
        GenerateNewSeed();
        InitSeed();
    }


    void Solve()
    {
        InitSeed();
        gridBeforeSolving = (HexagoneTile[,])grid.Clone();
    }

    [ContextMenu("Generate")]   
    void Generate()
    {
        Init();

        GenerateJob job = new GenerateJob
        {
            gridPossibilities = this.gridPossibilities,
            collapsed = this.collapsed,
            water = setup.water[0],
            grid_width = this.grid_width,
            grid_height = this.grid_height
        };

        jobHandle = job.Schedule();
        jobRunning = true;

    }

    void OnComplete()
    {
        for (int i = 0; i < grid_width; i++)
        {
            for (int j = 0; j < grid_height; j++)
            {
                Debug.Assert(gridPossibilities[i, j].Count == 1, "WFC has failed");
                grid[i, j] = gridPossibilities[i, j][0];
            }
        }
        StartCoroutine(basegen.ShowCoroutine(grid, collapsed));
    }


    void ChangeOccurenceValues()
    {
        setup.SetupOccurenceValue(grassProbability, waterProbability, roadProbability, coastProbability, riverProbability, forestProbability, strangeProbability);
    }

    void InitBase()
    {
        basegen.size = new Vector2Int(grid_width, grid_height);
    }

   
    void GenerateNewSeed()
    {
        seed = UnityEngine.Random.Range(0, int.MaxValue);
    }

    void InitSeed()
    {
        UnityEngine.Random.InitState(seed);
    }

    public struct GenerateJob : IJob
    {

        public List<HexagoneTile>[,] gridPossibilities;
        public List<Vector2Int> collapsed;
        public HexagoneTile water;
        public int grid_width;
        public int grid_height;
        public void Execute()
        {
            Generate();
        }

        void Generate()
        {

            collapsed = new List<Vector2Int>();
            InitWater(collapsed);


            //1) pick random number
            int rx = UnityEngine.Random.Range(1, grid_width - 1);// 1 et -1 pour l'eau
            int ry = UnityEngine.Random.Range(1, grid_height - 1);//1 et -1 pour l'eau

            List<Vector2Int> todo = new List<Vector2Int>();
            for (int i = 1; i < grid_width - 1; i++)
            {
                for (int j = 1; j < grid_height - 1; j++)
                {
                    todo.Add(new Vector2Int(i, j));
                }
            }


            //2) go to that tile and do
            Collapse(new Vector2Int(rx, ry), collapsed, todo);

        }

        void InitWater(List<Vector2Int> collapsed)
        {
            for (int i = 0; i < grid_width; i++)
            {
                Vector2Int bottom = new Vector2Int(i, 0);
                Vector2Int top = new Vector2Int(i, grid_height - 1);
                Collapse(bottom, water);
                collapsed.Add(bottom);
                Collapse(top, water);
                collapsed.Add(top);
            }

            for (int i = 1; i < grid_height - 1; i++)
            {
                Vector2Int left = new Vector2Int(0, i);
                Vector2Int right = new Vector2Int(grid_width - 1, i);
                Collapse(left, water);
                collapsed.Add(left);
                Collapse(right, water);
                collapsed.Add(right);
            }
        }
        
         void Propagate(Vector2Int target)
        {
            Vector2Int[] neighbours = GetNeighbours(target);

            //merge all possibilities
            HashSet<HexagoneTile>[] adjacencyPossibilities = new HashSet<HexagoneTile>[6]{  new HashSet<HexagoneTile>(300),
                                                                                            new HashSet<HexagoneTile>(300),
                                                                                            new HashSet<HexagoneTile>(300),
                                                                                            new HashSet<HexagoneTile>(300),
                                                                                            new HashSet<HexagoneTile>(300),
                                                                                            new HashSet<HexagoneTile>(300), };

            //RAJOUTER NE PAS MAJ LES COLLAPSED                                                                                     
            createHashet(adjacencyPossibilities, target);
            //update
            List<Vector2Int> nextToPropagate = new List<Vector2Int>();
            for (int i = 0; i < 6; i++)
            {
                if (!(neighbours[i].x >= grid_width || neighbours[i].y >= grid_height || neighbours[i].x < 0 || neighbours[i].y < 0))// si pas en dehors de la grid
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

                    if (asChanged) nextToPropagate.Add(neighbours[i]); // only propagate if the possibilities have changed
                }
            }

            foreach (var next in nextToPropagate)
            {
                Propagate(next);
            }
        }

        void createHashet( HashSet<HexagoneTile>[] adjacencyPossibilities, Vector2Int target)
        {
            foreach (var possibility in gridPossibilities[target.x, target.y]) //TODO creer type de données qui stock ca des le dsebut pour pas avoir a faire ca du tt juste une fois au debut 
            {
                foreach (var item in possibility.northWest)
                {
                    adjacencyPossibilities[0].Add(item);
                }

                foreach (var item in possibility.northEast)
                {
                    adjacencyPossibilities[1].Add(item);
                }

                foreach (var item in possibility.west)
                {
                    adjacencyPossibilities[2].Add(item);
                }

                foreach (var item in possibility.east)
                {
                    adjacencyPossibilities[3].Add(item);
                }

                foreach (var item in possibility.southWest)
                {
                    adjacencyPossibilities[4].Add(item);
                }

                foreach (var item in possibility.southEast)
                {
                    adjacencyPossibilities[5].Add(item);
                }
            }
        }
        


        void Collapse(Vector2Int target, List<Vector2Int> collapsed, List<Vector2Int> todo)
        {
            HexagoneTile pick = RandomPick(gridPossibilities[target.x, target.y]);
            if (pick == null) throw new WFCError($"find a tile with 0 possibilities : {target}");
            gridPossibilities[target.x, target.y] = new List<HexagoneTile> { pick };
            collapsed.Add(target);
            todo.Remove(target);
            Propagate(target);
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

        void Collapse(Vector2Int target, HexagoneTile tileToSet)
        {
            HexagoneTile pick = tileToSet;
            if (pick == null) throw new WFCError($"find a tile with 0 possibilities : {target}");
            gridPossibilities[target.x, target.y] = new List<HexagoneTile> { pick };
            Propagate(target);
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

        Vector2Int[] GetNeighbours(Vector2Int v)
        {
            int y = v.y;
            int x = v.x;
            if (y % 2 != 0)
            {
                return new Vector2Int[] {  new Vector2Int(x, y + 1), new Vector2Int(x + 1, y + 1),
                                        new Vector2Int(x - 1, y), new Vector2Int(x + 1, y),
                                        new Vector2Int(x, y - 1), new Vector2Int(x + 1, y - 1) };
            }
            else
            {
                return new Vector2Int[] {  new Vector2Int(x - 1, y + 1), new Vector2Int(x, y + 1),
                                        new Vector2Int(x - 1, y), new Vector2Int(x + 1, y),
                                        new Vector2Int(x - 1, y - 1), new Vector2Int(x, y - 1) };
            }

        }


    }


}