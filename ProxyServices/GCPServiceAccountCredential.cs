using Google.Apis.Auth.OAuth2;

using System.Security.Cryptography.X509Certificates;

namespace Gapis.SA.Core.ProxyServices;
public interface IGCPServiceAccountCredential {

  public ServiceAccountCredential GetCredential();
}
public class GCPServiceAccountCredential : IGCPServiceAccountCredential {
  public GCPServiceAccountCredential(string serviceAccount, string impersonateAccount, string certifcateFilePath) {

  }

  public static ServiceAccountCredential CreateCredential(string serviceAccount, string impersonateAccount, string certifcateFilePath) {
    var certificate = new X509Certificate2(certifcateFilePath,
                        "notasecret", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
    string[] scopes = { };
    if (string.IsNullOrEmpty(impersonateAccount))
      return null;

    var credential = new ServiceAccountCredential(
             new ServiceAccountCredential.Initializer(serviceAccount) {
               User = impersonateAccount,
               Scopes = scopes
             }.FromCertificate(certificate));

    if (credential.RequestAccessTokenAsync(CancellationToken.None).Result)
      return credential;

    return null;
  }

  public ServiceAccountCredential GetCredential() {
    throw new NotImplementedException();
  }
}

