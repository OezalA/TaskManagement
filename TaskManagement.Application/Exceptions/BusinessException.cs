using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Application.Exceptions
{
    public abstract class BusinessException : Exception
    {
        protected BusinessException(string message) : base(message)
        {
        }

        public abstract string Code { get; }
    }
}
