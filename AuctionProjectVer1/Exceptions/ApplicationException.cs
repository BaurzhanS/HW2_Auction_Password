using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuctionProjectVer1.Extensions;

namespace AuctionProjectVer1.Exceptions
{
    public class ApplicationException : Exception
    {
        public ApplicationException(string exceptionIfno) : base(exceptionIfno)
        {

        }
    }
}
