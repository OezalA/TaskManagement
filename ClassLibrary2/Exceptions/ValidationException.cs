using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Application.Exceptions
{
    public class ValidationException : BusinessException
    {
        public ValidationException(string message, string code)
                : base(message)
        {
            Code = code;
        }

        public override string Code { get; }
    }
}
