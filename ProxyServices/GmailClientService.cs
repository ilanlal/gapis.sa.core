using Google.Apis.Services;
using Gmail = Google.Apis.Gmail.v1;

namespace Gapis.SA.Core.ProxyServices;

public interface IGmailClientService {
  List<Gmail.Data.Thread> ListThreads();
}

public class GmailClientService : IGmailClientService {
  public static Gmail.GmailService Create(string applicationName, string serviceAccount, string impersonateEmail, string cerFilePath) {
    return new Gmail.GmailService(
      new BaseClientService.Initializer() {
        HttpClientInitializer = GCPServiceAccountCredential.CreateCredential(serviceAccount, impersonateEmail, cerFilePath),
        ApplicationName = applicationName
      });
  }
    
  public List<Gmail.Data.Thread> ListThreads() {
    throw new NotImplementedException();
  }
}

