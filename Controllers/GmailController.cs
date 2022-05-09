using Gapis.SA.Core.Services;

using Microsoft.AspNetCore.Mvc;

namespace Gapis.SA.Core.Controllers {
  [ApiController]
  [Route("[controller]")]
  public class GmailController : ControllerBase {
    private readonly ILogger<GmailController> _logger;
    private readonly IGmailClientService _gmailClient;

    public GmailController(ILogger<GmailController> logger, IGmailClientService gmailClient) {
      _logger = logger;
      _gmailClient = gmailClient;
    }

    [HttpGet(Name = "ListThreads")]
    public async Task<IEnumerable<ThreadViewModel>> ListThreads(string userId) {
      var res = await _gmailClient.ListThreadsAsync(userId);

      return res.Select(thread => new ThreadViewModel {
        Snippet = thread.Snippet,
        Id = thread.Id,
      });
    }
  }
}