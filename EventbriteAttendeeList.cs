using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//*** List of EventBrite Attendee data object
namespace Eventbrite
{
   [System.Serializable]
    public class Addresses
    {
    }

    [System.Serializable]
    public class Answer
    {
        public string answer;
        public string question;
        public string type;
        public string question_id;
    }

    [System.Serializable]
    public class Attendee
    {
        public Costs costs = new Costs();
        public string resource_uri;
        public string id;
        public DateTime changed = new DateTime();
        public DateTime created = new DateTime();
        public int quantity;
        public object variant_id = new object();
        public Profile profile = new Profile();
        public List<Barcode> barcodes = new List<Barcode>();
        public List<Answer> answers = new List<Answer>();
        public bool checked_in;
        public bool cancelled;
        public bool refunded;
        public string affiliate;
        public object guestlist_id = new object();
        public object invited_by = new object();
        public string status;
        public string ticket_class_name;
        public string delivery_method;
        public string event_id;
        public string order_id;
        public string ticket_class_id;
    }

    [System.Serializable]
    public class Barcode
    {
        public string status;
        public string barcode;
        public DateTime created = new DateTime();
        public DateTime changed = new DateTime();
        public int checkin_type;
        public bool is_printed;
    }

    [System.Serializable]
    public class BasePrice
    {
        public string display;
        public string currency;
        public int value;
        public string major_value;
    }

    [System.Serializable]
    public class Costs
    {
        public BasePrice base_price = new BasePrice();
        public EventbriteFee eventbrite_fee = new EventbriteFee();
        public Gross gross = new Gross();
        public PaymentFee payment_fee = new PaymentFee();
        public Tax tax = new Tax();
    }

    [System.Serializable]
    public class EventbriteFee
    {
        public string display;
        public string currency;
        public int value;
        public string major_value;
    }

    [System.Serializable]
    public class Gross
    {
        public string display;
        public string currency;
        public int value;
        public string major_value;
    }

    [System.Serializable]
    public class Pagination
    {
        public int object_count;
        public int page_number;
        public int page_size;
        public int page_count;
        public string continuation;
        public bool has_more_items;
    }

    [System.Serializable]
    public class PaymentFee
    {
        public string display;
        public string currency;
        public int value;
        public string major_value;
    }

    [System.Serializable]
    public class Profile
    {
        public string first_name;
        public string last_name;
        public string email;
        public string name;
        public Addresses addresses = new Addresses();
    }

    [System.Serializable]
    public class EventBriteAttendeeList
    {
        public Pagination pagination = new Pagination();
        public List<Attendee> attendees = new List<Attendee>();
    }

    [System.Serializable]
    public class Tax
    {
        public string display;
        public string currency;
        public int value;
        public string major_value;
    }








}
