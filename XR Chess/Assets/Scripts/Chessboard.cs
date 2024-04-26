using System.Collections.Generic;
//using Unity.VisualScripting;
//using UnityEditor.ShaderGraph;
using UnityEngine;
//using UnityEngine.UIElements;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;
using System.Collections;
//using Unity.VisualScripting;
//using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
//using System.Text.Json;
//using System.Threading;
using System.Net.Http.Headers;
using UnityEditor;

public enum SpecialMove
{
    None = 0,
    EnPassant = 1,
    Castling = 2,
    Promotion = 3
}

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
    [SerializeField] private GameObject victoryScreen;

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
    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();
    private SpecialMove specialMove;
    private string mostRecentUserMove; // UCI notation of the most recent move

    // team turn
    private bool isWhiteTurn;

    // Lichess Stuff
    public string apiToken = "lip_mFklnRWDapFwCTqP1lDG"; // juan's api token
    public string gameId; // game id
                          //lichess move list
    private List<string> lichessMoveList = new List<string>();
    private bool blackMoveReceived;

    public int currentMoveNumber = 1;

    // Streaming stuff
    private Coroutine streamingCoroutine;
    private bool streaming = true;
    private bool gameIdStored = false; // Initialize gameIdStored



    // Main function running whole script
    private void Awake()
    {
        isWhiteTurn = true;
        blackMoveReceived = false;

        // Generates tiles layer
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);

        // Spawn all pieces
        SpawnAllPieces();

        // Position pieces
        PositionAllPieces();

        //start game
        //StartNewGame(1); // Level 1
        StartCoroutine(StartNewGameCoroutine(1));
        streamingCoroutine = StartCoroutine(StreamEventsCoroutine());

        EnterGame();
    }

    // game flow and transition through states aka alternating black and white turns
    private void EnterGame()
    {
        if (isWhiteTurn)
        {
            // user can make move

        }
        else
        {
            //wait for black move to be recieved

        }
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

        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
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
                    tiles[currentHover.x, currentHover.y].layer = ContainsValidMove(ref availableMoves, currentHover) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                }

                // Set the new tile to "Hover" layer
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
        }


        // - - - - - - - Testing - - - - - - - 

        //black turn
        //         if (!isWhiteTurn)
        //         {
        //             //Debug.Log("Black turn is about to happen");
        //             //Debug.Log("Current Move number is " + currentMoveNumber);

        //             // //here we need to wait on the logic until lichessMoveList is the same length as currentMoveNumber
        //             foreach (string wah in lichessMoveList)
        //             {
        //                 Debug.Log(wah);
        //             }
        //             Debug.Log("about to hit block");
        //             //HaveWeReceivedBlackMove();
        //             //blackMoveReceived = false;
        //             Debug.Log("Black turn has started");
        //             // get the last move from lichessMoveList
        //             // string lastMove = lichessMoveList[lichessMoveList.Count - 1];
        //             // //string uciMove = "d2d3";
        //             // Vector2Int[] move = ConvertToVectorNotation(lastMove);

        //             // // Convert the last move from UCI notation

        //             // ChessPiece blackP = chessPieces[2, 6];
        //             // int testx = 2;
        //             // int testy = 5;

        //             // Vector2Int hitPosition = new Vector2Int(testx, testy);
        //             // Get the last move from the list
        //             string lastMove = lichessMoveList[lichessMoveList.Count - 1];

        //             // Convert the last move from algebraic notation to vector notation
        //             Vector2Int[] move = ConvertToVectorNotation(lastMove);

        //             // Extract the start position from the move
        //             Vector2Int startPosition = move[0];

        //             // Extract the end position from the move
        //             Vector2Int endPosition = move[1];

        //             // Check if there's a piece at the start position
        //             // ChessPiece blackP = chessPieces[startPosition.x, startPosition.y];

        //             // // Define the hit position (e.g., where the piece is moving to)
        //             // Vector2Int hitPosition = endPosition;
        //             // int testx = hitPosition.x;
        //             // int testy = hitPosition.y;

        //             ChessPiece blackP = chessPieces[2, 6];
        //             int testx = 2;
        //             int testy = 5;

        //             Vector2Int hitPosition = new Vector2Int(testx, testy);

        // // // Check if the piece exists and if it's a black piece
        // // if (blackP != null && blackP.team == 1)

        //             if (blackP != null)
        //             {
        //                 if (blackP.team == 1) // Check it's a black piece just to be sure
        //                 {
        //                     // Move the test pawn
        //                     MoveTo(blackP, hitPosition.x, hitPosition.y);

        //                     // Update chessPieces array with the new position of blackP
        //                     chessPieces[hitPosition.x, hitPosition.y] = blackP;
        //                     chessPieces[blackP.currentX, blackP.currentY] = null;

        //                     // Update blackP's current position
        //                     blackP.currentX = hitPosition.x;
        //                     blackP.currentY = hitPosition.y;
        //                 }

        //                 // Apply additional logic if needed
        //                 Plane horizantalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
        //                 float distance = 0.0f;
        //                 if (horizantalPlane.Raycast(ray, out distance))
        //                 {
        //                     Vector3 testVector = new Vector3(testx, testy, yOffset);
        //                     blackP.SetPosition(testVector);
        //                 }
        //             }

        //             // Update the visual representation of the piece
        //             Vector3 newPosition = GetTileCenter(hitPosition.x, hitPosition.y);
        //             blackP.SetPosition(newPosition);
        //             currentMoveNumber++;
        //             isWhiteTurn = !isWhiteTurn;
        //             blackMoveReceived = false;

        //             Debug.Log("Black turn is over");
        //         }

        // - - - - - - - Testing - - - - - - - 

        // Mouse (Ray) is on board 
        // Allowed layers to position pieces
        if (isWhiteTurn)
        {
            if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
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
                        tiles[currentHover.x, currentHover.y].layer = ContainsValidMove(ref availableMoves, currentHover) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
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
                        if ((chessPieces[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn) || (chessPieces[hitPosition.x, hitPosition.y].team == 1 && !isWhiteTurn))
                        {
                            currentDraggingPiece = chessPieces[hitPosition.x, hitPosition.y];

                            // Highlight legal tiles for piece movement
                            availableMoves = currentDraggingPiece.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);

                            // Get list of special moves
                            specialMove = currentDraggingPiece.GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves);

                            // Check prevent
                            PreventCheck();

                            // Highlight valid move tiles
                            HighlightTiles();
                        }
                    }
                }


                // Mouse realeasing with a chess piece
                if (currentDraggingPiece != null && Input.GetMouseButtonUp(0))
                {
                    Vector2Int previousPosition = new Vector2Int(currentDraggingPiece.currentX, currentDraggingPiece.currentY);
                    //Debug.Log("move to: " + currentDraggingPiece, hitPosition.x, hitPosition.y);
                    string a = hitPosition.x.ToString();
                    string b = hitPosition.y.ToString();
                    //Debug.Log("Current Move number is " + currentMoveNumber);
                    //Debug.Log("move to: " + a + ", " + b);
                    bool validMove = MoveTo(currentDraggingPiece, hitPosition.x, hitPosition.y);
                    //Debug.Log("Starting delay?");
                    StartCoroutine(BlackMoveCoroutine());
                    //Debug.Log("Ending delay?");
                    // No valid move
                    if (!validMove)
                    {
                        Debug.Log("Blacks got here");
                        currentDraggingPiece.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                        currentDraggingPiece = null;
                    }

                    else
                    {
                        currentDraggingPiece = null;
                    }
                    RemoveHighlightTiles();
                }
            }

            // Mouse (Ray) is not on chessboard
            else
            {
                if (currentHover != -Vector2Int.one)
                {
                    // Reset the previous tile to "Tile" layer
                    tiles[currentHover.x, currentHover.y].layer = ContainsValidMove(ref availableMoves, currentHover) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                    currentHover = -Vector2Int.one;
                }

                // release chesspiece after dragging
                if (currentDraggingPiece && Input.GetMouseButtonUp(0))
                {
                    currentDraggingPiece.SetPosition(GetTileCenter(currentDraggingPiece.currentX, currentDraggingPiece.currentY));
                    currentDraggingPiece = null;
                    RemoveHighlightTiles();
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

        else
        {
            //Debug.Log("ingame");
            if (blackMoveReceived)
            {
                //Debug.Log("Black move received");
                //if (!isWhiteTurn)
                //{
                //Debug.Log("Black turn is about to happen");
                //Debug.Log("Current Move number is " + currentMoveNumber);

                // //here we need to wait on the logic until lichessMoveList is the same length as currentMoveNumber
                //foreach (string wah in lichessMoveList)
                // {
                //     Debug.Log(wah);
                // }
                // STATIC TEST CASE FOR BLACK MOVE

                // ChessPiece blackP = chessPieces[2, 6];
                // int testx = 2;
                // int testy = 5;

                // Vector2Int hitPosition = new Vector2Int(testx, testy);

                // DYNAMIC TEST CASE FOR BLACK MOVE
                string lastMoveTemp = lichessMoveList[lichessMoveList.Count - 1];
                Vector2Int[] tempMove = ConvertToVectorNotation(lastMoveTemp);
                Vector2Int startPosition = tempMove[0];
                Vector2Int endPosition = tempMove[1];

                ChessPiece blackP = chessPieces[startPosition.x, startPosition.y];

                //chessPieces[hitPosition.x, hitPosition.y].team = 1;

                int testx = endPosition.x;
                int testy = endPosition.y;

                Vector2Int hitPosition = new Vector2Int(testx, testy);
                //chessPieces[hitPosition.x, hitPosition.y].team = 1;

                // // Check if the piece exists and if it's a black piece
                // if (blackP != null && blackP.team == 1)

                if (blackP != null)
                {
                    if (blackP.team == 1) // Check it's a black piece just to be sure
                    {
                        // Move the test pawn
                        //MoveTo(blackP, hitPosition.x, hitPosition.y);

                        if (chessPieces[hitPosition.x, hitPosition.y] != null)
                        {
                            chessPieces[hitPosition.x, hitPosition.y].SetScale(Vector3.one * deathSize);
                            chessPieces[hitPosition.x, hitPosition.y].SetPosition(new Vector3(8 * tileSize, yOffset, -1 * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2) + (Vector3.forward * deathSpacing) * deadWhilesList.Count);
                        }


                        // Update chessPieces array with the new position of blackP
                        chessPieces[hitPosition.x, hitPosition.y] = blackP;

                        // Change scale and position outside chessboard
                        chessPieces[blackP.currentX, blackP.currentY] = null;

                        // Update blackP's current position
                        blackP.currentX = hitPosition.x;
                    }

                    // Apply additional logic if needed
                    Plane horizantalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
                    float distance = 0.0f;
                    if (horizantalPlane.Raycast(ray, out distance))
                    {
                        Vector3 testVector = new Vector3(testx, testy, yOffset);
                        blackP.SetPosition(testVector);
                    }
                }

                // Update the visual representation of the piece
                Vector3 newPosition = GetTileCenter(hitPosition.x, hitPosition.y);
                blackP.SetPosition(newPosition);
                currentMoveNumber++;
                isWhiteTurn = !isWhiteTurn;
                blackMoveReceived = false;

                //Debug.Log("Black turn is over wagmi");
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
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }

    // Remove hightlighted given tiles
    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
        availableMoves.Clear();
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
        if (!ContainsValidMove(ref availableMoves, new Vector2Int(x, y)))
        {
            Debug.Log("- - - - - - Contains invalid move! - - - - - -, " + chessPiece.team);
            return false;
        }

        Vector2Int previousPosition = new Vector2Int(chessPiece.currentX, chessPiece.currentY);

        // avoid placing pieces in same team positions
        if (chessPieces[x, y] != null)
        {
            ChessPiece otherChessPiece = chessPieces[x, y];

            if (chessPiece.team == otherChessPiece.team)
            {
                return false;
            }

            // avoid placing pieces in rival team positions
            // Avoid by 'killing' the rival
            if (otherChessPiece.team == 0)
            {
                // check mate - Black wins
                if (otherChessPiece.type == ChessPieceType.King)
                {
                    // display win-loss screen
                    CheckMate(1);
                }

                // add to dead list : white
                deadWhilesList.Add(otherChessPiece);

                // Change scale and position outside chessboard
                otherChessPiece.SetScale(Vector3.one * deathSize);
                otherChessPiece.SetPosition(new Vector3(8 * tileSize, yOffset, -1 * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2) + (Vector3.forward * deathSpacing) * deadWhilesList.Count);

            }

            // black
            else
            {
                // checkmate - white wins
                if (otherChessPiece.type == ChessPieceType.King)
                {
                    // display win-loss screen
                    CheckMate(0);
                }

                // add to dead list : white
                deadBlacksList.Add(otherChessPiece);

                // Change scale and position outside chessboard
                otherChessPiece.SetScale(Vector3.one * deathSize);
                otherChessPiece.SetPosition(new Vector3(-1 * tileSize, yOffset, 8 * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2) + (Vector3.back * deathSpacing) * deadBlacksList.Count);

            }
        }

        Debug.Log("- - - - - - - Got hereeeeeeee - - - - - -, " + chessPiece.team);

        // Auto move chess

        // - - - - - - - - - - - - Testing - - - - - - - - - 
        if (chessPieces[x, y] != null)
        {
            Debug.Log("- - - - - PLACING ON TOP - - - - -, " + chessPieces[x, y].team);
        }
        // - - - - - - - - - - - - Testing - - - - - - - - - 


        chessPieces[x, y] = chessPiece;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);

        //isWhiteTurn = !isWhiteTurn;
        currentMoveNumber++;

        moveList.Add(new Vector2Int[] { previousPosition, new Vector2Int(x, y) });

        processSpecialMove();

        // Make move through lichess
        // Retrieve the last move from moveList
        Vector2Int[] lastMove = moveList[moveList.Count - 1];

        // Convert the last move to UCI notation
        string uciMove = ConvertToUCI(lastMove);

        // Store the UCI notation in mostRecentUserMove
        mostRecentUserMove = uciMove;

        //yield return StartCoroutine(MakeMoveCoroutine(mostRecentUserMove));
        MakeMove(mostRecentUserMove);
        //MakeMoveCoroutine(mostRecentUserMove);

        //StartCoroutine(MakeMoveCoroutine(mostRecentUserMove));
        //Debug.Log("Whites most recent move: " + mostRecentUserMove);
        lichessMoveList.Add(mostRecentUserMove);

        StartCoroutine(BlackMoveCoroutine());


        //HaveWeReceivedBlackMove();

        isWhiteTurn = !isWhiteTurn;

        return true;

    }

    // Checkmate
    private void CheckMate(int team)
    {
        DisplayVictory(team);
    }

    private bool CheckForCheckmate()
    {
        var lastMove = moveList[moveList.Count - 1];
        int targetTeam = (chessPieces[lastMove[1].x, lastMove[1].y].team == 0) ? 1 : 0;

        // king reference
        List<ChessPiece> attackingPieces = new List<ChessPiece>();
        List<ChessPiece> defendingPieces = new List<ChessPiece>();

        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    if (chessPieces[x, y].team == targetTeam)
                    {
                        defendingPieces.Add(chessPieces[x, y]);

                        if (chessPieces[x, y].type == ChessPieceType.King)
                        {
                            targetKing = chessPieces[x, y];
                        }
                    }

                    else
                    {
                        attackingPieces.Add(chessPieces[x, y]);
                    }
                }
            }
        }

        // check if king is being attacked
        List<Vector2Int> currentAvailableMoves = new List<Vector2Int>();
        for (int i = 0; i < attackingPieces.Count; i++)
        {
            var pieceMoves = attackingPieces[i].GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);

            for (int b = 0; b < pieceMoves.Count; b++)
            {
                currentAvailableMoves.Add(pieceMoves[b]);
            }
        }

        // Check if king is in check
        if (ContainsValidMove(ref currentAvailableMoves, new Vector2Int(targetKing.currentX, targetKing.currentY)))
        {
            // if under attack, check for prevention
            for (int i = 0; i < defendingPieces.Count; i++)
            {
                List<Vector2Int> defendingMoves = defendingPieces[i].GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                SimulateMoveForSinglePiece(defendingPieces[i], ref defendingMoves, targetKing);

                // Check but not in checkmate
                if (defendingMoves.Count != 0)
                {
                    return false;
                }
            }

            // In checkmate
            return true;

        }

        return false;
    }

    // Special Moves
    private void PreventCheck()
    {
        // king reference
        ChessPiece targetKing = null;

        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    if (chessPieces[x, y].type == ChessPieceType.King)
                    {
                        if (chessPieces[x, y].team == currentDraggingPiece.team)
                        {
                            targetKing = chessPieces[x, y];
                        }
                    }
                }
            }
        }

        SimulateMoveForSinglePiece(currentDraggingPiece, ref availableMoves, targetKing);
    }

    private void SimulateMoveForSinglePiece(ChessPiece chessPiece, ref List<Vector2Int> moves, ChessPiece targetKing)
    {
        // Save current values
        int actualX = chessPiece.currentX;
        int actualY = chessPiece.currentY;
        List<Vector2Int> movesToRemove = new List<Vector2Int>();

        // Simulate moves and check checkmate
        for (int i = 0; i < moves.Count; i++)
        {
            int simulationX = moves[i].x;
            int simulationY = moves[i].y;

            Vector2Int kingPositionSimulation = new Vector2Int(targetKing.currentX, targetKing.currentY);

            // Simulated king move?
            if (chessPiece.type == ChessPieceType.King)
            {
                kingPositionSimulation = new Vector2Int(simulationX, simulationY);
            }

            // Simulation
            ChessPiece[,] simulation = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];
            List<ChessPiece> simAttackingPieces = new List<ChessPiece>();
            for (int x = 0; x < TILE_COUNT_X; x++)
            {
                for (int y = 0; y < TILE_COUNT_Y; y++)
                {
                    if (chessPieces[x, y] != null)
                    {
                        simulation[x, y] = chessPieces[x, y];
                        if (simulation[x, y].team != chessPiece.team)
                        {
                            simAttackingPieces.Add(simulation[x, y]);
                        }
                    }
                }
            }

            simulation[actualX, actualY] = null;
            chessPiece.currentX = simulationX;
            chessPiece.currentY = simulationY;
            simulation[simulationX, simulationY] = chessPiece;

            // Did one of the piece got taken down our simulation
            var deadPiece = simAttackingPieces.Find(c => c.currentX == simulationX && c.currentY == simulationY);

            if (deadPiece != null)
            {
                simAttackingPieces.Remove(deadPiece);
            }

            // Get simulated attacking pieces move
            List<Vector2Int> simMoves = new List<Vector2Int>();
            for (int a = 0; a < simAttackingPieces.Count; a++)
            {
                var pieceMoves = simAttackingPieces[a].GetAvailableMoves(ref simulation, TILE_COUNT_X, TILE_COUNT_Y);

                for (int b = 0; b < pieceMoves.Count; b++)
                {
                    simMoves.Add(pieceMoves[b]);
                }
            }

            // If King in target, remove the valid movement
            if (ContainsValidMove(ref simMoves, kingPositionSimulation))
            {
                movesToRemove.Add(moves[i]);
            }

            // Restore chess piece data
            chessPiece.currentX = actualX;
            chessPiece.currentY = actualY;
        }

        // Remove movements if in checkmate
        for (int i = 0; i < movesToRemove.Count; i++)
        {
            moves.Remove(movesToRemove[i]);
        }
    }

    private void processSpecialMove()
    {
        // En passant
        if (specialMove == SpecialMove.EnPassant)
        {
            var newMove = moveList[moveList.Count - 1];
            ChessPiece myPawn = chessPieces[newMove[1].x, newMove[1].y];
            var targetPawnPosition = moveList[moveList.Count - 2];
            ChessPiece enemyPawn = chessPieces[targetPawnPosition[1].x, targetPawnPosition[1].y];

            if (myPawn.currentX == enemyPawn.currentX)
            {
                if (myPawn.currentY == enemyPawn.currentY - 1 || myPawn.currentY == enemyPawn.currentY + 1)
                {
                    if (enemyPawn.team == 0)
                    {
                        // add to dead list : white
                        deadWhilesList.Add(enemyPawn);

                        // Change scale and position outside chessboard
                        enemyPawn.SetScale(Vector3.one * deathSize);
                        enemyPawn.SetPosition(new Vector3(8 * tileSize, yOffset, -1 * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2) + (Vector3.forward * deathSpacing) * deadWhilesList.Count);
                    }

                    else
                    {
                        // add to dead list : white
                        deadBlacksList.Add(enemyPawn);

                        // Change scale and position outside chessboard
                        enemyPawn.SetScale(Vector3.one * deathSize);
                        enemyPawn.SetPosition(new Vector3(8 * tileSize, yOffset, -1 * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2) + (Vector3.forward * deathSpacing) * deadBlacksList.Count);
                    }

                    // delete chesspiece reference
                    chessPieces[enemyPawn.currentX, enemyPawn.currentY] = null;
                }
            }
        }

        // Promotion
        if (specialMove == SpecialMove.Promotion)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            ChessPiece targetPawn = chessPieces[lastMove[1].x, lastMove[1].y];

            if (targetPawn.type == ChessPieceType.Pawn)
            {
                // white team
                if (targetPawn.team == 0 && lastMove[1].y == 7)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 0);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y, true);
                }

                // black team
                if (targetPawn.team == 1 && lastMove[1].y == 0)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 1);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y, true);
                }
            }
        }

        // Castling
        if (specialMove == SpecialMove.Castling)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];

            // Left rook
            if (lastMove[1].x == 2)
            {
                // white side
                if (lastMove[1].y == 0)
                {
                    ChessPiece rook = chessPieces[0, 0];
                    chessPieces[3, 0] = rook;
                    PositionSinglePiece(3, 0);
                    chessPieces[0, 0] = null;
                }

                // black side
                if (lastMove[1].y == 7)
                {
                    ChessPiece rook = chessPieces[0, 7];
                    chessPieces[3, 7] = rook;
                    PositionSinglePiece(3, 7);
                    chessPieces[0, 7] = null;
                }
            }

            // right rook
            if (lastMove[1].x == 6)
            {
                // white side
                if (lastMove[1].y == 0)
                {
                    ChessPiece rook = chessPieces[7, 0];
                    chessPieces[5, 0] = rook;
                    PositionSinglePiece(5, 0);
                    chessPieces[7, 0] = null;
                }

                // black side
                if (lastMove[1].y == 7)
                {
                    ChessPiece rook = chessPieces[7, 7];
                    chessPieces[5, 7] = rook;
                    PositionSinglePiece(5, 7);
                    chessPieces[7, 7] = null;
                }
            }
        }
    }

    // Diplay victory screen depending on winning team
    private void DisplayVictory(int winningTeam)
    {
        victoryScreen.SetActive(true);
        victoryScreen.transform.GetChild(winningTeam).gameObject.SetActive(true);
    }

    public void OnResetButton()
    {
        // Clean victory UI
        victoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(1).gameObject.SetActive(false);
        victoryScreen.SetActive(false);

        // Field reset
        currentDraggingPiece = null;
        availableMoves.Clear();
        moveList.Clear();

        // clean up chessboard and game objects
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    Destroy(chessPieces[x, y].gameObject);
                }

                chessPieces[x, y] = null;
            }
        }

        // clean dead pieces
        for (int i = 0; i < deadWhilesList.Count; i++)
        {
            Destroy(deadWhilesList[i].gameObject);
        }

        for (int i = 0; i < deadBlacksList.Count; i++)
        {
            Destroy(deadBlacksList[i].gameObject);
        }

        deadWhilesList.Clear();
        deadBlacksList.Clear();

        // Spawn all pieces and start again
        SpawnAllPieces();
        PositionAllPieces();
        isWhiteTurn = true;
    }

    public void OnExitButton()
    {
        Application.Quit();
    }

    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            if (moves[i].x == pos.x && moves[i].y == pos.y)
            {
                return true;
            }
        }

        return false;
    }

    // Lichess API Calls

    private IEnumerator StartNewGameCoroutine(int level)
    {
        // Create and send the UnityWebRequest
        string color = "white";
        WWWForm form = new WWWForm();
        form.AddField("level", level.ToString());
        form.AddField("color", color);
        form.AddField("clock.limit", "10800");
        form.AddField("clock.increment", "1");

        UnityWebRequest www = UnityWebRequest.Post("https://lichess.org/api/challenge/ai", form);
        www.SetRequestHeader("Authorization", "Bearer " + apiToken);

        // Send the request and wait for it to complete
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error while starting a new game: " + www.error);
        }
        else
        {
            Debug.Log("Game started: " + www.downloadHandler.text);

            // Parse the JSON response to extract the game ID
            string gameId = ParseGameId(www.downloadHandler.text);
            Debug.Log("Game ID: " + gameId);

            // Assign the game ID
            this.gameId = gameId;

            // Set the flag indicating that the game ID has been stored
            gameIdStored = true;
        }

        www.Dispose(); // Dispose the UnityWebRequest
    }

    private IEnumerator WaitForGameId()
    {
        // Wait until the game ID is stored
        while (!gameIdStored)
        {
            yield return null;
        }

        // Continue with the program
        Debug.Log("Game ID stored. Continuing with the program...");
    }

    private string ParseGameId(string jsonResponse)
    {
        // Deserialize the JSON string into GameIdData object
        GameIdData data = JsonUtility.FromJson<GameIdData>(jsonResponse);

        // Return the game ID
        return data.id;
    }

    [System.Serializable]
    public class GameIdData
    {
        public string id;
    }


    // make a move through lichess
    private IEnumerator MakeMoveCoroutine(string move)
    {
        WWWForm form = new WWWForm();
        string url = $"https://lichess.org/api/board/game/{gameId}/move/{move}";

        UnityWebRequest www = UnityWebRequest.Post(url, form);

        www.SetRequestHeader("Authorization", "Bearer " + apiToken);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error while making a move: " + www.error);
        }
        else
        {
            //Debug.Log("White Move made was: " + move);
        }
    }


    // Convert the last move to UCI notation
    private string ConvertToUCI(Vector2Int[] move)
    {
        // Convert the start position to algebraic notation
        string fromSquare = ConvertToAlgebraicNotation(move[0].x, move[0].y);
        // Convert the end position to algebraic notation
        string toSquare = ConvertToAlgebraicNotation(move[1].x, move[1].y);

        // Concatenate the start and end positions to form the UCI move
        string uciMove = fromSquare + toSquare;

        // Assign the UCI move to mostRecentUserMove directly
        mostRecentUserMove = uciMove;

        // Return the UCI move
        return uciMove;
    }
    // algebraic notation helper function
    private string ConvertToAlgebraicNotation(int x, int y)
    {
        char file = (char)('a' + x);
        int rank = y + 1;
        return $"{file}{rank}";
    }
    // Method to handle event streaming
    // private IEnumerator StreamEventsCoroutine()
    // {
    //     Debug.Log("StreamEvents coroutine started.");

    //     while (streaming)
    //     {
    //         string streamurl = $"https://lichess.org/api/board/game/stream/{gameId}";
    //         using (UnityWebRequest request = UnityWebRequest.Get(streamurl))
    //         {
    //             request.SetRequestHeader("Authorization", "Bearer " + apiToken);
    //             request.SetRequestHeader("Accept", "application/x-ndjson");

    //             yield return request.SendWebRequest();

    //             if (request.result == UnityWebRequest.Result.Success)
    //             {
    //                 using (StreamReader reader = new StreamReader(new MemoryStream(request.downloadHandler.data)))
    //                 {
    //                     string line;
    //                     while ((line = reader.ReadLine()) != null)
    //                     {
    //                         if (!string.IsNullOrEmpty(line))
    //                         {
    //                             //Received JSON data 53424: {"type":"gameState","moves":"g2g4","wtime":10800000,"btime":10800000,"winc":1000,"binc":1000,"status":"s
    //                             //onyl do the debug log if is of type gameState, example json in live above
    //                             if (IsGameState(line))
    //                             {
    //                                 Debug.Log("Received JSON data: " + line); // Log received JSON data
    //                                 string moves = ExtractMoves(line);
    //                                 Debug.Log("Moves: " + moves);
    //                             }
    //                         }
    //                     }
    //                 }
    //             }
    //             else
    //             {
    //                 Debug.LogError("Failed to connect to the event stream: " + request.error); // Log error message
    //             }
    //         }

    //         // Wait before reconnecting
    //         yield return new WaitForSeconds(5); // Adjust as needed
    //     }
    // }
    private IEnumerator StreamEventsCoroutine()
    {
        Debug.Log("Stream event coroutine started.");

        while (streaming)
        {
            string streamurl = $"https://lichess.org/api/board/game/stream/{gameId}";
            using (UnityWebRequest request = UnityWebRequest.Get(streamurl))
            {
                request.SetRequestHeader("Authorization", "Bearer " + apiToken);
                request.SetRequestHeader("Accept", "application/x-ndjson");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    //string[] lines = request.downloadHandler.text.Split('\n');
                    string[] lines = request.downloadHandler.text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            //Debug.Log("Received CS Event: " + line); // Log received JSON data
                            if (line.Contains("gameState"))
                            {
                                string moves = ExtractMoves(line);
                                List<string> res = new List<string>();

                                // Split moves and add them to res
                                string[] splitMoves = moves.Split(' ');
                                res.AddRange(splitMoves);

                                //lichessMoveList has the current moves ive stored

                                if (res.Count == lichessMoveList.Count + 1 && lichessMoveList.Count != 0)
                                {
                                    lichessMoveList.Add(res[res.Count - 1]);
                                    //Debug.Log("fuck this shit");
                                    blackMoveReceived = true;
                                }

                                // Split moves and add them to temp
                                //string[] splitMoves = moves.Split(' ');
                                //temp.AddRange(splitMoves);




                                //Debug.Log("Moves: " + moves);
                                //new list to store moves
                                // if(temp.count > lichessMoveList.length)
                                // {
                                //     lichessMoveList.Add(temp[temp.length-1]);
                                //     Debug.Log("Black's move received is: " + temp[temp.length-1]);
                                // }

                                // // Reverse temp to handle if the same move has occurred twice and find the most recent occurrence
                                // Array.Reverse(temp);

                                // if (temp[0] != mostRecentUserMove && temp[0] != "")
                                // {
                                //     //add the move to the lichessMoveList
                                //     lichessMoveList.Add(temp[0]);
                                //     Debug.Log("Black's move received is: " + temp[0]);
                                // }

                                // // Find the index of mostRecentUserMove
                                // int index = Array.IndexOf(temp, mostRecentUserMove);

                                // // Check if the index of mostRecentUserMove is the second-to-last index in the array
                                // if (index == temp.Length - 2)
                                // {
                                //     // second to last means that last is black
                                //     //blackMoveReceived = true;
                                //     //isWhiteTurn = false;
                                // }

                            }
                            //string tempmoves = ExtractMoves(line);
                            //Debug.Log("Moves: " + tempmoves);


                        }
                    }
                }
                else
                {
                    Debug.LogError("Failed to connect to the event stream on Chessboard: " + request.error); // Log error message
                }
            }

            // Wait before reconnecting
            yield return new WaitForSeconds(3); // Adjust as needed
        }
    }

    private bool IsGameState(string jsonData)
    {
        // Check if the JSON data contains "gameState" as the value of "type"
        int typeIndex = jsonData.IndexOf("\"type\"");
        if (typeIndex != -1)
        {
            int colonIndex = jsonData.IndexOf(':', typeIndex);
            int commaIndex = jsonData.IndexOf(',', colonIndex);

            if (colonIndex != -1 && commaIndex != -1)
            {
                string typeValue = jsonData.Substring(colonIndex + 1, commaIndex - colonIndex - 1).Trim(' ', '"');

                return typeValue == "gameState";
            }
        }

        return false;
    }
    //extract moves from json data
    public string ExtractMoves(string jsonData)
    {
        string moves = null;

        // Find the index of the "moves" key
        int movesIndex = jsonData.IndexOf("\"moves\"");

        if (movesIndex != -1)
        {
            // Find the start of the moves value
            int movesValueStart = jsonData.IndexOf(':', movesIndex) + 1;

            // Find the end of the moves value
            int movesValueEnd = jsonData.IndexOf(',', movesValueStart);

            // If a comma is not found, use the end of the string
            if (movesValueEnd == -1)
            {
                movesValueEnd = jsonData.Length - 1;
            }

            // Extract the moves substring
            moves = jsonData.Substring(movesValueStart, movesValueEnd - movesValueStart);

            // Remove surrounding quotes and trim any whitespace
            moves = moves.Trim('"', ' ').Replace("\\", string.Empty);
        }

        return moves;
    }

    private Vector2Int[] ConvertToVectorNotation(string uciMove)
    {
        // Split the UCI move into two parts: start square and end square
        string fromSquare = uciMove.Substring(0, 2);
        string toSquare = uciMove.Substring(2, 2);

        // Convert the start square from algebraic notation to vector notation
        Vector2Int startSquare = ConvertToVectorNotationHelper(fromSquare);
        // Convert the end square from algebraic notation to vector notation
        Vector2Int endSquare = ConvertToVectorNotationHelper(toSquare);

        // Return the move as a pair of Vector2Int representing start and end positions
        return new Vector2Int[] { startSquare, endSquare };
    }

    // Convert algebraic notation to vector notation helper function
    private Vector2Int ConvertToVectorNotationHelper(string algebraicNotation)
    {
        // Extract the file and rank from algebraic notation
        int x = algebraicNotation[0] - 'a';
        int y = algebraicNotation[1] - '1';

        // Return the vector notation
        return new Vector2Int(x, y);
    }

    private void MakeMove(string move)
    {
        StartCoroutine(MakeMoveCoroutine(move));
    }

    private void HaveWeReceivedBlackMove()
    {
        StartCoroutine(BlackMoveCoroutine());
        Debug.Log("WE FINALLY GOT IT WORKING");
    }

    private IEnumerator BlackMoveCoroutine()
    {
        // Wait for 6 seconds
        Debug.Log("in coroutine after waiting for 6 seconds");
        yield return new WaitForSeconds(6f);

        //Debug.Log("in coroutine after waiting for 6 seconds");
        // Continue with the rest of your logic...
    }


}