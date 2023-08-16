using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class QRCodeScanner_Controls : MonoBehaviour
{
    //*** Reference to QR Code Scnaner plugin
    public QRCodeDecodeController qrCodeDecodeController;

    public TextMeshProUGUI qrScanDebugLabel;

    public GameObject scanner_Contents;


    public List<GameObject> objectsToHideAndReveal;

    //*** Reference to menu state controller  which switches between the vaiours menu states DocentUI Main , QR code, ConfCode confimation, and Manual Entry
    public MenuStateController menuController;

    //*** Reference to Confirmation codem enu
    public ConfirmationCodeMenu confCodeMenu;

    //*** Reference to the menu that handles manual input for Barcodes
    public ManualEntryDialogue manualEntryDialogue;

    public Image queueMenuBackground;
    public Image checkInMenuBackground;

    public void Start()
    {
        //*** Initialize Scanner 
        InitScanner();
    }

    public void InitScanner()
    {
        //*** Assign Scanner callback event  which processes QR Scanner result
        qrCodeDecodeController.onQRScanFinished += OnQRScanResultRecieived;
    }


    public void OpenQRScanner()
    {

        //*** Activate the QR Scanner camera , hide  associated Menu State objects
        scanner_Contents.SetActive(true);

        //*** Hide irrelevant menu objects
        RevealOrHidEMenuObjects(false);


        //*** Reset Scnaner functionality to reinitiate code
        qrCodeDecodeController.Reset();
    }

    public void CloseQRScanner()
    {
        //qrCodeDecodeController.StopWork();

        //*** Deactivate QR Scnaner game objects
        scanner_Contents.SetActive(false);

        //*** Activate any assigned menu objects
        RevealOrHidEMenuObjects(true);
    }

    //**** This is hwere the Scanner Result is received, in terms of the EventBrite code it's where we pullthe Barcode value which is then cross referenced with our EventBrite API query results
    public void OnQRScanResultRecieived(string pResult)
    {
        //Debug.Log("QR Scan Result ==" + pResult);

        qrScanDebugLabel.text = "Scan Result : " +  pResult;

        //*** Disable Scnaner Functionality
        CloseQRScanner();

        //*** Set Menu State To Confirmation Menu to Render Barcode data
        menuController.ActivateMenuState(MenuState.menuStateType.ConfirmationMenu);


        //*** Take retrieved Barcode Data and Convert to a player using our Barcode as a LINQ query parameter
        EventBriteUserInfo oUser = Docent_UI_Manager.instance.eventbriteAPIManager.ConvertAttendeDataToPlayer(pResult);

        //*** If User not found , report scan reuslt and exit
        if(oUser == null)
        {
            Debug.Log("Error USer not found!");
            oUser = new EventBriteUserInfo();

            oUser.first_name = "error";
            oUser.last_name = "error";
            oUser.emails[0].email = "error";
        }

        // Set Retrieved  User  info in confcode Menu
        confCodeMenu.SetConfCodText(oUser);

        //*** Set QR Scanner data using Barcode ID
        manualEntryDialogue.SetLatestQRScannedData(oUser.id);
    }

    //*** Function to hide and reveal relevant menu objects
    public void RevealOrHidEMenuObjects(bool pRevealObjects)
    {
        queueMenuBackground.enabled = pRevealObjects;
        checkInMenuBackground.enabled = pRevealObjects;
        
        //*** Reveal or hide all objects 
        for (int i = 0; i < objectsToHideAndReveal.Count; i++)
        {
            objectsToHideAndReveal[i].SetActive(pRevealObjects);
        }

        //*** Reset Menu state to Checkin / QR Codestate
        menuController.ActivateMenuState(MenuState.menuStateType.Manual_Checkin_Entry);
    }
}
