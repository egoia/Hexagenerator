using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGenerator : MonoBehaviour
{

    public GameObject placeHolder;
    public GameObject[,] grid;
    private const float SIDE_SIZE = 1.1547f;
    private float HAUTEUR;
    private const double ANGLE_EQUILATERAL = Math.PI / 3;
    [HideInInspector] public Vector2Int size;

    [Header("Animation")]
    public float animationHeight;
    public AnimationCurve curve;
    [Min(1)] public float animationTime = 1;
    public float spawnInterval = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HAUTEUR = SIDE_SIZE * (float)Math.Sin(ANGLE_EQUILATERAL);
        if (placeHolder != null)
        {
            //Generate();
        }
    }

    [ContextMenu("GenerateBase")]
    void Generate()
    {
        if (grid != null) Clean();
        grid = new GameObject[size.x, size.y];

        float yOffset = SIDE_SIZE + (SIDE_SIZE / 2);
        float xOffset = HAUTEUR * 2;

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                float xPos = x * xOffset;
                float yPos = y * yOffset; // Z en realité
                if (y % 2 != 0)
                {
                    xPos += HAUTEUR;
                }
                grid[x, y] = Instantiate(placeHolder, new Vector3(xPos, 0, yPos), placeHolder.transform.rotation, transform);
            }
        }
    }

    public IEnumerator ShowCoroutine(HexagoneTile[,] generatedGrid, List<Vector2Int> positions)
    {
        
        HAUTEUR = SIDE_SIZE * (float)Math.Sin(ANGLE_EQUILATERAL);
        if (grid != null) Clean();
        grid = new GameObject[size.x, size.y];

        float yOffset = SIDE_SIZE + (SIDE_SIZE / 2);
        float xOffset = HAUTEUR * 2;

        int iterations = positions.Count;
        for (int i = 0; i < iterations; i++)
        {


            int r = UnityEngine.Random.Range(0, positions.Count);
            int x = positions[r].x;
            int y = positions[r].y;

            float xPos = x * xOffset;
            float yPos = y * yOffset; // Z en realité
            if (y % 2 != 0)
            {
                xPos += HAUTEUR;

            }
            StartCoroutine(SpawnTileCoroutine(generatedGrid[x, y], new Vector3(xPos, 0, yPos), positions[r]));
            //grid[x, y] = generatedGrid[x, y].Spawn(new Vector3(xPos, 0, yPos), transform);

            positions.Remove(positions[r]);

            yield return new WaitForSeconds(spawnInterval);
        }

        /*for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                float xPos = x * xOffset;
                float yPos = y * yOffset; // Z en realité
                if (y % 2 != 0)
                {
                    xPos += HAUTEUR;
                }
                grid[x, y] = generatedGrid[x, y].Spawn(new Vector3(xPos, 0, yPos), transform);
            }
        }*/
        yield return null;
    }

    void Clean()
    {
        Debug.Log(grid.GetLength(0) + " , " + grid.GetLength(1));
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                Debug.Log($"x : {x}, y : {y}");
                Destroy(grid[x, y]);
                grid[x, y] = null;
            }
        }
    }

    IEnumerator SpawnTileCoroutine(HexagoneTile tile, Vector3 position, Vector2Int gridPos)
    {
        Vector3 startPosition = position + new Vector3(0, animationHeight, 0);
        GameObject tileObj = tile.Spawn(startPosition, transform);
        grid[gridPos.x, gridPos.y] = tileObj;
        float t = 0;
        float timer = Time.time;
        while (t <= animationTime)
        {
            float interpolationValue = curve.Evaluate(t/animationTime);

            float interpolationHeight = Mathf.Lerp(animationHeight, position.y, interpolationValue);

            tileObj.transform.position = position + new Vector3(0, interpolationHeight, 0);
            t += Time.time - timer;
            timer = Time.time;
            yield return null;
        }
        tileObj.transform.position = position;
        yield return null;
    }
}
