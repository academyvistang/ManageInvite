using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IencircleAdmin.Models
{
    public abstract class Entity
    {
        public Entity(string type)
        {
            this.Type = type;
        }
        /// <summary>
        /// Object unique identifier
        /// </summary>
        [Key]
        [JsonProperty("id")]
        public string Id { get; set; }
        /// <summary>
        /// Object type
        /// </summary>
        public string Type { get; private set; }
    }
}
