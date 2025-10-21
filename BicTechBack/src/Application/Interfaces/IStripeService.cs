using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IStripeService
    {
        Task<PaymentIntentResponseDTO> CreatePaymentIntentAsync(CreatePaymentIntentDTO dto, CancellationToken ct = default);
        Task<PaymentIntentResponseDTO> GetPaymentIntentAsync(string id, CancellationToken ct = default);
    }

}
