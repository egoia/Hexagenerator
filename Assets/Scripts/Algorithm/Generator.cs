using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using static HexagoneTile;
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

    NativeArray<NativeList<HexagoneTileData>> gridPossibilities;
    NativeList<Vector2Int> collapsed;
    HexagoneTileData[,] grid;
    HexagoneTileData[,] gridBeforeSolving;
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

    JobHandle handle;
    bool running;


    void Start()
    {
        basegen.size = new Vector2Int(grid_width, grid_height);
        Generate();
    }

    void Update()
    {
        if(running && handle.IsCompleted)
        {
            handle.Complete();
            running = false;
            OnComplete();
        }
    }

    void Init()
    {
        Debug.Assert(setup.allHexagoneTiles != null && setup.allHexagoneTiles.Count != 0, "possibilities must be entered");
        grid = new HexagoneTileData[grid_width, grid_height];
        gridBeforeSolving = new HexagoneTileData[grid_width, grid_height];
        gridPossibilities = new NativeArray<NativeList<HexagoneTileData>>(grid_width*grid_height, Allocator.Persistent);
        collapsed = new NativeList<Vector2Int>(Allocator.Persistent);

        NativeList<HexagoneTileData> gridPossibilities_setup = setup.GetAllHexagoneTiles();
        for (int i = 0; i < grid_width; i++)
        {
            for (int j = 0; j < grid_height; j++)
            {
                gridPossibilities[index1D(i,j,grid_width)] = new NativeList<HexagoneTileData>(Allocator.Persistent);
                gridPossibilities[index1D(i,j,grid_width)].AddRange(gridPossibilities_setup.AsArray());
            }
        }

        gridPossibilities_setup.Dispose();
        ChangeOccurenceValues();
        InitBase();        
        GenerateNewSeed();
        InitSeed();
    }


    void Solve()
    {
        InitSeed();
        gridBeforeSolving = (HexagoneTileData[,])grid.Clone();
    }

    [ContextMenu("Generate")]   
    void Generate()
    {
        Init();

        GenerateJob job = new GenerateJob
        {
            gridPossibilities = gridPossibilities,
            collapsed = collapsed,
            water = setup.ToThreadSafe(setup.water[0]),
            grid_width = grid_width,
            grid_height = grid_height
        };

        handle = job.Schedule();
        running = true;
    }

    void OnComplete()
    {
        for (int i = 0; i < grid_width; i++)
        {
            for (int j = 0; j < grid_height; j++)
            {
                Debug.Assert(gridPossibilities[index1D(i,j,grid_width)].Length == 1, "WFC has failed");
                grid[i, j] = gridPossibilities[index1D(i,j,grid_width)][0];
                gridPossibilities[index1D(i,j,grid_width)].Dispose();

            }
        }
        HexagoneTile[,] grid_tiles = new HexagoneTile[grid_width,grid_height];
        for (int i = 0; i < grid_width; i++)
        {
            for (int j = 0; j < grid_height; j++)
            {
                grid_tiles[i,j] = setup.allHexagoneTiles[grid[i,j].identifier];
                grid[i,j].Free();
            }
        }

        StartCoroutine(basegen.ShowCoroutine(grid_tiles, new List<Vector2Int>(collapsed)));
        collapsed.Dispose();
        gridPossibilities.Dispose();
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

    static int index1D(int x, int y, int grid_width)
    {
        return y*grid_width +x;
    }

    public struct GenerateJob : IJob
    {

        public NativeArray<NativeList<HexagoneTileData>> gridPossibilities;
        public NativeList<Vector2Int> collapsed;
        public HexagoneTileData water;
        public int grid_width;
        public int grid_height;
        public void Execute()
        {
            Generate();
        }



        void Generate()
        {
            InitWater(collapsed);


            //1) pick random number
            int rx = UnityEngine.Random.Range(1, grid_width - 1);// 1 et -1 pour l'eau
            int ry = UnityEngine.Random.Range(1, grid_height - 1);//1 et -1 pour l'eau

            NativeList<Vector2Int> todo = new NativeList<Vector2Int>(Allocator.Persistent);
            for (int i = 1; i < grid_width - 1; i++)
            {
                for (int j = 1; j < grid_height - 1; j++)
                {
                    todo.Add(new Vector2Int(i, j));
                }
            }


            //2) go to that tile and do
            Collapse(new Vector2Int(rx, ry), collapsed, todo);
            todo.Dispose();
        }

        void InitWater(NativeList<Vector2Int> collapsed)
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
            NativeArray<Vector2Int> neighbours = GetNeighbours(target);

            //merge all possibilities
            NativeHashMap<int, bool>[] adjacencyPossibilities = new NativeHashMap<int, bool>[6]{  new NativeHashMap<int, bool>(300, Allocator.Temp),
                                                                                            new NativeHashMap<int, bool>(300, Allocator.Temp),
                                                                                            new NativeHashMap<int, bool>(300, Allocator.Temp),
                                                                                            new NativeHashMap<int, bool>(300, Allocator.Temp),
                                                                                            new NativeHashMap<int, bool>(300, Allocator.Temp),
                                                                                            new NativeHashMap<int, bool>(300, Allocator.Temp), };

            //RAJOUTER NE PAS MAJ LES COLLAPSED                                                                                     
            createHashMap(ref adjacencyPossibilities, target);
            //update
            NativeList<Vector2Int> nextToPropagate = new NativeList<Vector2Int>();
            for (int i = 0; i < 6; i++)
            {
                if (!(neighbours[i].x >= grid_width || neighbours[i].y >= grid_height || neighbours[i].x < 0 || neighbours[i].y < 0))// si pas en dehors de la grid
                {
                    bool asChanged = false;
                    NativeList<HexagoneTileData> updatedPossibilities = new NativeList<HexagoneTileData>(Allocator.Temp);
                    updatedPossibilities.AddRange(gridPossibilities[index1D(neighbours[i].x, neighbours[i].y, grid_width)].AsArray());

                    foreach (var neighbourPossibility in gridPossibilities[index1D(neighbours[i].x, neighbours[i].y, grid_width)])
                    {
                        if (!adjacencyPossibilities[i].ContainsKey(neighbourPossibility.identifier))
                        {
                            updatedPossibilities = RemoveDataFromNativeList(updatedPossibilities, neighbourPossibility);
                            asChanged = true;
                        }
                    }
                    gridPossibilities[index1D(neighbours[i].x, neighbours[i].y, grid_width)].Dispose();
                    gridPossibilities[index1D(neighbours[i].x, neighbours[i].y, grid_width)] = updatedPossibilities;

                    if (asChanged) nextToPropagate.Add(neighbours[i]); // only propagate if the possibilities have changed
                }
            }
            foreach (var item in adjacencyPossibilities)
            {
                item.Dispose();
            }

            foreach (var next in nextToPropagate)
            {
                Propagate(next);
            }
        }

        private NativeList<HexagoneTileData> RemoveDataFromNativeList( NativeList<HexagoneTileData> updatedPossibilities, HexagoneTileData neighbourPossibility)
        {
            for (int i =0; i<updatedPossibilities.Length; i++ )
            {
                if (updatedPossibilities[i].identifier == neighbourPossibility.identifier)
                {
                    updatedPossibilities.RemoveAt(i);
                    break;
                } 
            }
            return updatedPossibilities;
        }

        private NativeList<Vector2Int> RemoveFromNativeList(NativeList<Vector2Int> list, Vector2Int obj)
        {
            for (int i =0; i<list.Length; i++ )
            {
                if (list[i]== obj)
                {
                    list.RemoveAt(i);
                    break;
                } 
            }
            return list;
        }



        void createHashMap(ref NativeHashMap<int, bool>[] adjacencyPossibilities, Vector2Int target)
        {
            foreach (var possibility in gridPossibilities[index1D(target.x, target.y, grid_width)]) //TODO creer type de données qui stock ca des le dsebut pour pas avoir a faire ca du tt juste une fois au debut 
            {
                foreach (var item in possibility.northWest)
                {
                    adjacencyPossibilities[0].Add(item, true);
                }

                foreach (var item in possibility.northEast)
                {
                    adjacencyPossibilities[1].Add(item, true);
                }

                foreach (var item in possibility.west)
                {
                    adjacencyPossibilities[2].Add(item, true);
                }

                foreach (var item in possibility.east)
                {
                    adjacencyPossibilities[3].Add(item, true);
                }

                foreach (var item in possibility.southWest)
                {
                    adjacencyPossibilities[4].Add(item, true);
                }

                foreach (var item in possibility.southEast)
                {
                    adjacencyPossibilities[5].Add(item, true);
                }
            }
        }
        


        void Collapse(Vector2Int target, NativeList<Vector2Int> collapsed, NativeList<Vector2Int> todo)
        {
            HexagoneTileData pick = RandomPick(gridPossibilities[index1D(target.x, target.y, grid_width)]);
            gridPossibilities[index1D(target.x, target.y, grid_width)].Dispose();
            gridPossibilities[index1D(target.x, target.y, grid_width)] = new NativeList<HexagoneTileData>(Allocator.Temp) { pick };
            collapsed.Add(target);
            todo = RemoveFromNativeList(todo, target);
            Propagate(target);
            if (todo.Length == 0) return;
            float min = float.PositiveInfinity;
            Vector2Int realNext = new Vector2Int();

            foreach (var next in todo)
            {
                if (gridPossibilities[index1D(next.x, next.y, grid_width)].Length < min)
                {
                    min = gridPossibilities[index1D(next.x, next.y,grid_width)].Length;
                    realNext = next;
                }
            }
            Collapse(realNext, collapsed, todo);
        }

        void Collapse(Vector2Int target, HexagoneTileData tileToSet)
        {
            HexagoneTileData pick = tileToSet;
            gridPossibilities[index1D(target.x, target.y, grid_width)].Dispose();
            gridPossibilities[index1D(target.x, target.y, grid_width)] = new NativeList<HexagoneTileData> { pick };
            Propagate(target);
        }

        HexagoneTileData RandomPick(NativeList<HexagoneTileData> possibilities)
        {
            float entropy = 0;
            foreach (var possibility in possibilities)
            {
                entropy += possibility.occurence;
            }

            float sum = 0;
            float r = UnityEngine.Random.Range(0, entropy);
            foreach (var possibility in possibilities)
            {
                sum += possibility.occurence;
                if (r <= sum) return possibility;
            }
            throw new WFCError($"find a tile with 0 possibilities");
        }

        NativeArray<Vector2Int> GetNeighbours(Vector2Int v)
        {
            int y = v.y;
            int x = v.x;
            if (y % 2 != 0)
            {
                NativeArray<Vector2Int> res =  new NativeArray<Vector2Int>(6,Allocator.Persistent);
                res[0] = new Vector2Int(x, y + 1);
                res[1] = new Vector2Int(x + 1, y + 1);
                res[2] = new Vector2Int(x - 1, y);
                res[3] = new Vector2Int(x + 1, y);
                res[4] = new Vector2Int(x, y - 1);
                res[5] = new Vector2Int(x + 1, y - 1);
                return res;
            }
            else
            {
                NativeArray<Vector2Int> res =  new NativeArray<Vector2Int>(6,Allocator.Persistent);
                res[0] = new Vector2Int(x - 1, y + 1);
                res[1] = new Vector2Int(x, y + 1);
                res[2] = new Vector2Int(x - 1, y);
                res[3] = new Vector2Int(x + 1, y);
                res[4] =  new Vector2Int(x - 1, y - 1);
                res[5] = new Vector2Int(x, y - 1);
                return res;
            }

        }


    }


}