using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuctionProjectVer1.Models
{
    public class ApplicationUser
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public bool IsActivatedAccount { get; set; }
        public int FailedSigninCount { get; set; }
        public bool IsBlockedBySystem { get; set; }
        public string AssociatedEmployeeId { get; set; }
        public string CreationDate { get; set; }

    }
}
