using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GetComponent
{
    public class ComponentListBasedOnUser
    {
        public string User { get; set; }
        public List<ComponentDetails> ComponentList { get; set; }

        public ComponentListBasedOnUser()
        {
            ComponentList = new List<ComponentDetails>();
        }
    }
}