# GmailAPI request to Google Workspace using Service Account
> .NET 6.0
> ASP.NET Core WebAPI
> C#

A service account is a special kind of account used by an application, rather than a person. 

You can use a service account to access data or perform actions by the robot account itself, or to access data on behalf of Google Workspace or Cloud Identity users.

## Prerequisites:
  + ### A Google Cloud Platform project 
    With the Admin SDK API enabled service account with domain-wide delegation. 
  + ### A Google Workspace domain.
    With account in that domain with administrator privileges.
  + ### Visual Studio 2013 or later 

## Step 1: Set up the Google Cloud Platform project
+ ### Create Google Cloud project
  A Google Cloud project is required to use Google Workspace APIs and build Google Workspace add-ons or apps.
  If you don't already have a Google Cloud project, refer to: [How to create a Google Cloud project](https://developers.google.com/workspace/guides/create-project)

+ ### Enable Google Workspace APIs
  Before using Google APIs, you need to enable them in a Google Cloud project. 
    
  To Enable Google Workspace APIs refer to: [How to Enable Google Workspace APIs](https://developers.google.com/workspace/guides/enable-apis)
  
  For this example you are enabling the the [Admin SDK Directory API](https://developers.google.com/admin-sdk/directory)
  with the data scope `/auth/admin.directory.user.readonly`.
 
    
+ ### Create Service Account with domain-wide delegation
  To create service account refer to: [How to create service account?](https://developers.google.com/workspace/guides/create-credentials#create_a_service_account)
  
  In the `Domain wide delegation` pane, select `Manage Domain Wide Delegation`.

+ ### Download Service Account private key (p12 format)
  Download p12 file [contains the private key](https://cloud.google.com/iam/docs/creating-managing-service-account-keys) for your Service Account.
  
## Step 2: Set up the Google Workspace 
+ ### Enable API access in the Google Workspace domain with
  To enable API access in Google Workspace domain, refer to: [how to enable API access](https://support.google.com/a/answer/7281227?visit_id=637865874764605082-823144595&rd=1)
+ ### Delegating domain-wide authority to the service account
  To call APIs on behalf of users in a Google Workspace organization, your service account needs to be granted domain-wide delegation of authority in the Google Workspace Admin console __by a super administrator account__
  
  To delegating domain-wide authority in Google Workspace domain, refer to: [How to Delegating domain-wide authority](https://developers.google.com/identity/protocols/oauth2/service-account#delegatingauthority) to the service account

## Step 3: Prepare Visual Stodio project - 
+ ### Create a new Visual C# ASP.NET Core WebAPI (.NET 6.0) project in Visual Studio.
+ ### Open the NuGet Package Manager Console, select the package source nuget.org, and run the following commands:
  + #### `Install-Package Google.Apis.Auth`
  + #### `Install-Package Google.Apis.Gmail.v1`

## Step 4: Add code

### `CertificateProvider.cs`
```csharp
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
```

### `GoogleServiceProvider.cs`
```csharp
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
         HttpClientInitializer = credential
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
```

### `GmailClientService.cs`
```csharp
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
```

### Startup - `Program.cs`

```csharp
using Gapis.SA.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// The full path; name of a certificate file
string filePath = "[X509 Certificate File]";

// The service account ID (typically an e-mail address like: *@*iam.gserviceaccount.com)
string serviceAccountId = "[Service Account ID]";

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ICertificateProvider>((provider) => {
  return new CertificateProvider(filePath);
});

builder.Services.AddSingleton<IGoogleServiceProvider>((provider) => {
  var certificte = provider.GetService<ICertificateProvider>();

  return new GoogleServiceProvider(certificte, serviceAccountId);
});

builder.Services.AddTransient<IGmailClientService, GmailClientService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

```

### Execute GmailAPI request - `GmailController.cs`
```csharp
using Gapis.SA.Core.Services;

using Microsoft.AspNetCore.Mvc;

namespace Gapis.SA.Core.Controllers;

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
```

## References

  + [Creating and managing service account keys](https://cloud.google.com/iam/docs/creating-managing-service-account-keys#iam-service-account-keys-create-csharp)