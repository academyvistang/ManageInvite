using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IencircleAdmin.Models
{
    public class Item : Entity
    {
        public Item() : base("Item")
        {
            Email = "";
            Fullname = "";
            Telephone = "";
        }
        [JsonProperty("fullname")]
        public string Fullname { get; set; }

        [JsonProperty("telephone")]
        public string Telephone { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }


        [JsonProperty("noofinvites")]
        public int NoOfInvites { get; set; }

        [JsonProperty("invitesent")]
        public bool InviteSent { get; set; }

        public string QrCode { get; set; }


    }
}
