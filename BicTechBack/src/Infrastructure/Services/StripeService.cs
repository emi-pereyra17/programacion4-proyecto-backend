using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    using Application.DTOs;
    using Infrastructure.Config;
    using Microsoft.Extensions.Options;
    using System.Net.Http;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class StripeService : Application.Interfaces.IStripeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly StripeOptions _options;

        public StripeService(IHttpClientFactory httpClientFactory, IOptions<StripeOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        public async Task<PaymentIntentResponseDTO> CreatePaymentIntentAsync(CreatePaymentIntentDTO dto, CancellationToken ct = default)
        {
            var client = _httpClientFactory.CreateClient("Stripe");

            var form = new Dictionary<string, string>
            {
                ["amount"] = dto.AmountInCents.ToString(),
                ["currency"] = dto.Currency,
                ["payment_method_types[]"] = "card"
            };

            if (!string.IsNullOrEmpty(dto.Description))
                form["description"] = dto.Description;

            using var content = new FormUrlEncodedContent(form);
            var response = await client.PostAsync("payment_intents", content, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Stripe error {(int)response.StatusCode}: {body}");

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var pi = JsonSerializer.Deserialize<StripePaymentIntentResponse>(body, jsonOptions);

            return new PaymentIntentResponseDTO(pi.Id, pi.Status, pi.Amount, pi.Currency, pi.ClientSecret);
        }

        public async Task<PaymentIntentResponseDTO> GetPaymentIntentAsync(string id, CancellationToken ct = default)
        {
            var client = _httpClientFactory.CreateClient("Stripe");
            var response = await client.GetAsync($"payment_intents/{id}", ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Stripe error {(int)response.StatusCode}: {body}");

            var pi = JsonSerializer.Deserialize<StripePaymentIntentResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return new PaymentIntentResponseDTO(pi.Id, pi.Status, pi.Amount, pi.Currency, pi.ClientSecret);
        }

        private class StripePaymentIntentResponse
        {
            public string Id { get; set; }
            public string Status { get; set; }
            public long Amount { get; set; }
            public string Currency { get; set; }

            [JsonPropertyName("client_secret")]
            public string ClientSecret { get; set; }
        }
    }

}
