using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public GameObject wallPrefab;
    private Cell[,] grid;
    private Stack<Cell> stack = new Stack<Cell>();
    private Cell currentCell;

    void Start()
    {
        InitializeGrid();
        GenerateMaze();
        CreateRandomEntranceAndExit();
        DrawMaze();
    }

    void CreateRandomEntranceAndExit()
    {
        (int, int) entrance = GetRandomEdgeCell();
        // (int, int) exit;

        // do
        // {
        //     exit = GetRandomEdgeCell();
        // } while (entrance == exit);

        RemoveWallForEntranceOrExit((Random.Range(0, width), 0));
        RemoveWallForEntranceOrExit((Random.Range(0, width), height - 1));
    }

    (int, int) GetRandomEdgeCell()
    {
        int edge = Random.Range(1, 2);
        edge++;
        int x = 0, y = 0;

        switch (edge)
        {
            case 0: // Left edge
                x = 0;
                y = Random.Range(0, height);
                break;
            case 1: // Right edge
                x = width - 1;
                y = Random.Range(0, height);
                break;
            case 2: // Bottom edge
                x = Random.Range(0, width);
                y = 0;
                break;
            case 3: // Top edge
                x = Random.Range(0, width);
                y = height - 1;
                break;
        }

        return (x, y);
    }

    void RemoveWallForEntranceOrExit((int x, int y) cell)
    {
        int x = cell.x;
        int y = cell.y;

        if (x == 0) grid[x, y].leftWall = false; // Left edge
        else if (x == width - 1) grid[x, y].rightWall = false; // Right edge
        else if (y == 0) grid[x, y].bottomWall = false; // Bottom edge
        else if (y == height - 1) grid[x, y].topWall = false; // Top edge
    }

    void InitializeGrid()
    {
        grid = new Cell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new Cell(x, y);
            }
        }
        currentCell = grid[0, 0];
    }

    void GenerateMaze()
    {
        currentCell.visited = true;
        while (true)
        {
            Cell nextCell = GetUnvisitedNeighbor(currentCell);
            if (nextCell != null)
            {
                stack.Push(currentCell);
                RemoveWall(currentCell, nextCell);
                currentCell = nextCell;
                currentCell.visited = true;
            }
            else if (stack.Count > 0)
            {
                currentCell = stack.Pop();
            }
            else
            {
                break;
            }
        }
    }

    Cell GetUnvisitedNeighbor(Cell cell)
    {
        List<Cell> neighbors = new List<Cell>();

        int x = cell.x;
        int y = cell.y;

        if (x > 0 && !grid[x - 1, y].visited) neighbors.Add(grid[x - 1, y]);
        if (x < width - 1 && !grid[x + 1, y].visited) neighbors.Add(grid[x + 1, y]);
        if (y > 0 && !grid[x, y - 1].visited) neighbors.Add(grid[x, y - 1]);
        if (y < height - 1 && !grid[x, y + 1].visited) neighbors.Add(grid[x, y + 1]);

        if (neighbors.Count > 0)
        {
            return neighbors[Random.Range(0, neighbors.Count)];
        }
        return null;
    }

    void RemoveWall(Cell current, Cell next)
    {
        int dx = current.x - next.x;
        int dy = current.y - next.y;

        if (dx == 1) { current.leftWall = false; next.rightWall = false; }
        else if (dx == -1) { current.rightWall = false; next.leftWall = false; }
        else if (dy == 1) { current.bottomWall = false; next.topWall = false; }
        else if (dy == -1) { current.topWall = false; next.bottomWall = false; }
    }

    void DrawMaze()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];
                Vector3 pos = new Vector3(x, 0, y);

                if (cell.topWall) Instantiate(wallPrefab, pos + new Vector3(0, 0, 0.5f), Quaternion.identity, this.transform);
                if (cell.bottomWall) Instantiate(wallPrefab, pos + new Vector3(0, 0, -0.5f), Quaternion.identity, this.transform);
                if (cell.leftWall) Instantiate(wallPrefab, pos + new Vector3(-0.5f, 0, 0), Quaternion.Euler(0, 90, 0), this.transform);
                if (cell.rightWall) Instantiate(wallPrefab, pos + new Vector3(0.5f, 0, 0), Quaternion.Euler(0, 90, 0), this.transform);
            }
        }
    }
}

public class Cell
{
    public int x, y;
    public bool visited = false;
    public bool topWall = true;
    public bool bottomWall = true;
    public bool leftWall = true;
    public bool rightWall = true;

    public Cell(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}
