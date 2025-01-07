using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWhatsAppVerification.Domain.Entities
{
    public class PhoneNumberVerification
    {
        public string Id { get; set; }
        public string PhoneNumber { get; set; }
        public bool HasWhatsApp { get; set; }
        public DateTime VerifiedAt { get; set; }
    }
}
