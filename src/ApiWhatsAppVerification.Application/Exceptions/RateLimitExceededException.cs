using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWhatsAppVerification.Application.Exceptions
{
    public class RateLimitExceededException : Exception
    {
        public RateLimitExceededException() : base() { }

        public RateLimitExceededException(string message) : base(message) { }

        public RateLimitExceededException(string message, Exception innerException) : base(message, innerException) { }
    }
}
