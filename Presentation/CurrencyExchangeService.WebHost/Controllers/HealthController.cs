using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchangeService.WebHost.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<object> Get()
        => Ok(new { status = "ok" });
}

