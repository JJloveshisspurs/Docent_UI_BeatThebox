using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class ActiveGameInfoViewer : MonoBehaviour
{
    public int initialRoundtimeInSeconds;
    private int remainingRoundtimeInSeconds;

    private float roundtimevalue;

    public GameStateData currentGameState;

    public enum roundTimer
    {
        RoundStarted,
        RoundOver
    }

    public roundTimer currentRoundtimeState = roundTimer.RoundOver;

    public string systemTime;

    public string roundtime;

    public int roundcount;

    public string activeUserName;

    public string activeDeviceName;

    public TextMeshProUGUI userNameLabel;

    public TextMeshProUGUI deviceNameLabel;

    private int roundClockMinutes;

    private int roundClockSeconds;

    private float roundTimerTicksInterval = 1f;

    private float roundTimeTickCounter = 0f;

    private int gameScore = 0;
    public int miss_Count = 0;

    public TextMeshProUGUI roundTimerLabel;
    public TextMeshProUGUI gameStateLabel;
    public TextMeshProUGUI gameScoreLabel;


    public List<RoundStateInfo> roundStates;
    [SerializeField] private RoundInfoRenderer roundInfoViewer;

    [SerializeField] private GameStateData latestGameStateInfo;

    public bool beginGameStateInfoUpdate = false;
    private float gamestateInfoUpdatetimer;
    private float gamestateInfoUpdateInterval = 1f;


    public float gameStateInfoRetrieveInterval = 1.5f;
    public float gameStateInfoRetrieveTimer = 1.5f;


    private GameState lastGameState;
    // Start is called before the first frame update
    void Start()
    {
        InitializeTimer();
    }

    void Update()
    {

        gamestateInfoUpdatetimer += Time.deltaTime;
        gameStateInfoRetrieveTimer += Time.deltaTime;

        if(gamestateInfoUpdatetimer >= gamestateInfoUpdateInterval && beginGameStateInfoUpdate == true)
        {
            beginGameStateInfoUpdate = false;

            UpdateGameStateInfo();


        }


        if( gamestateInfoUpdatetimer >= gamestateInfoUpdateInterval)
        {
            gamestateInfoUpdatetimer = 0f;
            Docent_UI_Manager.instance.RequestGameState();

            
        }
            
        
    }

    public void InitializeTimer()
    {
        systemTime = System.DateTime.Now.ToString();


        ResetRoundTimer();
    }

    public void SetActiveUserInfo(PlayerData pPlayerData)
    {
        //Debug.Log(" Player First name == " + pPlayerData.firstname);
        if (pPlayerData != null && pPlayerData.firstname != null)
        {

            //*** I'm assuming some people won't wan to include their last name so in htose cases we only include  the first name 
            if (pPlayerData.lastname != null  )
            {
                if (pPlayerData.lastname.Length > 0)
                {
                    activeUserName = pPlayerData.firstname + " " + pPlayerData.lastname.Substring(0, 1) + ".";
                }
                else
                {
                    //*** enforce just first names if empty last name found
                    activeUserName = pPlayerData.firstname;
                }
            }
            else
            {
                activeUserName = pPlayerData.firstname;

            }
            activeDeviceName = pPlayerData.deviceId;

            userNameLabel.text = activeUserName;
            deviceNameLabel.text = activeDeviceName;


            //*** Initialize new Round Info
            //UpdateRoundDetails(RoundStateInfo.roundCategory.Round1);
        }

    }


    

    void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        roundTimerLabel.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void BeginRoundTimer()
    {

        currentRoundtimeState = roundTimer.RoundStarted;
        ResetRoundTimer();
        StartCoroutine(TickRoundclock());
    }

    public void RoundOver()
    {
        currentRoundtimeState = roundTimer.RoundOver;
        ResetRoundTimer();

        StopCoroutine(TickRoundclock());

    }

    public void ResetRoundTimer()
    {
        
        remainingRoundtimeInSeconds = initialRoundtimeInSeconds;

        DisplayTime(remainingRoundtimeInSeconds);
    }

    public IEnumerator TickRoundclock()
    {


        yield return new WaitForSeconds(1f);

        remainingRoundtimeInSeconds = remainingRoundtimeInSeconds - 1;

        if (remainingRoundtimeInSeconds <= 0)
        {
            RoundOver();
        }
        else
        {
            DisplayTime(remainingRoundtimeInSeconds);
            StartCoroutine(TickRoundclock());
        }
    }

    public void UpdateRoundDetails(RoundStateInfo.roundCategory pCategory)
    {
        switch (pCategory)
        {


            case RoundStateInfo.roundCategory.Round1:

                //roundInfoViewer.SetNewRoundDetails(roundStates[0]);
                break;

            case RoundStateInfo.roundCategory.Round2:

                //roundInfoViewer.SetNewRoundDetails(roundStates[1]);
                break;

            case RoundStateInfo.roundCategory.Round3:

                //roundInfoViewer.SetNewRoundDetails(roundStates[2]);
                break;

            default:

                break;



        }

    }


    public void SetLatestGameStateInfo(GameStateData pGameStateJSON)
    {
        //Debug.Log("Setting Game State Info!");
        latestGameStateInfo = pGameStateJSON;
        beginGameStateInfoUpdate = true;

    }

    public void UpdateGameStateInfo()
    {
        if(latestGameStateInfo != null)
        {
           //*** Check to make sure we Reset Player Info in between rounds
            if (latestGameStateInfo.gameState == "GameOver")
            {
                //*** Clear Round Info 
                //ClearRoundInfo();
                RenderGameStateOnly();
                return;
            }


            //*** Set New Round Info object
            RoundStateInfo oRoundInfo = new RoundStateInfo();
            oRoundInfo.roundTitle = "Round " + (latestGameStateInfo.round + 1).ToString();
            
            roundInfoViewer.SetNewRoundDetails(oRoundInfo);

            //*** Game score
            gameScore = latestGameStateInfo.score;

            //*** Assign  Device NAme
            activeDeviceName = latestGameStateInfo.activeDevice;

            
            //*** round time
            roundtime = Mathf.FloorToInt((float)latestGameStateInfo.gametime).ToString();

            //*** Create Player Data object
            PlayerData oPlayerData = new PlayerData();
            oPlayerData.firstname = latestGameStateInfo.player.firstname;
            oPlayerData.lastname = latestGameStateInfo.player.lastname;
            oPlayerData.confcode = latestGameStateInfo.player.confcode;
            oPlayerData.email = latestGameStateInfo.player.email;
            oPlayerData.score = latestGameStateInfo.player.score;
            
            //*** Set Active Player data based on new player object
            SetActiveUserInfo(oPlayerData);


            //*** Render Out Round Info based on data
            RenderRoundInfo(latestGameStateInfo.missed);

           
        }else
        {
            Debug.LogError("No game State Info set!");
            ClearRoundInfo();


        }
    }

    public void RenderGameStateOnly()
    {
        gameStateLabel.text = latestGameStateInfo.gameState;

        roundTimerLabel.text = "0:00";
        gameScoreLabel.text = "0";
        roundInfoViewer.roundLabel.text = "";
        userNameLabel.text = "";
        deviceNameLabel.text = "";
    }

    public void RenderRoundInfo( int pMissed)
    {
        
        roundTimerLabel.text = roundtime.ToString();

        gameStateLabel.text = latestGameStateInfo.gameState.ToString();
        Debug.Log(latestGameStateInfo.gameState.ToString());

        gameScoreLabel.text = gameScore.ToString();

        miss_Count = pMissed;
    }


    public void ClearRoundInfo()
    {
        roundTimerLabel.text = "0:00";

        gameStateLabel.text = "";

        gameScoreLabel.text = "0";

        roundInfoViewer.roundLabel.text = "";

        userNameLabel.text = "";
        deviceNameLabel.text = "";
    }
}
