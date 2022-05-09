using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;

namespace Gapis.SA.Core.Services;
public interface IGoogleServiceProvider : IDisposable {
  string ServiceAccountId { get; }
  Task<GmailService> CreateGmailServiceAsync(string userId, string[] scopes);
}

public class GoogleServiceProvider : IGoogleServiceProvider, IDisposable {
  private readonly ICertificateProvider? _certificateProvider;
  public string ServiceAccountId { get; }

  public GoogleServiceProvider(ICertificateProvider? certificateProvider, string serviceAccountId) {
    this._certificateProvider = certificateProvider;
    this.ServiceAccountId = serviceAccountId;
  }

  public void Dispose() {
    if (this._certificateProvider != null) {
      this._certificateProvider.Dispose();
    }
  }

  public async Task<GmailService> CreateGmailServiceAsync(string userId, string[] scopes) {
    var credential = await InitializeServiceAccountCredentialAsync(userId,scopes);
    return new GmailService(
       new BaseClientService.Initializer() {
         HttpClientInitializer = credential,
         
         //????????? ApplicationName = applicationName ?????????
       });
  }

  private async Task<ServiceAccountCredential> InitializeServiceAccountCredentialAsync(string userId, string[] scopes) {
    var credential = new ServiceAccountCredential(
             new ServiceAccountCredential.Initializer(ServiceAccountId) {
               User = userId,
               Scopes = scopes
             }.FromCertificate(_certificateProvider?.Certificate));

    if (await credential.RequestAccessTokenAsync(CancellationToken.None))
      return credential;
    else
      throw new Exception($"Fail to receive new access token for ServiceAccountCredential" +
        $"{ServiceAccountId} {userId}");
  }

}

