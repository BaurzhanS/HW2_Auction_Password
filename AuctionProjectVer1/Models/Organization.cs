using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuctionProjectVer1.Models
{
    public class Organization
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string IdentificationNumber { get; set; }
        public string RegistrationDate { get; set; }
        public string OrganizationTypeId { get; set; }
    }
}
