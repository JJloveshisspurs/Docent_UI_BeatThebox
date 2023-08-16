using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Sirenix.OdinInspector;
using SimpleJSON;
using System;
using Eventbrite;
using System.Linq;

public class EventBriteAPIManager : MonoBehaviour
{
    //*** The brief delay after scene initialization before beginning  first API Query
    public float attendeeListRequestInitialDelay = 5f;

    //*** How Frequently we request the Attendee list, I  would reccomend this between 2 and 3 minutes in a test the system handled roughly 4000 attendees
    public float attendeeListRequestInterval = 20f;

    //*** Private Token for Beat The Box Event, CRITICAL for authorization using all API Queries, use as oAuth Token if testing Queries
    private const string _PRIVATE_TOKEN = "G67DXMXIIUQPKOE5MBMM";

    //*** Organization ID code for Beat the Box event, used to pull multi-user Event ID's from the API query , hardcoded below in filtered Request urls
    private const string ORGANIZATION_ID = "483616920945";

   
    //*** Test ID To return specific User Data
    private const string ATTENDEE_TEST_ID = "6466110299";

    //*** Base EventBrite API request call
    private const string _BASE_REQUEST_URL = "https://www.eventbriteapi.com/v3/";

    //*** Test Query URL
    //https://www.eventbriteapi.com/v3/users/me/?token=G67DXMXIIUQPKOE5MBMM

    //*** Attendee List API Request
    //https://www.eventbriteapi.com/v3/events/[EVENT_ID/attendees/


    //*** IMPORTANT !!! Attendee List by Organization with DateTime filter ( April 1st, 2023 ) This is how we filter to all active event users post April 1st 2023
    string filteredRequest = "https://www.eventbriteapi.com/v3/organizations/483616920945/attendees/?continuation=[CONTINUATION_TOKEN]?status=attending&changed_since=2023-04-01T12%3A02%3A52Z";

    //*** Test Request I used to get user data
    string testExtendedDataRequest = "https://www.eventbriteapi.com/v3/organizations/483616920945/attendees/?continuation=[CONTINUATION_TOKEN]";

    //*** Field for Retrieved event  info , particularly used for extracting DateTime data
    [SerializeField] private EventBriteEventInfo eventInfo;

    //*** List of all event Attendees returned from API request, this is the list we cross-reference against when processing Barcodes
   [SerializeField] private EventBriteAttendeeList attendeeList;

    //*** This is where we process a specific Attendees data usually extracted by barcode
    public List<Attendee> currentAttendee;

    //*** Eventbrite API only returns 50 users per request so the continuation Token allows us to request the next page of attendees until the complete Query attendee list has been retrieved.
    private string continuationToken = "";

    //*** List constructed of all unique events we extract from the attendee List
    public List<EventsData> uniqueEventsInfo;

    //*** Hashset of multiple Event ID's within the beat the Box Series, used to not get duplicate event ID's but is used to get event unique info from each session for Beat The Box
    public HashSet<string> eventIDs;

    //*** Converted list of unique event ID's taken from Event ID HAshset
    public List<string> uniqueEventIds;

    //*** Final events list with all Unique Event ID values
    public List<string> finalEventIds;

    //*** Bool to "Lock" Queries for Event ID's and also to make sure our conuous query for new event times isn't interrupted by new requests but also continues if outstanding requests exists.
    public bool retrievingEventTimeData = false;

    //*** Index to help in continuously grabbing event times and iterate through  gradually populatinglist of event
    public int latestRetrievedEventIndex = 0;

    void Start()
    {
       
        //*** Request to contunuously pull event Brite data
        InvokeRepeating("RetrieveFreshAttendeeList", attendeeListRequestInitialDelay, attendeeListRequestInterval);
    }



    public void RetrieveFreshAttendeeList()
    {
        //*** End previous user data request
        StopCoroutine(UpdateAttendeeList(filteredRequest));

        //*** Clear user token data
        continuationToken = "";

        //*** Clear previous Attendee list data
        attendeeList = new EventBriteAttendeeList();

        //*** Begin new Reset Request
        StartCoroutine(UpdateAttendeeList(filteredRequest));
    }

