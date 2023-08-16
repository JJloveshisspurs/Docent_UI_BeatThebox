using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Player_QueuePosition_Renderer : MonoBehaviour
{
    //Player Name Plate and ID attributes
    private string player_AppointmentTime;
    private string player_Name;
    private string player_assignedDevice;

    //*** Labels and Drop down for for rendering player Info on the Button panell
    public TextMeshProUGUI time_Label;
    public TextMeshProUGUI name_Label;
    public TextMeshProUGUI assigned_Device_Label;
    public TMP_Dropdown deviceDropDown;
    public TextMeshProUGUI device_dropdown_Label;

    //*** Reference to button object
    public Button button;

    //*** Inactive color and active color to rese button states
    public Color inactiveColor;
    public Color activeColor;

    //*** Player data objects
    private PlayerData currentData;

    //*** Index of the Player Data Item
    private int PlayerDataIndex;

   
    public void Start()
    {
        //*** Initialize and create a local Device Drop down basedo n the geneated Device info list in the central Docent UI
        CreateDeviceDropDownList(Docent_UI_Manager.instance.GetDeviceList());
    }

    public void SetUserData(PlayerData oPlayerData, int pPlayerDataIndex)
    {
        //*** Set Current data 
        currentData = oPlayerData;
        player_AppointmentTime = currentData.appointmentTime;
        player_Name = oPlayerData.firstname +" " + oPlayerData.lastname.Substring(0,1)+".";
        player_assignedDevice = currentData.deviceId;
        PlayerDataIndex = pPlayerDataIndex;

       //*** Set Text label values
        time_Label.text = oPlayerData.appointmentTime;
        name_Label.text = player_Name;
        assigned_Device_Label.text = player_assignedDevice;

        //*** Assign event to change device dropdown and its values
        deviceDropDown.onValueChanged.AddListener(delegate {
            UpdatedDeviceDropdownValue(deviceDropDown);
        });

    }

    public void SelectPlayer()
    {
        
        if(Docent_UI_Manager.instance == null)
        {
            Debug.LogError("DOCENT UI MANAGER IS MISSING , WILL CAUSE NULLS!!!!");
            return;
        }
        //*** Reset Which button is selected
        Docent_UI_Manager.instance.ResetSelectionList();

        //Debug.Log("Selected This Player : " + player_Name);

       // Swap button to active color
        button.image.color = activeColor;

        //*** Prep active player
        Docent_UI_Manager.instance.PrepActivePlayer(currentData,PlayerDataIndex);

        //*** Updateplaye button based on checking confcode
        Docent_UI_Manager.instance.UpdateLastSelectedPlayerDataButton(currentData.confcode);

    }

    public void DeselectPlayer()
    {
        //Debug.Log("Deselected This Player : " + player_Name);

        //*** Reset button to inactive color
        button.image.color = inactiveColor;
    }

    public void UpdatedDeviceDropdownValue(TMP_Dropdown change)
    {
        //*** Update assigned device values 
        player_assignedDevice = change.captionText.text;
        currentData.deviceId = player_assignedDevice;
        //Debug.Log("User :" + player_Name + " Updated evice Info ==" + player_assignedDevice);

        //*** Select current button
        SelectPlayer();
    }

    public void OnDestroy()
    {
        //*** Remove Device Dropdown listeners
        deviceDropDown.onValueChanged.RemoveListener(delegate {
            UpdatedDeviceDropdownValue(deviceDropDown);
        });
    }

    public void RefreshGridPosition()
    {
        if (Docent_UI_Manager.instance == null)
        {
            Debug.LogError("DOCENT UI MANAGER IS MISSING , WILL CAUSE NULLS!!!!");

            return;
        }
       
        //Refresh position
        //Debug.Log("Refreshing Grid Position!");

        //*** Refresh Player Queue and submitcurrent items position and index
        Docent_UI_Manager.instance.RefreshPlayerQueueGrid(this.gameObject.transform.localPosition.y,this.gameObject,PlayerDataIndex);
    }

    public void CreateDeviceDropDownList(List<string> pDeviceOptions)
    {
        //*** Create a list of items from the Docent UI drop down info
        deviceDropDown.AddOptions(pDeviceOptions);

        //*** Set all of the Device info items as new Dropdown list items
        for(int i = 0; i < deviceDropDown.options.Count; i++)
        {
            //*** Default to empty device option from drop down
            if (deviceDropDown.options[i].text == "") ;
            deviceDropDown.SetValueWithoutNotify(i);
        }

    }

    //*** Submit data for player Device change
    public void SubmitPlayerDeviceChange()
    {
        //Debug.Log("Seleted drop down device == " + device_dropdown_Label.text);

        currentData.deviceId = device_dropdown_Label.text;

        if (currentData.confcode == null)
        {

            Debug.LogError("CONF CODE IS NULL! not submitting device change!");
            return;
        }

        if(currentData.deviceId == null)
        {
            Debug.LogError("Device ID IS NULL! not submitting device change!");
            return;
        }


        //Debug.Log("Assigning user Device ID : " + currentData.deviceId + " Conf Code" + currentData.confcode);
        //*** Make a Post Request submitting the current user confcode and Device ID info
        Docent_UI_Manager.instance.POSTRequestAssignDevice(currentData.confcode,currentData.deviceId);


    }
}
