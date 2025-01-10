using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWhatsAppVerification.Domain.Response
{
    public class EvolutionNumberResponse
    {
        public bool Success { get; set; }
        public bool Exists { get; set; }
        public string Message { get; set; }
    }
}
