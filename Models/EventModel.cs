using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CosmosDBEntities.Models;

namespace IencircleAdmin.Models
{
    public class EventModel
    {
        public string Id { get; set; }
        public string AppBusinessName { get; set; }
        public string EventLogo { get; set; }

        public List<Guest> Guests { get; set; }
        public Guest Guest { get; set; }
        public int Counter { get; set; }
    }
}
