using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ManualEntryDialogue : MonoBehaviour
{
    //*** Input field where the confirmation code is entered
    public TMP_InputField confirmationCodeField;

    //*** Confirmation code value
    public string confirmationCode;

    //*** Reference to Confirmation menu
    public ConfirmationCodeMenu confMenu;

    //*** Object for missing data for the user
    public GameObject missingUsererrorLabelObject;


    public void OnEnable()
    {

        missingUsererrorLabelObject.SetActive(false);
    }

    //*** Set latest scanned info
    public void SetLatestQRScannedData(string pQRScannerInfo)
    {

        confirmationCodeField.text = pQRScannerInfo;

        confirmationCode = pQRScannerInfo;
    }

    //*** Refresh Confirmation code to default text after confirmation
    public void RefreshConfirmationCodeEntry()
    {
        confirmationCode = confirmationCodeField.text;


    }

    //*** Add code for manual entry
    public void AddManualEntrycode()
    {


        //*** We take the text in the ocnfirmation code input field and use it to create a new Event Brite user info object for rendering out the data
        EventBriteUserInfo oUser = Docent_UI_Manager.instance.eventbriteAPIManager.ConvertAttendeDataToPlayer(confirmationCodeField.text);

        //*** If for osme reason the data returns a null then show there error fields and render error label text
        if (oUser == null)
        {
            Debug.Log("Error USer not found!");
            oUser = new EventBriteUserInfo();

            oUser.first_name = "error";
            oUser.last_name = "error";
            //oUser.emails[0].email = "error";
            missingUsererrorLabelObject.SetActive(true);
            return;
        }

        //*** Set confirmation code in confirmation menu
        confMenu.SetConfCodText(oUser);

        //*** Refresh Confirmation code value to reset this menu
        RefreshConfirmationCodeEntry();

        //*** Grab Menu State Controller and Set state to confirmation Menu
        MenuStateController oMenustateController = GameObject.FindObjectOfType<MenuStateController>();

        if (oMenustateController != null)
            oMenustateController.ActivateMenuState(MenuState.menuStateType.ConfirmationMenu);
    }






    //*** Function to Submit Confirmation Code Data
    public void SubmitConfirmationCode()
    {
        //*** Reset Confirmation code input label
        RefreshConfirmationCodeEntry();


        if(Docent_UI_Manager.instance != null)
        {
            //***submit manual confirmation code
            Docent_UI_Manager.instance.SubmitManualConfirmationCode(confirmationCode);

        }
        else
        {

            Debug.LogError("DOCENTUI  MANAGER INSTANCE IS MISSING!!!!");
        }
    }
}
