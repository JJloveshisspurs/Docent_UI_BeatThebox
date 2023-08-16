using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using TMPro;
using RestServer;

public class Docent_UI_Manager : MonoBehaviour
{
    //*** Our Tentative max player count is 6 we use this to cap how many players are rendered in our UI Queue
    public int maxQueueSize = 6;

    public static Docent_UI_Manager instance;

  
    //*** Data for the current Players in Queue
    public List<PlayerData> currentPlayersQueueData;

    //*** This is a list of th Previous Queue , used to track changes between new and old data
    public List<PlayerData> previousPlayersQueueData;


   //*** Game Object that acts as the center for the Player Queue Info Renderers
    public GameObject playerInfoGridParent;

    //*** Prefabs for Player Queue Info Renderers objects
    public GameObject PlayerInfoPrefab;

    //*** Reference for the box that displays Player Data, Game State, round info, round time, and more
    public ActiveGameInfoViewer gameInfoviewer;

    //*** List of actively spawned player button objects, this list is used to destroy  and manage player data item buttons
    private List<Player_QueuePosition_Renderer> activePlayerButtons = new List<Player_QueuePosition_Renderer>();

    //*** Currently assigned Target Player data
    private PlayerData targetPlayerData;

    //*** Index corresponding to the actively select Target player object in a list
    private int targetPlayerDataIndex = -1;

    //*** Text String for handling manipulation of Time info
    string dateTimeNowText;

    //*** These control how frequently we request the time data , right now we shoot to update our time every 3 seconds so the time is never too far off.
    private float DatetimeUpdateDelayTimer;
    private float DateTimeDelayTimerInterval = 3f;

    //*** Time labels corresponding to the Local Date and Time info
    public TextMeshProUGUI timeLabel_Date;
    public TextMeshProUGUI timeLabel_Time;

   
    //*** Was having errors updating UI elements directly after a server response was received, so we use this bool to trigger a refresh in LateUpdate
    private bool refreshGrid;

    //*** contains the last selected player conf code to track active player selection so even if there are two player tickets with the same name we track selection  by confcode
    private string lastSelectedDataButtonConfCode;

    //*** Grid position values for Player queue data grid
    [Header("Grid Position Values")]
    public float xStartPosition;
    public float yStartPosition;

    //*** How Spaced out should the grid items be?
    public float yPositionOffsetIncrement;

    

    //*** Device List scriptable object reference
    public DeviceList deviceList;

    //*** List of Device info items by name , used to populate the PlayerQueueData Item dropdowns
    private List<string> deviceInfoList;

    //*** Reference to EventBrite API Manager used to pull in player data and manage QR Code Scanning
    public EventBriteAPIManager eventbriteAPIManager;

    /***************************
    //**Unity functions --
    //**************************
    //**************************
    */

