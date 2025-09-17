using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanManagement.Service.Exceptions
{
    internal class BadRequestException : Exception
    {
        public int StatusCode { get; set; }
        public BadRequestException(string message) : base(message)
        {
            StatusCode = 400;
        }
    }
}
