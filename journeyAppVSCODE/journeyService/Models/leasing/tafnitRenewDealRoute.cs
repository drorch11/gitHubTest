using Microsoft.AspNetCore.Mvc;
using System.Reflection.PortableExecutable;
using System.Xml.Schema;

namespace journeyService.Models.leasing
{
    public class tafnitRenewDealRoute
    {
        public int LogIdInforU { get; set; }
        public string hpno { get; set; } = string.Empty;
        public string dealno { get; set; } = string.Empty;
        public int step { get; set; }

        public int LogIdLeasingContractRenew { get; set; }
        
        
        
    }
}