    private void Awake()
    {
        DeviceList oDevice = new DeviceList();

        if(deviceList!= null)
        {

            //Debug.Log("Device List EXISTS!!!");

           //*** Generate a list of Devices which the player dropdowns will reference
            GenerateDeviceList();
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        //*** Create docentUI Manager singleton instance
        if (instance == null)
            instance = this;

        //*** Begin Querying server
          InitializeServerQueries();

        //*** Update Initial Date Time
        UpdateDateTime();

        //*** Reset Playe info
        ResetRoundAndPlayerInfo();
    }

    void OnEnable()
    {
        //*** Update DateTime immediately
        UpdateDateTime();
        
    }

    private void Update()
    {


        //*** Timer to ocntinually update the Date Time request
        DatetimeUpdateDelayTimer += Time.deltaTime;


        if (DatetimeUpdateDelayTimer >= DateTimeDelayTimerInterval)
        {
            DatetimeUpdateDelayTimer = 0f;

            UpdateDateTime();
        }
    }

    void LateUpdate()
    {

        //*** Refresh the grid for player Data after the latest request has been processed
        if (refreshGrid)
        {
            refreshGrid = false;

            InstantiateUserDataPrefabs();
        }
    }

    /***************************
   //** UI Focused functions -- 
   //**************************
   //**************************
   */

    //*** Function that processes latest retrieved player queue
    public void SetLatestQueueData(List<PlayerData> pPlayerData)
    {

        //*** Create new Queue data for players
        currentPlayersQueueData = new List<PlayerData>();

        //*** Update Queued Data
        currentPlayersQueueData = pPlayerData;

        //*** 
        bool oListHasUpdated = ListHasUpdated();

        if (oListHasUpdated == true)
        {
            //*** Refresh Data Player Prefab list
            RefreshPlayerDataPrefabs();

            //*** Reset Previous Player Queue list
            previousPlayersQueueData = new List<PlayerData>();

            //*** Create  list of  previous  playerData 
            for (int i = 0; i < currentPlayersQueueData.Count; i++)
            {
                //*** Take list and store as previous list data to tests against future list queries
                previousPlayersQueueData.Add(currentPlayersQueueData[i]);
            }
        }
        else
        {

           // Debug.Log("List Has Not Updated!");

        }
    }


    public void UpdateLastSelectedPlayerDataButton(string pSelectedConfCode)
    {
        //*** Mark the last selected Data button based on confcode
        lastSelectedDataButtonConfCode = pSelectedConfCode;


    }

    //*** Function to  mark Player Data prefabs for Refresh  
    public void RefreshPlayerDataPrefabs()
    {
        refreshGrid = true;
    }

    private string prevFirstUserNameCode ="";
    private string prevLastUserNamecode ="";
    private int lastUpdatedQueueSize = 0;

    public void InstantiateUserDataPrefabs(bool pForceRefresh = false)
    {
        //*** Clear Existing Player data objects to prevent unwanted duplicates
        ClearActivePlayerDataPrefabs();

        //Debug.Log("Creating new player data items  count == " + currentPlayersQueueData.Count.ToString());
        for (int i = 0; i < currentPlayersQueueData.Count; i++)
        {
            //*** Way to make sure that queue size is capped at (6 by default) individuals by default visually 
            if (i < maxQueueSize)
            {
                //*** From here we instantiate player data items in our Player queue list

                //*** First we Instantiate a ne player Queue Info Renderer object prefab instance and parent it to the PlayerInfroGrid Parent object
                GameObject oPlayerInfoPrefab = Instantiate(PlayerInfoPrefab, playerInfoGridParent.transform);

                //*** We get the Player Queue Position renderer item class
                Player_QueuePosition_Renderer oRenderer = oPlayerInfoPrefab.GetComponent<Player_QueuePosition_Renderer>();

                //*** We Set the  current users data and assign an index value to the button so it knows it's order/ position in the list
                oRenderer.SetUserData(currentPlayersQueueData[i], i);

                //*** We add it to the running list of Active Player button items
                activePlayerButtons.Add(oRenderer);

                //*** We Get the Rect Transform for the instantiated player object for future position manipulations
                RectTransform oRectTransform = oPlayerInfoPrefab.GetComponent<RectTransform>();

                //*** We Set the Y position based off the initial Y position + the current instantiated buttons position index times it's offset in yStartPos = 10, i = 2, yPositionOffset Increment = 50 we end up with
                //so that we get something like yPosition at index 2  =  110  =  10 + (2 *50) and y position at index 3 = 160  = 10 +(3 *50) placing each button with a 50 unit offset in a grid
                float newYPosition = yStartPosition + (i * (int)yPositionOffsetIncrement);

                //Debug.Log("new Y Position == " + newYPosition.ToString());

                //*** Set Items position with new Y offset in mind
                oRectTransform.localPosition = new Vector3(xStartPosition, newYPosition, oRectTransform.localPosition.z);

                //*** If last selected data item  has matching confcode then select item
                if (lastSelectedDataButtonConfCode == currentPlayersQueueData[i].confcode)
                {
                    //*** MArk player button object as selected
                    oRenderer.SelectPlayer();
                }
            }


            //*** If Last element store both username + confcode and last username +confcode to determine if a list update should occur
            if ( i == currentPlayersQueueData.Count - 1)
            {
                prevFirstUserNameCode = currentPlayersQueueData[0].firstname + currentPlayersQueueData[0].lastname + currentPlayersQueueData[0].confcode;
                prevLastUserNamecode = currentPlayersQueueData[currentPlayersQueueData.Count - 1].firstname + currentPlayersQueueData[currentPlayersQueueData.Count - 1].lastname + currentPlayersQueueData[currentPlayersQueueData.Count - 1].confcode;
                lastUpdatedQueueSize = currentPlayersQueueData.Count;
            }
        }


    }

    public void SubmitTargetPlayerData()
    {
        //*** Check if Target Player data exists
        if (targetPlayerData != null)
        {
            //*** Send Target Player Data with assigned conf code to the server
            POSTRequestActivatePlayer(targetPlayerData.confcode);
        }
    }

    public void RemoveTargetPlayerData()
    {
        //Debug.Log(" Target player index == " + targetPlayerDataIndex.ToString() + " and current player queue data ==" + currentPlayersQueueData.Count.ToString());

        //*** Make sure we don't try removing target data where no data exists
        if (currentPlayersQueueData.Count <= 0 || targetPlayerDataIndex == -1)
            return;


           //*** Request to remove data of user at TargetPlayerdata Index position
            POSTRequestRemoveUser(currentPlayersQueueData[targetPlayerDataIndex]);

            //*** Reset value to prevent trying to remove nonexistent data
            targetPlayerDataIndex = -1;    
    }

    public void ResetSelectionList()
    {
        //*** ITerate through al lactive player buttons
        for (int i = 0; i < activePlayerButtons.Count; i++)
        {
            //*** Deselect All active Player button items
            activePlayerButtons[i].DeselectPlayer();
        }
    }

    //*** Begin prepping data for player to be selected as active player
    public void PrepActivePlayer(PlayerData pPlayerData, int pIndex)
    {
        //*** Update target player values
        targetPlayerDataIndex = pIndex;
        targetPlayerData = pPlayerData;
    }

    public void ClearActivePlayerDataPrefabs()
    {
        //*** Function to destroy pre-existing Active Player Data Prefabs
        for (int i = 0; i < activePlayerButtons.Count; i++)
        {
            if (activePlayerButtons[i].gameObject != null)
                Destroy(activePlayerButtons[i].gameObject);
        }

        //reset list of active player buttons
        activePlayerButtons = new List<Player_QueuePosition_Renderer>();
    }

    public void UpdateDateTime()
    {
        //*** Get Raw date time value
        dateTimeNowText = System.DateTime.Now.ToLocalTime().ToString("hh:mm tt");

        //*** Clean string for formatting
        dateTimeNowText = dateTimeNowText.Replace("P", "p");
        dateTimeNowText = dateTimeNowText.Replace("A", "a");

        //*** Set time Label values
        timeLabel_Time.text = dateTimeNowText.Replace("M","");

        //*** Set Date label values
        timeLabel_Date.text = System.DateTime.Now.ToLongDateString();
    }

    public void SubmitManualConfirmationCode(string pManualConfCode)
    {
        //Debug.Log("Manual confirmation Code ==" + pManualConfCode);

        //*** Send Post Request with Manual confcode
        POSTRequestActivatePlayer(pManualConfCode);
    }

    public void RefreshPlayerQueueGrid(float pbuttonCurrentYPosition, GameObject pDraggedGameObject, int pLastPositionIndex)
    {

        //Debug.Log("Refreshing Player Queue!!!  current Y position: " + pbuttonCurrentYPosition.ToString());

        //***Create local variable for new Position set
        int oNewButtonPositionIndex = 0;

        //*** Get Position for first object in list
        float oFirstQueueButtonPosition = yStartPosition + yPositionOffsetIncrement;

        //Debug.Log("First Queue Item position Y == " + oFirstQueueButtonPosition.ToString());

        //*** Get Position for LAst object in list
        float oLastQueueButtonPosition = (yStartPosition + ((activePlayerButtons.Count - 1) * (int)yPositionOffsetIncrement)) + yPositionOffsetIncrement;

        //Debug.Log("Last Queue Item position Y == " + oLastQueueButtonPosition.ToString());

        //*** Refreshing Grid

        //*** Get a percentage corresponding to the objects position within the full expanse of the menu items
        float oDraggedPositionMultiplier = pbuttonCurrentYPosition / (oLastQueueButtonPosition + yPositionOffsetIncrement);

        //Debug.Log("oDragged Position Multiplier == " + oDraggedPositionMultiplier.ToString());

        //*** Set Button position to snap into position based off of position multiplier multiplied by the number of buttons on the list and round it to come up wih the new position index for a button
        oNewButtonPositionIndex = Mathf.CeilToInt(oDraggedPositionMultiplier * activePlayerButtons.Count);

        //*** Mke sureour index is never belod 0
        if (oNewButtonPositionIndex <= 0)
            oNewButtonPositionIndex = 0;

        //* Maker usre our index never exceeds the number of Active player buttons in the list
        if (oNewButtonPositionIndex >= activePlayerButtons.Count)
            oNewButtonPositionIndex = activePlayerButtons.Count - 1;

        //Debug.Log(" New Button Position index == " + oNewButtonPositionIndex);

        //***  Reorganize Grid with new values
        ReorganizeGrid(oNewButtonPositionIndex, pDraggedGameObject, pLastPositionIndex);
    }

    public void ReorganizeGrid(int pNewButtonPositionIndex, GameObject pDraggedObject, int pLastPositionIndex)
    {
        //*** Temporary local player data
        PlayerData oTempData;

        //*** Set up temporary reference for old data in position
        oTempData = currentPlayersQueueData[pNewButtonPositionIndex];

        //*** Move newly dragged object data into position correct button position from it's last known spot
        currentPlayersQueueData[pNewButtonPositionIndex] = currentPlayersQueueData[pLastPositionIndex];

        //*** Move Old Dragged object data in new position of old dragged item
        currentPlayersQueueData[pLastPositionIndex] = oTempData; ;

        //*** MAke a put request to update player List
        Docent_UI_Manager.instance.PUTRequestUpdateList(currentPlayersQueueData);

        //***Get reference to old object in new dragged items position
        InstantiateUserDataPrefabs(true);
    }

    public void GenerateDeviceList()
    {
        //*** Create Device info lsit based off of Device List scriptable object
        deviceInfoList = deviceList.devices.Keys.ToList();
    }

    //*** Function to provide Player Queue items with device info
    public List<string> GetDeviceList()
    {
        return deviceInfoList;
    }

    public bool ListHasUpdated()
    {
        //Debug.Log("CHECKING IF LIST HAS UPDATED!!!");

        //*** We assume no changes have happened in the list
        bool oListHasChanged = false;

        //*** Whether a user was removed or added some aspect of the lsit has changed count and needs to be updated;
        if (previousPlayersQueueData.Count != currentPlayersQueueData.Count)
        {
            //Debug.Log("List Counts don't match , List has updated");

           //*** mark list as changed
            oListHasChanged = true;

            return oListHasChanged;
        }

        //*** If Data counts match let's check if the same matching conf codes are detected
        int oMatchingPlayerDataCount = 0;

        //*** ITerate through list of the previous player data and new player data  to see whether all the items match in terms of conf code
        for(int i = 0; i < previousPlayersQueueData.Count; i++)
        {
            for (int x = 0; x < currentPlayersQueueData.Count; x++)
            {
                if(previousPlayersQueueData[i].confcode == currentPlayersQueueData[x].confcode)
                {
                    oMatchingPlayerDataCount = oMatchingPlayerDataCount + 1;
                }
            }
        }

        //*** If the same number of matching conf codes are found in both the new and old list then no data has changed, no need to update list
        //*** Otherwise new confcode found , please update list
        if (oMatchingPlayerDataCount != currentPlayersQueueData.Count || oMatchingPlayerDataCount != previousPlayersQueueData.Count)
        {
            //Debug.Log("Confcode counts do not mach , player data has changed");
            oListHasChanged = true;
        }

            return oListHasChanged;
    }

   

    /// <summary>
    /// *** Server Request properties, variables, and functions
    /// </summary>

    [Tooltip("This is a GET Method!")]
    public string test_URL = "/test";

    [Tooltip("This is a POST Method!")]
    public string userArrived_URL = "/userArrived";

    [Tooltip("This is a GET Method!")]
    public string getQueue_URL = "/getQueue";

    [Tooltip("This is a POST Method!")]
    public string removeUser_URL = "/removeUser";

    [Tooltip("This is a PUT Method!")]
    public string updateList_URL = "/updateList";

    [Tooltip("This is a PUT Method!")]
    public string assignDevice_URL = "/assignDevice";

    [Tooltip("This is a POST Method!")]
    public string activate_URL = "/activateUser";

    [Tooltip("This is a GET Method!")]
    public string getGameState_URL = "/getGameState";

    [Header("Rest Query Properties")]

    [Tooltip("How many seconds to wait before beginning server querues after loading scene")]
    public float queryStartDelay = 5f;

    public float playerQueueRefreshInitializationDelay = 10f;

    [Tooltip("How frequently we can refresh the Player Queue created using prefabs")]
    public float playerQueueRefreshInterval = 10f;

   //*** Server details
    [Header("Rest Server Address")]
    [SerializeField] private string address = "localhost";
    [SerializeField] private string port = "8080";
    public string ServerAddress { get { return $"http://{address}:{port}"; } }


    public void InitializeServerQueries()
    {
        //*** Initialize Queries for requesting player data with slight delay  and do it at the Referh Interval
        InvokeRepeating("RequestExistingPlayerQueueData", playerQueueRefreshInitializationDelay, playerQueueRefreshInterval);

    }

    //*** Player Activation Server Functions
    public void POSTRequestActivatePlayer(string pConfcode)
    {
        //*** Initialize empty conf code
        string confcode = "";

        //*** Create conf Code JSON and assign it to object
        var confCodeJson = new ConfCodeJSON();
        confCodeJson.confcode = pConfcode;

        //*** Convert Json with new confcode value and submit data to the server
        string json = JsonUtility.ToJson(confCodeJson);

        //*** Submit player Activated
        StartCoroutine(ServerUtilities.PostJson(ServerAddress + activate_URL, json));
    }

    //*** FUNCTION IS INVOKED DO NOT CHANGE NAME OR DELETE!!!!
    public void RequestExistingPlayerQueueData()
    {
        StartCoroutine(GETRequestExistingPlayerQueueData());
    }


    //*** Player Queue data  Server  requestFunction
    IEnumerator GETRequestExistingPlayerQueueData()
    {
 
        using (UnityWebRequest webRequest = UnityWebRequest.Get(ServerAddress + getQueue_URL))
        {

            //webRequest.SetRequestHeader("Authorization", "Bearer " + _PRIVATE_TOKEN);
            //www.SetRequestHeader("Authorization", "Bearer " + token);
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            //string[] pages = uri.Split('/');
            //int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    //Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    //Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    //Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);

                   
                    //*** Deserialize list into USer Data object
                    UserDataList oUserList = JsonUtility.FromJson<UserDataList>(webRequest.downloadHandler.text);

                    //string json = JsonUtility.ToJson(userListJson);
                    //Debug.Log("Deserialized Players count  == " + oUserList.users.Count.ToString());

                    //*** Create new local list of player data
                    List<PlayerData> oPlayerData = new List<PlayerData>();

                    for (int i = 0; i < oUserList.users.Count; i++)
                    {
                        //*** Set and create new player data
                        PlayerData oPlayer = new PlayerData();

                        oPlayer.firstname = oUserList.users[i].firstname;
                        oPlayer.lastname = oUserList.users[i].lastname;
                        oPlayer.appointmentTime = oUserList.users[i].appointmentTime;
                        oPlayer.deviceId = oUserList.users[i].deviceId;
                        oPlayer.confcode = oUserList.users[i].confcode;
                        oPlayer.email = oUserList.users[i].email;

                        oPlayerData.Add(oPlayer);
                    }
                    //Debug.Log("player data count ==" + oPlayerData.Count);
                    //*** Set Player list to match returned player list
                    SetLatestQueueData(oPlayerData);
                    break;
            }
        }


    }


