using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;

namespace Gapis.SA.Core.Services;
public interface IGoogleServiceProvider {
  Task<GmailService> InitializeGmailServiceAsync(string serviceAccountId, string superUserId, string[] scopes);
}

public class GoogleServiceProvider : IGoogleServiceProvider, IDisposable {
  private readonly ICertificateProvider? _certificateProvider;

  public GoogleServiceProvider(ICertificateProvider? certificateProvider) {
    this._certificateProvider = certificateProvider;
  }

  public void Dispose() {
    if (this._certificateProvider != null) {
      this._certificateProvider.Dispose();
    }
  }

  public async Task<GmailService> InitializeGmailServiceAsync(
      string serviceAccountId, string superUserId, string[] scopes) {
    var credential = await InitializeServiceAccountCredentialAsync(serviceAccountId, superUserId, scopes);
    return new GmailService(
       new BaseClientService.Initializer() {
         HttpClientInitializer = credential
         //????????? ApplicationName = applicationName ?????????
       });
  }

  private async Task<ServiceAccountCredential> InitializeServiceAccountCredentialAsync(
      string serviceAccountId, string superUserId, string[] scopes) {
    var credential = new ServiceAccountCredential(
             new ServiceAccountCredential.Initializer(serviceAccountId) {
               User = superUserId,
               Scopes = scopes
             }.FromCertificate(_certificateProvider?.Certificate));

    if (await credential.RequestAccessTokenAsync(CancellationToken.None))
      return credential;
    else
      throw new Exception($"Fail to receive new access token for ServiceAccountCredential" +
        $"{serviceAccountId} {superUserId}");
  }

}

