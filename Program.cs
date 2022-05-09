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
