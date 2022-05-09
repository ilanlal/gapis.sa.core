using Gapis.SA.Core.Services;
using Google.Apis.Gmail.v1;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string filePath = "";//Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),)
//string serviceAccountId = "";
//string superAdminId = "";
builder.Services.AddSingleton<ICertificateProvider>((provider) => {
  return new CertificateProvider(filePath);
});

builder.Services.AddTransient<IGoogleServiceProvider>((provider) => {
  var certificte = provider.GetService<ICertificateProvider>();
  
  return new GoogleServiceProvider(certificte);
});

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