    public void RetrieveAttendeeList()
    {
        //Debug.Log("Retrieving updated Attendee list!");
        StartCoroutine(UpdateAttendeeList(filteredRequest));
    }


  //*** Retrieve specific event Time value
    public void RetrieveEventTime(string pEventID)
    {
        //*** Check if Event Data Query is in place already
        if (retrievingEventTimeData == false)
        {
            
            //Debug.Log("Retrieving Event Time!");

            //*** Call to Retrieve Event time based off of Event ID value
            StartCoroutine(UpdateEventTime("https://www.eventbriteapi.com/v3/events/" + pEventID.ToString() + "/"));
        }
    }

    //*** Coroutine to Update list of Attendees
    IEnumerator UpdateAttendeeList(string uri)
    {
        //*** Check whether a Continuation Token was assigned in the last request, if not this is a fresh new request.
        if (continuationToken == "")
        {
            //*** Completely remove Continuation token paramaeter for request if no contination token exists
            uri = uri.Replace("?continuation=[CONTINUATION_TOKEN]", "");

            //Debug.Log("No Continuation token found request == " + uri);

        }
        else
        {
            //*** Replace continuation Token value
            uri = uri.Replace("[CONTINUATION_TOKEN]", continuationToken);

            //Debug.Log("No Continuation token found request == " + uri);

        }

        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            //*** Add Private Token to Request
            webRequest.SetRequestHeader("Authorization", "Bearer " + _PRIVATE_TOKEN);
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

                    //*** Reset continuation Token
                    continuationToken = "";
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);

                    //*** Reset continuation Token
                    continuationToken = "";
                    break;
                case UnityWebRequest.Result.Success:
                    //Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);

                    //*** Check if  we have a continuation token , if not we are performing a new request and reset our overall attendee list
                    if (continuationToken == "")
                    {

                        //Debug.LogWarning("Found list first page");

                        //*** Reset  and create new base Attendee list
                        attendeeList = JsonUtility.FromJson<EventBriteAttendeeList>(webRequest.downloadHandler.text);
                    }
                    else
                    {
                        //Debug.LogWarning("Extending Attendee List starting count ==" + attendeeList.attendees.Count.ToString());

                        //*** Query is an Attenee list extension so process latest attendees as an extension
                        EventBriteAttendeeList oExtendedAttendeeList = JsonUtility.FromJson<EventBriteAttendeeList>(webRequest.downloadHandler.text);

                        //*** Iterate through extended  attenees data and add extension list members to original attendee list to expand barcodes in requested user list
                        for(int i = 0; i < oExtendedAttendeeList.attendees.Count; i++)
                        {
                            //add extended list of users to existing list of users
                            attendeeList.attendees.Add(oExtendedAttendeeList.attendees[i]);
                        }

                        //Debug.Log("New Attendee list size == " + attendeeList.attendees.Count.ToString());

                    }

