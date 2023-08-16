using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//*** Event Brite USer Info data
[Serializable]
public class EventBriteUserInfo 
{
    public List<EventBriteUserEmail> emails;
    public string id;
    public string name;
    public string first_name;
    public string last_name;
    public bool is_public;
    public object image_id;
    public string appointmentTime;
}
