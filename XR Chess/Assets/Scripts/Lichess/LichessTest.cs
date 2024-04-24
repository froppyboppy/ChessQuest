using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LichessApiTest : MonoBehaviour
{
    private string apiToken = "lip_mFklnRWDapFwCTqP1lDG"; // Replace with your actual API token
    private string username = "froppyboppy"; // Replace with the username you want to query

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetUserProfile());
        StartCoroutine(ChallengeBot(apiToken, 2, 300, 10)); // Example challenge with level 2, 5-minute game with 10-second increment
    }

    IEnumerator GetUserProfile()
    {
        string url = $"https://lichess.org/api/user/{username}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.SetRequestHeader("Authorization", $"Bearer {apiToken}");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error fetching user profile: {webRequest.error}");
            }
            else
            {
                Debug.Log($"User Profile: {webRequest.downloadHandler.text}");
            }
        }
    }

    IEnumerator ChallengeBot(string token, int level, int clockLimit, int clockIncrement)
    {
        string url = "https://lichess.org/api/challenge/ai";
        WWWForm form = new WWWForm();
        form.AddField("level", level);
        form.AddField("clock.limit", clockLimit);
        form.AddField("clock.increment", clockIncrement);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
        {
            webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error challenging bot: {webRequest.error}");
            }
            else
            {
                Debug.Log($"Bot Challenge Response: {webRequest.downloadHandler.text}");
            }
        }
    }
}
