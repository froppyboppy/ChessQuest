using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
//using System.Text.Json;
//using System.Text.Json.Serialization;
using System.Collections.Generic;
using System;
//using System.Collections.Generic;
//using Unity.VisualScripting;
//using UnityEditor.ShaderGraph;
//using UnityEngine;
//using UnityEngine.UIElements;
//using UnityEngine.Networking;
//using System.Threading.Tasks;
//using System;
//using System.Collections;
//using Unity.VisualScripting;
//using UnityEditor.ShaderGraph;
//using UnityEngine;
//using UnityEngine.UIElements;
//using System.IO;
//using System.Net.Http;
//using System.Threading.Tasks;
//using System.Text.Json;
//using System.Threading;
//using System.Net.Http.Headers;
//using SimpleJSON;
public class EventHandler : MonoBehaviour
{
    private string gameId;
    private string apiToken;

    private bool streaming = true;

    public List<string> blackMoveList = new List<string>();

    public string moves;
    private void Start()
    {
        // Delay the execution of the Awake method by 3 seconds
        Invoke("DelayedAwake", 3f);
    }

    private void DelayedAwake()
    {
        // Assuming Chessboard script is attached to the same GameObject
        Chessboard chessboard = GetComponent<Chessboard>();
        if (chessboard != null)
        {
            gameId = chessboard.gameId;
            apiToken = chessboard.apiToken;
        }
        else
        {
            Debug.LogError("Chessboard script not found!");
        }
        
        // Start streaming events
        StartCoroutine(StreamEvents());
    }

    private IEnumerator StreamEvents()
    {
        Debug.Log("Stream event EVENT HANDLER coroutine started.");

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
                            Debug.Log("Received CS Event: " + line); // Log received JSON data
                            if (line.Contains("gameState"))
                            {
                                string moves = ExtractMoves(line);
                                List<string> res = new List<string>();

                                // Split moves and add them to res
                                string[] splitMoves = moves.Split(' ');
                                res.AddRange(splitMoves);
                                
                                //lichessMoveList has the current moves ive stored

                                if (res.Count == blackMoveList.Count+1 && blackMoveList.Count > 0)
                                {
                                    blackMoveList.Add(res[res.Count-1]);
                                    Debug.Log("fuck this shit events");
                                    //blackMoveReceived = true;
                                }

                                // Split moves and add them to temp
                                //string[] splitMoves = moves.Split(' ');
                                //temp.AddRange(splitMoves);

                                


                                Debug.Log("Moves: " + moves);
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



    // private IEnumerator StreamEvents()
    // {
    //     Debug.Log("Event streaming started.");

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
    //                 string[] lines = request.downloadHandler.text.Split('\n');

    //                 foreach (string line in lines)
    //                 {
    //                     if (!string.IsNullOrEmpty(line))
    //                     {
    //                         // Debug.Log("Received JSON data 53424: " + line); // Log received JSON data
    //                         // moves = ExtractMoves(line);
    //                         // Debug.Log("Moves: " + moves);
    //                         // string[] temp = moves.Split(' ');
    //                         // for (int i = 0; i < temp.Length; i++)
    //                         // {
    //                         //     if (i % 2 == 0)
    //                         //     {
    //                         //         Debug.Log("White move: " + temp[i]);
    //                         //     }
    //                         //     else
    //                         //     {
    //                         //         Debug.Log("Black move: " + temp[i]);
    //                         //         blackmoves.Add(temp[i]);
    //                         //     }
    //                         // }

    //                         // for (int i = 0; i < blackmoves.Count; i++)
    //                         // {
    //                         //     Debug.Log("BLACK MOCES Move: " + blackmoves[i]);
    //                         // }


                            
    //                     }
    //                 }
    //             }
    //             else
    //             {
    //                 //Debug.LogError("Failed to connect to the event stream on Event Handler: " + request.error); // Log error message
    //             }
    //         }

    //         // Wait before reconnecting
    //         yield return new WaitForSeconds(5); // Adjust as needed
    //     }
    // }

    // Additional methods to start and stop streaming if needed
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
}
