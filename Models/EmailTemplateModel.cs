using CosmosDBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IencircleAdmin.Models
{
    public class EmailTemplateModel
    {
        public Business newEvent { get; set; }
        public Business Business { get; set; }
        public string Telephone { get; set; }
        public string InviteAcceptPath { get; set; }
        public string FullName { get; set; }
        public string QrCode { get; set; }
        public string InvitePath { get; set; }
        public int NoOfInvites { get; set; }
        public Guest Guest { get; set; }
        public string Time { get; set; }
    }
}
