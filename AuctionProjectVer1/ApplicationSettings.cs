using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuctionProjectVer1
{
    public class ApplicationSettings
    {
        public static readonly string IDENTITY_CONNECTION_STRING = 
            ConfigurationManager.ConnectionStrings["IdentityDbConnectionString"]
            .ConnectionString;

        public static readonly string APPLICATION_CONNECTION_STRING =
            ConfigurationManager.ConnectionStrings["AuctionDbConnectionString"].ConnectionString;
    }
}
