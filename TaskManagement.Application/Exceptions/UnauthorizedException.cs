using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Application.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public string ErrorCode { get; }

        public UnauthorizedException(string message, string errorCode = "Unauthorized")
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
