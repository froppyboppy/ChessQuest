using System;
using UnityEngine;

public class Chessboard : MonoBehaviour
{
    // Art related
    [Header("Art stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;


    // Logic
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles; // tiles 2D array
    private Camera currentCamera;
    private Vector2Int currentHover = -Vector2Int.one; // store hitted tiles indexes locally
    private Vector3 bounds;

    private void Awake()
    {
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
    }

    private void Update()
    {
        // Generate game camera
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        // Mouse input as ray with 100 unit range
        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);

        // Mouse (Ray) is on board
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover")))
        {

            // Get tile coordinate hitted by mouse ray
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            // Check if new tile is hovered
            if (currentHover != hitPosition)
            {
                if (currentHover != -Vector2Int.one)
                {
                    // Reset previous tile to "Tile" layer
                    tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                }

                // Set the new tile to "Hover" layer
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
        }

        // Mouse (Ray) is not on chessboard
        else
        {
            if (currentHover != -Vector2Int.one)
            {
                // Reset the previous tile to "Tile" layer
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }
        }
    }


    // Generate Board methods
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;

        // Instance board tiles
        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountY; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        // 8x8 Grid
        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        // Vertex in every single corner of the square
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;

        // Triangle array
        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };
        mesh.vertices = vertices;
        mesh.triangles = tris;

        // light ajustment
        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    //Operations
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {

        // return actual hit tile index
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (tiles[x, y] == hitInfo)
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        // **DEBUG INFO:**
        if (hitInfo == null)
        {
            Debug.LogWarning("LookupTileIndex called with null hitInfo!");
        }

        // out of board index (-1) invalid!!!
        return -Vector2Int.one; // -1 -1

    }
}
