using System.Collections.Generic;
using UnityEngine;

public class Chessboard : MonoBehaviour
{
    // Art related
    [Header("Art Settings")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 2.0f;
    [SerializeField] private float yOffset = 6.0f;
    [SerializeField] private Vector3 boardCenter = new Vector3(-0.23f, 0.0f, 0.8f);
    [SerializeField] private float deathSize = 0.55f;
    [SerializeField] private float deathSpacing = 0.775f;

    // Prefabs
    [Header("Prefabs & Materials")]
    // [SerializeField] private GameObject[] prefabs;
    // [SerializeField] private Material[] teamMaterials;

    // New
    [SerializeField] private GameObject[] whitePiecePrefabs;
    [SerializeField] private GameObject[] blackPiecePrefabs;
    [SerializeField] private Material[] teamMaterials;


    // Constants
    private const int TILE_COUNT_X = 8; // chessboard x size
    private const int TILE_COUNT_Y = 8; // chessboard y size

    // Tiles, hover tiles, and camera
    private GameObject[,] tiles; // tiles 2D array
    private Camera currentCamera; // camera use in game mode lol
    private Vector2Int currentHover = -Vector2Int.one; // store hitted tile index
    private Vector3 bounds;

    // Chess pieces logic
    private ChessPiece[,] chessPieces; // array contaning pieces in chessboard
    private ChessPiece currentDraggingPiece;
    private List<ChessPiece> deadWhilesList = new List<ChessPiece>();
    private List<ChessPiece> deadBlacksList = new List<ChessPiece>();
    private List<Vector2Int> availableMoves = new List<Vector2Int>();


    // Main function running whole script
    private void Awake()
    {
        // Generates tiles layer
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);

        // Spawn all pieces
        SpawnAllPieces();
        //SpawnSinglePiece(ChessPieceType.King, 1);

        // Position pieces
        PositionAllPieces();
    }

    // Updates game every time user or chessboard moves
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
            // Logging the name of the object that the raycast hit
            // Debug.Log("Raycast hit: " + info.transform.name);

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

            // Mouse press down
            if (Input.GetMouseButtonDown(0))
            {
                if (chessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    // Check turn
                    if (true)
                    {
                        currentDraggingPiece = chessPieces[hitPosition.x, hitPosition.y];

                        // available moves list
                        availableMoves = currentDraggingPiece.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);

                        // Highlight valid move tiles
                        HighlightTiles();
                    }
                }
            }


            // Mouse realeasing with a chess piece
            if (currentDraggingPiece != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(currentDraggingPiece.currentX, currentDraggingPiece.currentY);

                bool validMove = MoveTo(currentDraggingPiece, hitPosition.x, hitPosition.y);

                // No valid move
                if (!validMove)
                {
                    currentDraggingPiece.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                    currentDraggingPiece = null;
                }

                else
                {
                    currentDraggingPiece = null;
                }
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

            // release chesspiece after dragging
            if (currentDraggingPiece && Input.GetMouseButtonUp(0))
            {
                currentDraggingPiece.SetPosition(GetTileCenter(currentDraggingPiece.currentX, currentDraggingPiece.currentY));
                currentDraggingPiece = null;
            }
        }

        // Dragging effect
        if (currentDraggingPiece)
        {
            Plane horizantalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if (horizantalPlane.Raycast(ray, out distance))
            {
                currentDraggingPiece.SetPosition(ray.GetPoint(distance));
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


    // All pieces spawning in board
    private void SpawnAllPieces()
    {
        // chess pieces array instance
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];

        // team codes
        int whiteTeamCode = 0, blacktTeamCode = 1;

        // white team
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeamCode);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeamCode);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeamCode);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeamCode);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeamCode);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeamCode);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeamCode);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeamCode);


        for (int pawns = 0; pawns < TILE_COUNT_X; pawns++)
        {
            // white pawn spawning
            chessPieces[pawns, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeamCode);
        }

        // black team
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blacktTeamCode);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blacktTeamCode);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blacktTeamCode);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, blacktTeamCode);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, blacktTeamCode);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blacktTeamCode);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blacktTeamCode);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blacktTeamCode);


        for (int pawns = 0; pawns < TILE_COUNT_X; pawns++)
        {
            // black pawn spawning
            chessPieces[pawns, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blacktTeamCode);
        }
    }

    /*
        Spawn single piece in board
        @param ChessPieceType type
        @param int team
    */
    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        // Use Instantiate without 'new'
        GameObject[] prefabsArray = team == 0 ? whitePiecePrefabs : blackPiecePrefabs;

        // Determine the rotation based on the team
        Quaternion rotation = team == 0 ? Quaternion.identity : Quaternion.Euler(0, 180, 0);
        GameObject pieceObject = Instantiate(prefabsArray[(int)type - 1], transform.position, rotation, transform);
        ChessPiece chessPiece = pieceObject.GetComponent<ChessPiece>();
        chessPiece.type = type;
        chessPiece.team = team;

        // Assuming you have corrected the access to MeshRenderer as discussed previously
        MeshRenderer meshRenderer = chessPiece.GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.material = teamMaterials[team];
        }

        return chessPiece;
    }

    // Positioning
    private void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    PositionSinglePiece(x, y, true);
                }
            }
        }
    }

    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].SetPosition(GetTileCenter(x, y), force);

    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    // Hightlight given tiles in availableMoves list
    private void HighlightTiles()
    {
        for (int move = 0; move < availableMoves.Count; move++)
        {
            tiles[availableMoves[move].x, availableMoves[move].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }

    // Remove hightlighted given tiles
    private void RemoveHighlightTiles()
    {
        for (int move = 0; move < availableMoves.Count; move++)
        {
            tiles[availableMoves[move].x, availableMoves[move].y].layer = LayerMask.NameToLayer("Tile");

            availableMoves.Clear();
        }
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

    private bool MoveTo(ChessPiece chessPiece, int x, int y)
    {
        Vector2Int previousPosition = new Vector2Int(chessPiece.currentX, chessPiece.currentY);

        // avoid placing pieces in same team positions
        if (chessPieces[x, y] != null)
        {
            ChessPiece otherChessPiece = chessPieces[x, y];

            if (chessPiece.team == otherChessPiece.team)
            {
                return false;
            }

            //avoid placing pieces in rival team positions
            // Avoid by 'killing' the rival
            if (otherChessPiece.team == 0)
            {
                // add to dead list : white
                deadWhilesList.Add(otherChessPiece);

                // Change scale and position outside chessboard
                otherChessPiece.SetScale(Vector3.one * deathSize);
                otherChessPiece.SetPosition(new Vector3(8 * tileSize, yOffset, -1 * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2) + (Vector3.forward * deathSpacing) * deadWhilesList.Count);

            }

            else
            {
                // add to dead list : white
                deadBlacksList.Add(otherChessPiece);

                // Change scale and position outside chessboard
                otherChessPiece.SetScale(Vector3.one * deathSize);
                otherChessPiece.SetPosition(new Vector3(-1 * tileSize, yOffset, 8 * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2) + (Vector3.back * deathSpacing) * deadBlacksList.Count);

            }
        }

        chessPieces[x, y] = chessPiece;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);

        return true;

    }

}
