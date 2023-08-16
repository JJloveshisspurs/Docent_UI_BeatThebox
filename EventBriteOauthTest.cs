using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class EventBriteOauthTest : MonoBehaviour
{


    private const string _API_KEY = "Nunya";
    private const string _CLIENT_SECRET = "Nunya2";
    private const string _PRIVATE_TOKEN = "Nunya3";
    private const string _PUBLIC_TOKEN = "Nunya4";
    private const string _REDIRECT__URI = "Nunya5";

    //*** Event ID code for Beat the Box
    private const string EVENT_ID = "62258182800723";
    private const string ATTENDEE_TEST_ID = "6466110299";


    private const string _BASE_REQUEST_URL = "https://www.eventbriteapi.com/v3/";

    private const string _AUTHENTICATION_URL = "/users/me/?token=MYTOKEN";

    private const string _REDIRECT_URI = "https://www.eventbrite.com/oauth/authorize?response_type=code&client_id=YOUR_API_KEY&redirect_uri=YOUR_REDIRECT_URI";

    private const string ATTENDEE_REQUEST_URL = "events/{event_id}/attendees/{attendee_id}";

    private string formattedString = "";















    string id = "";
    DateTime from = new DateTime();
    DateTime to = new DateTime();

    

    public void InitGetMeasurements()
    {

        StartCoroutine(GetMeasurements(id, from, to, (measurementResult) =>
        {
            string measurement = measurementResult;

            //Do something with measurement
            UnityEngine.Debug.Log(measurement);

        }));




    }


    

    private  IEnumerator GetAccessToken(Action<string> result)
    {
        Dictionary<string, string> content = new Dictionary<string, string>();
        //Fill key and value
        content.Add("grant_type", "client_credentials");
        content.Add("client_id", "login-secret");
        content.Add("client_secret", "secretpassword");

        UnityWebRequest www = UnityWebRequest.Post("https://someurl.com//oauth/token", content);
        //Send request
        yield return www.Send();

        if (!www.isNetworkError)
        {
            string resultContent = www.downloadHandler.text;
            TokenClassName json = JsonUtility.FromJson<TokenClassName>(resultContent);

            //Return result
            result(json.access_token);
        }
        else
        {
            //Return null
            result("");
        }
    }

  

    private  IEnumerator GetMeasurements(string id, DateTime from, DateTime to, Action<string> result)
    {
        Dictionary<string, string> content = new Dictionary<string, string>();
        //Fill key and value
        content.Add("MeasurePoints", id);
        content.Add("Sampling", "Auto");
        content.Add("From", from.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        content.Add("To", to.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        content.Add("client_secret", "secretpassword");

        UnityWebRequest www = UnityWebRequest.Post("https://someurl.com/api/v2/Measurements", content);

        string token = null;

        yield return GetAccessToken((tokenResult) => { token = tokenResult; });

        www.SetRequestHeader("Authorization", "Bearer " + token);
        www.Send();

        if (!www.isNetworkError)
        {
            string resultContent = www.downloadHandler.text;
            MeasurementClassName rootArray = JsonUtility.FromJson<MeasurementClassName>(resultContent);

            string measurements = "";
           /* foreach (MeasurementClassName item in rootArray)
            {
                measurements = item.Measurements;
            }*/

            //Return result
            result(measurements);
        }
        else
        {
            //Return null
            result("");
        }
    }


}

[Serializable]
public class MeasurementClassName
{
    public string Measurements;
}

[Serializable]
public class TokenClassName
{
    public string access_token;
}
