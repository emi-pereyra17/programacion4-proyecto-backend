using Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly Application.Interfaces.IStripeService _stripeService;

        public PaymentsController(Application.Interfaces.IStripeService stripeService) => _stripeService = stripeService;

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreatePaymentIntentDTO req, CancellationToken ct)
        {
            var pi = await _stripeService.CreatePaymentIntentAsync(req, ct);
            return Ok(pi);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id, CancellationToken ct)
        {
            var pi = await _stripeService.GetPaymentIntentAsync(id, ct);
            return Ok(pi);
        }
    }

}