                    //*** If pages have more items call again and create extended attendee list
                    if (attendeeList.pagination.has_more_items)
                    {
                        //Debug.LogWarning("PAGE HAS MORE ITEMS, REQUESTING!!!");

                        //*** Add continuation token for future requests 
                        continuationToken = attendeeList.pagination.continuation;

                        //*** Call coroutine again with contnuation token assigned
                        RetrieveAttendeeList();

                    }
                    else
                    {
                        //Debug.LogWarning("End Of List Found resetting Continuation token!!!");
                        //if end of list reset continuation token
                        continuationToken = "";

                        //*** Create Events Data now that Request has finished for Player Data Times
                        CreateNewEventsData();
                    }

                   
                    break;
            }
        }
    }


   
    //*** Coroutine to grab specific event times for user and Player data construction 

    IEnumerator UpdateEventTime(string uri)
    {
        //*** Lock function for Request Event Times
        retrievingEventTimeData = true;

        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {

            webRequest.SetRequestHeader("Authorization", "Bearer " + _PRIVATE_TOKEN);
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

                    //*** Reset Request lock
                    retrievingEventTimeData = false;
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);

                    //*** Reset Request lock
                    retrievingEventTimeData = false;
                    break;
                case UnityWebRequest.Result.Success:
                   // Debug.Log(pages[page] + ":\nReceived EVentTime: " + webRequest.downloadHandler.text);

                    //*** Assign event info from  deserialized Request response
                    eventInfo = JsonUtility.FromJson<EventBriteEventInfo>(webRequest.downloadHandler.text);

                    //*** Pass Event Info in order to set event time values for later reference
                    UpdateEventTime(eventInfo);

                    //Debug.Log("Event time: " + GetFormattedEventTime());

                    //*** Event Has been retrieved , unlocking bool to allow for next request
                    retrievingEventTimeData = false;

                    //Debug.Log("Last Retrieved Event index == " + latestRetrievedEventIndex.ToString() + "Unique events i nfo count ==" +uniqueEventsInfo.Count.ToString());

                    //*** If not the last event ID checked for retrieval of info then continue checking next events for DateTimes in new requests.
                    if (latestRetrievedEventIndex < uniqueEventsInfo.Count -1)
                    {
                        //Debug.Log("Last Retrieved Event index == " + latestRetrievedEventIndex.ToString());
                        latestRetrievedEventIndex = latestRetrievedEventIndex + 1;

                        //*** Pass in next event index to search for new event date time
                        RetrieveEventTime(uniqueEventsInfo[latestRetrievedEventIndex].event_ID);
                    }



                    break;
            }
        }
    }

    //*** Test Attendee Retrieval
    [Button]
    public void TestRetrieveAttendeeList()
    {
        RetrieveAttendeeList();

    }


    //*** Take  Barcode data and Convert it to Player for Server Queries
    public EventBriteUserInfo ConvertAttendeDataToPlayer(string pBarcode)
    {
        //*** Initialize new Eventbrite User data with emails list
        EventBriteUserInfo oData = new EventBriteUserInfo();
        oData.emails = new List<EventBriteUserEmail>();

        //*** Add an mepty email object to help with assigning of email later
        EventBriteUserEmail oEmailObject = new EventBriteUserEmail();

        //*** Add Emails object to the list
        oData.emails.Add(oEmailObject);
       

        //*** LINQ Query  that retrieves user info from Attendee list based on passed Barcode value
        var theData = attendeeList.attendees.Where(data => data.barcodes[0].barcode.Equals(pBarcode)).ToList();

        //*** Assign Current Attendee based on query result
       


        //Debug.Log("the data ==" + theData.ToString());
        if (theData == null)
        {
            //Debug.Log(" COULD NOT FIND DATA!!!!");
        }
        else
        {
            //Debug.Log(" Found user with Barcode data!");

            //*** Assign Current Attendee based on query result
            currentAttendee = theData;
           // Debug.Log(" User count data == " +currentAttendee.Count.ToString() );

            //RetrieveEventTime(currentAttendee[0].event_id);

            //*** If Current Attendee has data assign retrieved data values to newly Initialized Player data Object
            if (currentAttendee.Count > 0)
            {
                oData.first_name = currentAttendee[0].profile.first_name;
                oData.last_name = currentAttendee[0].profile.last_name;
                oData.name = oData.first_name + " " + oData.last_name;
                oData.id = currentAttendee[0].barcodes[0].barcode.ToString();
                oData.emails[0].email = currentAttendee[0].profile.email;

                //*** Pass Event ID and see if we get a response from our existing Event Data list
                oData.appointmentTime = GetEventTime(currentAttendee[0].event_id);
            }
            else
            {
                //*** If No data found make sure we set data to null so that error menu shows
                oData = null;
            }
        }

        //*** Return newly constructed Player Data
        return oData;
    }

     

    //*** Create new event data
    public void CreateNewEventsData()
    {
        //*** Create Hashest of Strings to make sure we only have a unique set of Event ID's
        eventIDs = new HashSet<string>();

        //*** Iterate through all event attendees and amass our Event id collection
        for (int i = 0; i < attendeeList.attendees.Count; i++)
        {
            //*** Initilize new Event ID string
            string oEventsID = "";

            //*** Set String to current users event ID
           oEventsID = attendeeList.attendees[i].event_id;

            //*** Try to add the event ID to the list
            eventIDs.Add(oEventsID);
        }

        //Debug.Log("Events IDs Count :" + eventIDs.Count.ToString());

        //*** After grabbing all event ID's for hashset  let's convert hashset to list
        uniqueEventIds = eventIDs.ToList();

        //*** Let's iterate through Event ID's
        for(int x = 0; x < uniqueEventIds.Count; x++)
        {

            //Debug.Log("Events ID index :" + uniqueEventIds[x]);

            //*** Checking to confirm Event ID is not already in our Final Event ID data list
            var matchingIds = finalEventIds.FirstOrDefault(searchedID => searchedID.Contains(uniqueEventIds[x]));

            //*** If no matching ID exists
            if (matchingIds == null)
            {
                //Debug.Log("NEW ID : " + uniqueEventIds[x] + " ADDING TO THE LIST!");

                //*** Add new  Final event ID  string for this ID
                finalEventIds.Add(uniqueEventIds[x]);

                //*** Create new Event ID Object
                EventsData oData = new EventsData();

                //*** Set new objcts ID value  to the not found Event IDs value
                oData.event_ID = uniqueEventIds[x];

                //*** Add new event info object to list to later be asigned a time from the request
                uniqueEventsInfo.Add(oData);

                //*** Retrieve the event time based off of the event ID
                RetrieveEventTime(uniqueEventIds[x]);
            }
            else
            {

                //Debug.Log("Duplicate ID : " + uniqueEventIds[x] + " FOUND! TOSSING");


            }
        }

        

    }

    //*** Update Event time  based off of passed EventBrite info
    public void UpdateEventTime(EventBriteEventInfo pEventInfo)
    {

        //*** ITerate through assigned Unique Event Info objects
        for(int i = 0; i < uniqueEventsInfo.Count; i++)
        {
            //*** Check if we have an ID Match
           if( uniqueEventsInfo[i].event_ID == pEventInfo.id)
            {

                //Debug.Log("Updating Event INFO for Event" + pEventInfo.id.ToString());


                //Debug.Log("Time data output Local == " + pEventInfo.start.local.ToString());
                //Debug.Log("Time data output UTC == " + pEventInfo.start.utc.ToString());
                //Debug.Log("Time data output Timezone == " + pEventInfo.start.timezone.ToString());

                //*** split a string   an array based off of the Start time to seperate the Date out from the time
                string[] oStartTimeArray = pEventInfo.start.local.ToString().Split("T");

                //*** Remove the last digits of the start  time stamp which comes in a format of 00:00:00
                oStartTimeArray[1] = oStartTimeArray[1].Remove(5);

                //*** Assign trimmed start time value
                uniqueEventsInfo[i].event_TimeStart = oStartTimeArray[1];
                //uniqueEventsInfo[i].event_TimeEnd = pEventInfo.end.local.ToString().Replace("T", " "); ;



            }



        }
    }

    //*** Function for retrieving the Event time based on the Event ID of a given user
    public string GetEventTime(string pEventId)
    {
        //*** Initialize an event itme value
        string oEventTime = "";

        //*** Iterate through unique events objects
        for(int i = 0; i< uniqueEventsInfo.Count; i++)
        {
            //*** Check if event IDs match
            if (uniqueEventsInfo[i].event_ID == pEventId)
            {
                //*** Set As Event time value
                oEventTime = uniqueEventsInfo[i].event_TimeStart;


            }
        }
        //*** Return assigned event time
        return oEventTime;
    }

    //*** Test Value to retrieve specific ID's
    [SerializeField] string specificAttendeeBarcode = "646611029910526526189001";

    //*** Test Button to retrieve specific Attendee data
    [Button]
    public void TestGettingSpecificAttendeeData()
    {
        EventBriteUserInfo oUserInfo = new EventBriteUserInfo();
        
             oUserInfo = ConvertAttendeDataToPlayer(specificAttendeeBarcode);
        //Debug.Log("First name ==" + oUserInfo.first_name + " Last name : " + oUserInfo.last_name + " ID : " + oUserInfo.id + "email : "+ oUserInfo.emails[0].email);
    }
 }

//*** Events Data Object holding our Time Stat and End Values
[System.Serializable]
public class EventsData
{
    public string event_ID;
    public  string event_TimeStart;
    public string event_TimeEnd;

}