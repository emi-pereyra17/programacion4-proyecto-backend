using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Config
{
    public class StripeOptions
    {
        public string SecretKey { get; set; }
        public string BaseUrl { get; set; } = "https://api.stripe.com/v1";
    }
}