    //*** Remove User Server Function
    public void POSTRequestRemoveUser(PlayerData pPlayerData)
    {
        //*** Assign conf code to local data object
        string oConfcode = pPlayerData.confcode;

        //*** Create new confcode json object
        ConfCodeJSON confCodeJson = new ConfCodeJSON();

        //set confcode in data item
        confCodeJson.confcode = oConfcode;

        //*** Serailize confcode json object into JSON
        string json = JsonUtility.ToJson(confCodeJson);

        //*** Request user data with corresponding ocnfcode be removed
        StartCoroutine(ServerUtilities.PostJson(ServerAddress + removeUser_URL, json));
    }


    //*** Test Server Response

    public void GETRequestTestResponse()
    {
        Debug.Log("GETTING TEST REQUEST!");
        UnityWebRequest.Get(ServerAddress + test_URL).SendWebRequest();
    }


    public void PUTRequestUpdateList(List<PlayerData> pPlayerData)
    {
        //*** Create new UserInfoJSon object for user list
        var userList = new List<UserInfoJSON>();

        //*** Add all existing Player Data items to the user list
        foreach (PlayerData player in pPlayerData)
        {
            userList.Add(ServerUtilities.UserInfoFromPlayerData(player));
        }

        //*** Create USer List JSON object
        var userListJson = new UserListJSON();

        //*** Assign users as user list
        userListJson.users = userList;

        //*** Convert User ListJSON to json string
        string json = JsonUtility.ToJson(userListJson);

        //*** Make Server Update request with constrcted user info list
        StartCoroutine(ServerUtilities.PutJson(ServerAddress + updateList_URL, json));
    }


