using Google.Apis.Gmail.v1;

namespace Gapis.SA.Core.Services;

public interface IGmailClientService : IDisposable {
  Task<IList<Google.Apis.Gmail.v1.Data.Thread>> ListThreadsAsync(string userId);
}

public class GmailClientService : IGmailClientService, IDisposable {
  private readonly IGoogleServiceProvider _provider;

  public GmailClientService(IGoogleServiceProvider provider) {
    this._provider = provider;
  }

  public void Dispose() {
    if (this._provider != null) {
      _provider.Dispose();
    }
  }

  public async Task<IList<Google.Apis.Gmail.v1.Data.Thread>> ListThreadsAsync(string userId) {
    string[] scopes = { GmailService.ScopeConstants.GmailReadonly };

    var gmailClient = await _provider.CreateGmailServiceAsync(userId,scopes);

    var request = new UsersResource.ThreadsResource
      .ListRequest(gmailClient, userId);
    
    //var request = gmailClient.Users.Threads.List(userId);
    request.MaxResults = 25;
    //request.u
    var res = await request.ExecuteAsync();
    return res.Threads;
  }
}

