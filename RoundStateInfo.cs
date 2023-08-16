using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoundStateInfo 
{
    public enum roundCategory
    {
        Round1,
        Round2,
        Round3
    }

    public roundCategory currentRoundType;

    public string roundTitle;

    public Color roundLabelBackgroundColor;
}
