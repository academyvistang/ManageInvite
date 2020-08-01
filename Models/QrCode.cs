using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IencircleAdmin.Models
{
    public class Config
    {
        public string body { get; set; }
        public string logo { get; set; }
    }

    public class RootObject
    {
        public string data { get; set; }
        public Config config { get; set; }
        public int size { get; set; }
        public bool download { get; set; }
        public string file { get; set; }
    }
}
