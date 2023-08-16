using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Checkin_Manager : MonoBehaviour
{


    public QRCodeScanner_Controls _QR_Scanner_Controls;
    public TextMeshProUGUI timeLabel_Date;
    public TextMeshProUGUI timeLabel_Time;

    string dateTimeNowText;

    private float inputDelayTimer;
    private float inputDelayTimerInterval = .5f;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnEnable()
    {

        UpdateDateTime();
        OpenQRCodeScanner();
    }

    private void Update()
    {
        inputDelayTimer += Time.deltaTime;


        if (inputDelayTimer >= inputDelayTimerInterval)
        {
            inputDelayTimer = 0f;

            UpdateDateTime();
        }
    }

    public void OpenQRCodeScanner()
    {

        _QR_Scanner_Controls.OpenQRScanner();


    }

    public void CloseRCodeScanner()
    {

        _QR_Scanner_Controls.CloseQRScanner();


    }


    public void UpdateDateTime()
    {
        //*** Get Raw date time value
        dateTimeNowText = System.DateTime.Now.ToLocalTime().ToString();

        dateTimeNowText = System.DateTime.Now.ToString("hh:mm tt");
        timeLabel_Time.text = dateTimeNowText.Replace("P", "p");
        timeLabel_Time.text = dateTimeNowText.Replace("A", "a");
        timeLabel_Time.text = dateTimeNowText.Replace("M", "");

        timeLabel_Time.text = dateTimeNowText;
        timeLabel_Date.text = System.DateTime.Now.ToLongDateString();
    }
}
