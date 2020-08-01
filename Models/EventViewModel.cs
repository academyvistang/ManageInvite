using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IencircleAdmin.Models
{
    public class EventViewModel
    {
        public string Fullname { get; set; }
        public string Address { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string ColourCode { get; set; }
        public string NoOfInvitess { get; set; }
        public string ColourCodeDescription { get; set; }
        public string SeatingArea { get; set; }
        public string BusinessLogo { get; set; }
        public bool? InviteAcceped { get; set; }
        public string AppBusinessName { get; set; }
        public string CompanyName { get; set; }
        public string Id { get; set; }
        public string EventName { get; set; }
        public bool Confirmed { get; set; }
    }
}
