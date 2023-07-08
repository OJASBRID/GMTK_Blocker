using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Mathematics;

public class MazeGenerator : MonoBehaviour
{
    private GameObject player;
    public Vector3 pf;
    public int width, height;
    public Material brick;
    public int2 val;
    public int[,] Maze;
    public int[] MazeOneD;
    private List<Vector3> pathMazes = new List<Vector3>();
    private Stack<Vector2> _tiletoTry = new Stack<Vector2>();
    private GameObject[,] gmxy ;
    private List<Vector2> offsets = new List<Vector2> { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) };
    private System.Random rnd = new System.Random();
    private int _width, _height;
    private Vector2 _currentTile;
    public Vector2 CurrentTile
    {
        get { return _currentTile; }
        private set
        {
            if (value.x < 1 || value.x >= this.width - 1 || value.y < 1 || value.y >= this.height - 1)
            {
                throw new ArgumentException("CurrentTile must be within the one tile border all around the maze");
            }
            if (value.x % 2 == 1 || value.y % 2 == 1)
            { _currentTile = value; }
            else
            {
                throw new ArgumentException("The current square must not be both on an even X-axis and an even Y-axis, to ensure we can get walls around all tunnels");
            }
        }
    }

    private static MazeGenerator instance;
    public static MazeGenerator Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 30;
        GenerateMaze();
        CreateExit();
        PlayerInstantiation();
        Scale_Translate();
        ConvertMaze();
    }

    void GenerateMaze()
    {   gmxy = new GameObject[width,height];
        Maze = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Maze[x, y] = 1;
            }
        }
        CurrentTile = Vector2.one;
        _tiletoTry.Push(CurrentTile);
        Maze = CreateMaze();
        GameObject ptype = null;

        for (int i = 0; i <= Maze.GetUpperBound(0); i++)
        {
            for (int j = 0; j <= Maze.GetUpperBound(1); j++)
            {
                if (Maze[i, j] == 1)
                {
                    ptype = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    ptype.transform.position = new Vector3(i * ptype.transform.localScale.x, j * ptype.transform.localScale.y, 0);
                    if (brick != null)
                    {
                        gmxy[i, j] = ptype;
                        ptype.GetComponent<Renderer>().material = brick;
                    }
                    ptype.transform.parent = transform;
                }
                else if (Maze[i, j] == 0)
                {
                    pathMazes.Add(new Vector3(i, j, 0));
                }

            }
        }
    }

    void PlayerInstantiation ()
    {
        int mid_x = width/2;
        int mid_y = height/2;

        if(Maze[mid_x, mid_y] == 1)
        {
            if (Maze[mid_x + 1, mid_y] != 1)
            {
                mid_x += 1;

            }
            else if (Maze[mid_x, mid_y + 1] != 1)
            {
                mid_y += 1;
            }
            else if (Maze[mid_x - 1, mid_y] != 1)
            {
                mid_x -= 1;
            }
            else if (Maze[mid_x, mid_y - 1] != 1)
            {
                mid_y -= 1;
            }
            else if (Maze[mid_x + 1, mid_y + 1] != 1)
            {
                mid_x += 1;
                mid_y += 1;
            }
            else if (Maze[mid_x + 1, mid_y - 1] != 1)
            {
                mid_x += 1;
                mid_y -= 1;
            }
            else if (Maze[mid_x - 1, mid_y + 1] != 1)
            {
                mid_x -= 1;
                mid_y += 1;
            }
            else if (Maze[mid_x - 1, mid_y - 1] != 1)
            {
                mid_x -= 1;
                mid_y -= 1;
            }
        }
        val.x = mid_x;
        val.y = mid_y;
        Debug.Log(mid_x);
        player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        player.transform.SetParent(transform);
        player.transform.position = new Vector3(mid_x * player.transform.localScale.x, mid_y * player.transform.localScale.y, 0);
        player.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
    }

    void Scale_Translate()
    {
        Bounds parentBounds = GetBounds(transform);

        Camera camera = Camera.main;
        float screenHeight = 2f * camera.orthographicSize;

        float scaleFactor = screenHeight / parentBounds.size.y * 0.9f;

        transform.localScale = new Vector3(
            transform.localScale.x * scaleFactor,
            transform.localScale.y * scaleFactor,
            transform.localScale.z
        );

        // player.transform.localScale = new Vector3(
        //     player.transform.localScale.x * scaleFactor,
        //     player.transform.localScale.y * scaleFactor,
        //     player.transform.localScale.z
        // );

        transform.position = pf * scaleFactor;
        // player.transform.position = pf * scaleFactor;
    }

    private Bounds GetBounds(Transform transform)
    {
        Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(transform.position, Vector3.zero);

        foreach (Renderer renderer in renderers)
            bounds.Encapsulate(renderer.bounds);

        return bounds;
    }

    void CreateExit()
    {
        int x = 0;
        int y = UnityEngine.Random.Range(1, height - 1);
        while (Maze[1,y] == 1)
        {
            y = UnityEngine.Random.Range(1, height - 1);
        }
        Maze[x,y] = 2;
        gmxy[x,y].gameObject.SetActive(false);

        x = width - 1;
        y = UnityEngine.Random.Range(1, height - 1);
        while (Maze[x - 1, y] == 1)
        {
            y = UnityEngine.Random.Range(1, height - 1);
        }
        Maze[x, y] = 2;
        gmxy[x, y].gameObject.SetActive(false);

         y = 0;
         x = UnityEngine.Random.Range(1, width - 1);
        while (Maze[x, 1] == 1)
        {
            x = UnityEngine.Random.Range(1, width - 1);
        }
        Maze[x, y] = 2;
        gmxy[x, y].gameObject.SetActive(false);

        y = width - 1;
         x = UnityEngine.Random.Range(1, width - 1);
        while (Maze[x, y - 1] == 1)
        {
            x = UnityEngine.Random.Range(1, width - 1);
        }
        Maze[x, y] = 2;
        gmxy[x, y].gameObject.SetActive(false);

    }
    public int[,] CreateMaze()
    {
        //local variable to store neighbors to the current square
        //as we work our way through the maze
        List<Vector2> neighbors;
        //as long as there are still tiles to try
        while (_tiletoTry.Count > 0)
        {
            //excavate the square we are on
            Maze[(int)CurrentTile.x, (int)CurrentTile.y] = 0;

            //get all valid neighbors for the new tile
            neighbors = GetValidNeighbors(CurrentTile);

            //if there are any interesting looking neighbors
            if (neighbors.Count > 0)
            {
                //remember this tile, by putting it on the stack
                _tiletoTry.Push(CurrentTile);
                //move on to a random of the neighboring tiles
                CurrentTile = neighbors[rnd.Next(neighbors.Count)];
            }
            else
            {
                //if there were no neighbors to try, we are at a dead-end
                //toss this tile out
                //(thereby returning to a previous tile in the list to check).
                CurrentTile = _tiletoTry.Pop();
            }
        }

        return Maze;
    }
    /// <summary>
    /// Get all the prospective neighboring tiles
    /// </summary>
    /// <param name="centerTile">The tile to test</param>
    /// <returns>All and any valid neighbors</returns>
    private List<Vector2> GetValidNeighbors(Vector2 centerTile)
    {

        List<Vector2> validNeighbors = new List<Vector2>();

        //Check all four directions around the tile
        foreach (var offset in offsets)
        {
            //find the neighbor's position
            Vector2 toCheck = new Vector2(centerTile.x + offset.x, centerTile.y + offset.y);

            //make sure the tile is not on both an even X-axis and an even Y-axis
            //to ensure we can get walls around all tunnels
            if (toCheck.x % 2 == 1 || toCheck.y % 2 == 1)
            {
                //if the potential neighbor is unexcavated (==1)
                //and still has three walls intact (new territory)
                if (Maze[(int)toCheck.x, (int)toCheck.y] == 1 && HasThreeWallsIntact(toCheck))
                    {
                    //add the neighbor
                    validNeighbors.Add(toCheck);
                }
            }
        }

        return validNeighbors;
    }


    /// <summary>
    /// Counts the number of intact walls around a tile
    /// </summary>
    /// <param name="Vector2ToCheck">The coordinates of the tile to check</param>
    /// <returns>Whether there are three intact walls (the tile has not been dug into earlier.</returns>
    private bool HasThreeWallsIntact(Vector2 Vector2ToCheck)
    {
        int intactWallCounter = 0;

        //Check all four directions around the tile
        foreach (var offset in offsets)
        {
            //find the neighbor's position
            Vector2 neighborToCheck = new Vector2(Vector2ToCheck.x + offset.x, Vector2ToCheck.y + offset.y);

            //make sure it is inside the maze, and it hasn't been dug out yet
            if (IsInside(neighborToCheck) && Maze[(int)neighborToCheck.x, (int)neighborToCheck.y] == 1)
                {
                intactWallCounter++;
            }
        }

        //tell whether three walls are intact
        return intactWallCounter == 3;

    }

    private bool IsInside(Vector2 p)
    {
        return p.x >= 0  && p.y >= 0 && p.x < width && p.y < height;
    }

    private void ConvertMaze()
    {
        MazeOneD = new int[width * height];

        int index = 0;
        for(int i = 0; i < height; i++)
            for(int j = 0; j < width; j++)
            {
                MazeOneD[index++] = Maze[i, j];
            }
    }
}