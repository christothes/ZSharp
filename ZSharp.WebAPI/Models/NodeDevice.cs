using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZSharp.WebAPI.Models
{
    public class NodeDevice
    {
        public byte NodeID { get; set; }
        public string NodeDescription { get; set; }
        public byte State { get; set; }
        public string NodeType { get; set; }
    }
}