    public void POSTRequestUserArrived(EventBriteUserInfo pEventBriteUserInfo)
    {
        //*** Create User Info  JSON item and populate it based on all of the EventBrite USer Info
        var userInfo = new UserInfoJSON();
        userInfo.firstname = pEventBriteUserInfo.first_name;
        userInfo.lastname = pEventBriteUserInfo.last_name;
        userInfo.email = pEventBriteUserInfo.emails[0].email;
        userInfo.confcode = pEventBriteUserInfo.id;
        userInfo.appointmentTime = pEventBriteUserInfo.appointmentTime;
        
        //*** Serialize userinfo into Jsson string
        string json = JsonUtility.ToJson(userInfo);

        //*** Post User Arrived with User Info JSON
        StartCoroutine(ServerUtilities.PostJson(ServerAddress + userArrived_URL, json));
    }
    //*** Altenative functon to Post That a user has arrived with Player data instead 
    public void POSTRequestUserArrived(PlayerData pData)
    {
        //Create new USer Info JSON
        var userInfo = new UserInfoJSON();
        userInfo.firstname = pData.firstname;
        userInfo.lastname = pData.lastname;
        userInfo.email = pData.email;
        userInfo.confcode = pData.confcode;

        //*** Serialize info into user info
        string json = JsonUtility.ToJson(userInfo);

        //*** Post USer Arrived JSON
        StartCoroutine(ServerUtilities.PostJson(ServerAddress + userArrived_URL, json));
    }

