using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class EventHandler : MonoBehaviour
{
    private string gameId;
    private string apiToken;

    private bool streaming = true;

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
        Debug.Log("Event streaming started.");

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
                    string[] lines = request.downloadHandler.text.Split('\n');

                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            Debug.Log("Received JSON data 53424: " + line); // Log received JSON data
                            // Process the received data as needed
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

    // Additional methods to start and stop streaming if needed
}
