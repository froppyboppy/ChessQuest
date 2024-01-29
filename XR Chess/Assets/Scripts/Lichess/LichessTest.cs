using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LichessApiTest : MonoBehaviour
{
    private string apiToken = "lip_dzd4DLgEDWeNS8rqLYl9"; // Replace with your actual API token
    private string username = "Javs69"; // Replace with the username you want to query

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetUserProfile());
    }

    IEnumerator GetUserProfile()
    {
        string url = $"https://lichess.org/api/user/{username}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Add the authorization header with the token
            webRequest.SetRequestHeader("Authorization", $"Bearer {apiToken}");

            // Send the request and wait for the response
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {webRequest.error}");
            }
            else
            {
                // Parse the response
                Debug.Log($"User Profile: {webRequest.downloadHandler.text}");
            }
        }
    }
}