    public void POSTRequestAssignDevice(string pConfCode, string pDeviceID)
    {
        //*** Create new Assigned Device JSon info
        var assignDeviceJson = new AssignDeviceJSON();

        //*** Set Device info in Json based off of passed parameters and get device UID from device List Scriptable object
        assignDeviceJson.confcode = pConfCode;
        assignDeviceJson.deviceId = deviceList.GetDeviceUid(pDeviceID);

        //*** Serialize AssignedDeviceJSON into JSON string
        string json = JsonUtility.ToJson(assignDeviceJson);

        //*** Post Assign Device URL server request
        StartCoroutine(ServerUtilities.PostJson(ServerAddress + assignDevice_URL, json));

    }

    //*** Request the Game State
    public void RequestGameState()
    {

        StartCoroutine(GETRequestGetGameState());

    }

    IEnumerator GETRequestGetGameState()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(ServerAddress + getGameState_URL))
        {

            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            //string[] pages = uri.Split('/');
            //int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    //Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    //Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:

                    //Debug.Log("Game Stat Json:" + webRequest.downloadHandler.text);

                    //*** Create new Game State Inf object
                    var gameStateInfo = new GameStateData();

                    //*** Deserialize Game State Info request into Game State Info class
                    gameStateInfo = JsonUtility.FromJson<GameStateData>(webRequest.downloadHandler.text);

                    //*** Set Game State Info
                    gameInfoviewer.SetLatestGameStateInfo(gameStateInfo);
                    break;

            }
        }
    }

    //*** Function to Post New User Info to the Server
    public void PostSubmitNewUserInfoToServer(PlayerData pNewPlayerData)
    {

        UserInfoJSON oJson = ServerUtilities.UserInfoFromPlayerData(pNewPlayerData);

        ServerUtilities.PostJson(userArrived_URL, oJson.ToString()); ;
    }

    //*** This function resets the player info 
    public void ResetRoundAndPlayerInfo()
    {
        gameInfoviewer.ClearRoundInfo();
    }
}

/// <summary>
/// *** Temp Data Class for managingUser Data
/// </summary>
[System.Serializable]
public class UserDataList
{
    public List<PlayerData> users;
}
