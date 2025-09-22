using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Showcase : MonoBehaviour
{
    public float space;
    public List<HexagoneTile> tiles;
    [ContextMenu("show all tiles")]
    void Show()
    {
        tiles.OrderBy(tile => tile.name).ToList();
        int i = 0;
        while (i < tiles.Count)
        {
            int x = i % 6;
            int y = i / 6;
            tiles[i].Spawn(new Vector3(x*space, 0, y*space), transform);
            i++;
        }
    }
}
