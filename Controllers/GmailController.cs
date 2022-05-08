using Gapis.SA.Core.ProxyServices;

using Microsoft.AspNetCore.Mvc;

namespace Gapis.SA.Core.Controllers {
  [ApiController]
  [Route("[controller]")]
  public class GmailController : ControllerBase {
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<GmailController> _logger;
    private readonly IGmailClientService _gmailClient;

    public GmailController(ILogger<GmailController> logger, IGmailClientService gmailClient) {
      _logger = logger;
      _gmailClient = gmailClient;
    }

    [HttpGet(Name = "ListThreads")]
    public IEnumerable<ThreadViewModel> ListThreads() {
      return _gmailClient.ListThreads().Select(thread => new ThreadViewModel {
        Snippet = thread.Snippet,
        Id = thread.Id,
      }) 
      .ToArray();
    }
  }
}