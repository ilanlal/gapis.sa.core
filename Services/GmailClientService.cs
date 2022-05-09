using Google.Apis.Gmail.v1;

namespace Gapis.SA.Core.Services;

public interface IGmailClientService {
  Task<IList<Google.Apis.Gmail.v1.Data.Thread>> ListThreadsAsync(string userId);
}

public class GmailClientService : IGmailClientService {
  private readonly IGoogleServiceProvider _provider;

  public GmailClientService(IGoogleServiceProvider provider) {
    this._provider = provider;
  }

  public async Task<IList<Google.Apis.Gmail.v1.Data.Thread>> ListThreadsAsync(string userId) {
    string[] scopes = { GmailService.ScopeConstants.GmailReadonly };
    
    using (var gmailClient = await _provider.InitializeGmailServiceAsync("","",scopes)) {
      /*var request = new Gmail.UsersResource
        .ThreadsResource
        .ListRequest(gmailClient, userId);*/

      var request = gmailClient.Users.Threads.List(userId);
      request.MaxResults = 25;
      var res = await request.ExecuteAsync();
      return res.Threads;
    }
  }

  
}

