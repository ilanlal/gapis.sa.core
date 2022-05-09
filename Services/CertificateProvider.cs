using System.Security.Cryptography.X509Certificates;

namespace Gapis.SA.Core.Services;
public interface ICertificateProvider : IDisposable {
  X509Certificate2 Certificate { get; }
}

public class CertificateProvider : ICertificateProvider, IDisposable {
  public X509Certificate2 Certificate { get; }

  public CertificateProvider(string fileName) {
    this.Certificate = new X509Certificate2(
      fileName,
      "notasecret",
      X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
  }

  public void Dispose() {
    if (this.Certificate != null) {
      this.Certificate.Dispose();
    }
  }
}

