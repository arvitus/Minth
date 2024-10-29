using UnityEngine;
using System.Collections;

public class Maze : MonoBehaviour
{
    public int sizeX, sizeY;

    public MazeCell cellPrefab;

    private MazeCell[,] cells;

    public void Generate()
    {
        cells = new MazeCell[sizeX, sizeY];
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                CreateCell(x, y);
            }
        }
    }

    private void CreateCell(int x, int y)
    {
        MazeCell newCell = Instantiate(cellPrefab);
        cells[x, y] = newCell;
        newCell.name = "Maze Cell " + x + ", " + y;
        newCell.transform.parent = transform;
        newCell.transform.localPosition = new Vector2(x - sizeX * 0.5f + 0.5f, y - sizeY * 0.5f + 0.5f);
    }
}
