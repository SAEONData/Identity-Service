using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAEON.Identity.Service.Health
{
    public class HealthModel
    {
        public string Database { get; set; } = "Ok";
        public bool Healthy { get; set; } = true;
    }
}
