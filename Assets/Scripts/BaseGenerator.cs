using System;
using Unity.VisualScripting;
using UnityEngine;

public class BaseGenerator : MonoBehaviour
{

    public GameObject placeHolder;
    public GameObject[,] grid;
    private const float SIDE_SIZE = 1.1547f;
    private float HAUTEUR;
    private const double ANGLE_EQUILATERAL = Math.PI/3;
    public Vector2Int size;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HAUTEUR = SIDE_SIZE * (float)Math.Sin(ANGLE_EQUILATERAL);
        if (placeHolder != null)
        {
            Generate();
        }
    }

    // Update is called once per frame
    void Update()
    {

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
                grid[x, y] = Instantiate(placeHolder, new Vector3(xPos, 0, yPos), Quaternion.identity, transform);
            }
        }
    }

    void Clean()
    {
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                Destroy(grid[x, y]);
                grid[x, y] = null;
            }
        }
    }
}
