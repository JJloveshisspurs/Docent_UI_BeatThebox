using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoundInfoRenderer : MonoBehaviour
{
    public RoundStateInfo.roundCategory pCurrentRoundCategory;
    public TextMeshProUGUI roundLabel;
    public Image roundBackground;


    public void SetNewRoundDetails (RoundStateInfo pInfo)
        {

            pCurrentRoundCategory = pInfo.currentRoundType;
            roundLabel.text = pInfo.roundTitle;
            roundBackground.color = pInfo.roundLabelBackgroundColor;
        Debug.Log("Round info == " + pInfo.roundTitle);
    }
}
