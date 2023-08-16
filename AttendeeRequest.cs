using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.Networking;
using Eventbrite;


public class AttendeeRequest : MonoBehaviour
{


    // UnityWebRequest.Get example

    // Access a website and use UnityWebRequest.Get to download a page.
    // Also try to download a non-existing page. Display the error.

    public EventBriteAttendeeList attendeeList;


    void Start()
    {
        // A correct website page.
        StartCoroutine(GetRequest("https://www.eventbriteapi.com/v3/events/622581828007/attendees/"));

        // A non-existing page.
        //StartCoroutine(GetRequest("https://error.html"));
    }

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {

                webRequest.SetRequestHeader("Authorization", "Bearer " + "");
                    //www.SetRequestHeader("Authorization", "Bearer " + token);
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);

                    attendeeList = JsonUtility.FromJson<EventBriteAttendeeList>(webRequest.downloadHandler.text);
                    break;
            }
        }
    }

}
