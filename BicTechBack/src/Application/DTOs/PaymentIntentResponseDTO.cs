using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public record PaymentIntentResponseDTO(string Id, string Status, long Amount, string Currency, string? ClientSecret = null);
}
