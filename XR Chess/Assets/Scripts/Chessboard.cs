using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Text.Json;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using SimpleJSON;
public enum SpecialMove
{
    None = 0,
    EnPassant = 1,
    Castling = 2,
    Promotion = 3
}

public enum GameState
{
    WaitingForWhiteMove,
    WaitingForBlackMove
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
    private string mostRecentUserMove;

    //lichess move list
    private List<string> lichessMoveList = new List<string>();

    // Lichess API account info
    public string apiToken = "lip_mFklnRWDapFwCTqP1lDG"; // juan's api token
    public string gameId; // game id
    private string username = "froppyboppy"; // username
    private bool receivedFirstNDJSON = false; // flag for first NDJSON message

    private Coroutine streamingCoroutine;
    private bool streaming = true;

    // team turn
    private bool isWhiteTurn;
    private bool setupAndStreamingCompleted = false;

    // Main function running whole script
    private async void Awake()
    {
        //StartNewGame();
        //isWhiteTurn = true;
        Debug.Log("Game started once");

        // Generates tiles layer
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);

        // Spawn all pieces
        SpawnAllPieces();

        // Position pieces
        PositionAllPieces();

        // Start event streaming in the background
        StartCoroutine(GameSetupAndStreamEvents());
        isWhiteTurn = true;
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

        // Check if it's white's turn
        if (isWhiteTurn)
        {
            // Process white player's move
            ProcessWhiteMove();
            //Debug.Log("White move happened");
            isWhiteTurn = false;

        }
        else
        {
            // Process black player's move or bot's move
            //Debug.Log("Black move or bot's move happened");
            ProcessBlackMove();
            isWhiteTurn = true;
        }
    }

    // Method to process white player's move
    private void ProcessWhiteMove()
    {
        // Mouse input as ray with 100 unit range
        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);

        // Mouse (Ray) is on board 
        // Allowed layers to position pieces
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
                    if ((chessPieces[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn))
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

            // Mouse releasing with a chess piece
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
                RemoveHighlightTiles();
                //isWhiteTurn = false; // After white's move, set isWhiteTurn to false
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

    // Method to process black player's move or bot's move
    private void ProcessBlackMove()
    {
        // Implement the logic for black's move or bot's move here
        //Debug.Log("Black move or bot's move happened");
        //wait for 5 seconds

        // After processing black's move, set isWhiteTurn to true
        isWhiteTurn = true;
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

        chessPieces[x, y] = chessPiece;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);

        isWhiteTurn = !isWhiteTurn;
        moveList.Add(new Vector2Int[] { previousPosition, new Vector2Int(x, y) });

        processSpecialMove();

        if (CheckForCheckmate())
        {
            CheckMate(chessPiece.team);
        }

        // Retrieve the last move from moveList
        Vector2Int[] lastMove = moveList[moveList.Count - 1];

        // Convert the last move to UCI notation
        string uciMove = ConvertToUCI(lastMove);

        // Store the UCI notation in mostRecentUserMove
        mostRecentUserMove = uciMove;

        MakeMove(mostRecentUserMove);
        Debug.Log("Whites most recent move: " + mostRecentUserMove);
        lichessMoveList.Add(mostRecentUserMove);
        
        //GetGameInfo();

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

    private void MakeMove(string move)
    {
        StartCoroutine(MakeMoveCoroutine(move));
    }
    /**
     * Start the game by making an API call to Lichess to challenge the bot
     */
    private IEnumerator StartNewGameCoroutine(int level, string color = "white")
    {
        WWWForm form = new WWWForm();
        form.AddField("level", level.ToString());  // Level can be adjusted but for now, we'll keep it at 1
        form.AddField("color", color);  // Specify the color
        form.AddField("clock.limit", "10800");  // Add clock limit
        form.AddField("clock.increment", "1");  // Add clock increment
        UnityWebRequest www = UnityWebRequest.Post("https://lichess.org/api/challenge/ai", form);
        www.SetRequestHeader("Authorization", "Bearer " + apiToken);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error while starting a new game: " + www.error);
        }
        else
        {
            Debug.Log("Game started, lets go: " + www.downloadHandler.text);
            
            // Parse the JSON response to extract the game ID
            string gameId = ParseGameId(www.downloadHandler.text);
            Debug.Log("Game ID, lets go: " + gameId);
            
            // You can assign the game ID to a class variable if needed
            this.gameId = gameId;
        }
        yield return new WaitForSeconds(1f); // Adjust as needed
    }
    // Define classes to deserialize JSON response
    [System.Serializable]
    public class State
    {
        public string type;
        public string moves;
        public int wtime;
        public int btime;
        public int winc;
        public int binc;
        public string status;
    }

    [System.Serializable]
    public class RootObject
    {
        public string id;
        public State state;
    }


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
            Debug.Log("White Move made was: " + move);
        }
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
    public class MostRecentUserMove
    {
        public string move;
    }

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

    private string ConvertToAlgebraicNotation(int x, int y)
    {
        char file = (char)('a' + x);
        int rank = y + 1;
        return $"{file}{rank}";
    }

    private IEnumerator GameSetupAndStreamEvents()
    {
        int default_level = 1;  // Default level is 1
        // Start a new game and wait for it to complete
        yield return StartCoroutine(StartNewGameCoroutine(default_level));

        // Start event streaming in the background
        streamingCoroutine = StartCoroutine(StreamEvents());
    }

    // Method to handle event streaming
    private IEnumerator StreamEvents()
    {
        Debug.Log("StreamEvents coroutine started.");

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
                    using (StreamReader reader = new StreamReader(new MemoryStream(request.downloadHandler.data)))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (!string.IsNullOrEmpty(line))
                            {
                                Debug.Log("Received JSON data: " + line); // Log received JSON data
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("Failed to connect to the event stream: " + request.error); // Log error message
                }
            }

            // Wait before reconnecting
            yield return new WaitForSeconds(5); // Adjust as needed
        }
    }


    private void OnDestroy()
    {
        // Stop the coroutine when the object is destroyed
        if (streamingCoroutine != null)
        {
            StopCoroutine(streamingCoroutine);
        }
    }

}
