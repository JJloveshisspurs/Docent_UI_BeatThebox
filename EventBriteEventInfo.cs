using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Eventbrite
{
    //*** EventBrite info data object

    [System.Serializable]
    public class EventBriteEventInfo
    {
        public Name name = new Name();
        public Description description = new Description();
        public string url;
        public Start start = new Start();
        public End end = new End();
        public string organization_id;
        public string created;
        public string changed;
        public string published;
        public int capacity;
        public bool capacity_is_custom;
        public string status;
        public string currency;
        public bool listed;
        public bool shareable;
        public bool invite_only;
        public bool online_event;
        public bool show_remaining;
        public int tx_time_limit;
        public bool hide_start_date;
        public bool hide_end_date;
        public string locale;
        public bool is_locked;
        public string privacy_setting;
        public bool is_series;
        public bool is_series_parent;
        public string inventory_type;
        public bool is_reserved_seating;
        public bool show_pick_a_seat;
        public bool show_seatmap_thumbnail;
        public bool show_colors_in_seatmap_thumbnail;
        public string source;
        public bool is_free;
        public object version;
        public string summary;
        public object facebook_event_id;
        public string logo_id;
        public string organizer_id;
        public string venue_id;
        public string category_id;
        public string subcategory_id;
        public string format_id;
        public string id;
        public string resource_uri;
        public bool is_externally_ticketed;
        public string series_id;
        public Logo logo = new Logo();
    }

    [System.Serializable]
    public class CropMask
    {
        public TopLeft top_left;
        public int width;
        public int height;
    }

    [System.Serializable]
    public class Description
    {
        public string text;
        public string html;
    }

    [System.Serializable]
    public class End
    {
        public string timezone;
        public string local;
        public string utc;
    }

    [System.Serializable]
    public class Logo
    {
        public CropMask crop_mask = new CropMask();
        public Original original = new Original();
        public string id;
        public string url;
        public string aspect_ratio;
        public string edge_color;
        public bool edge_color_set;
    }

    [System.Serializable]
    public class Name
    {
        public string text;
        public string html;
    }

    [System.Serializable]
    public class Original
    {
        public string url;
        public int width;
        public int height;
    }


    [System.Serializable]
    public class Start
    {
        public string timezone;
        public string local;
        public string utc;
    }

    [System.Serializable]
    public class TopLeft
    {
        public int x;
        public int y;
    }
}