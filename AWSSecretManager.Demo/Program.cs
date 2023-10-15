using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using AWSSecretManager.Demo;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsDevelopment())
{
    builder.Configuration.AddSecretsManager(configurator: config =>
    {
        config.SecretFilter = record => record.Name.StartsWith($"{builder.Environment.EnvironmentName}/Demo/");
        config.KeyGenerator = (secret, name) => name
                        .Replace($"{builder.Environment.EnvironmentName}/Demo/", string.Empty)
                        .Replace("__", ":");
        //config.PollingInterval = TimeSpan.FromSeconds(5);
    });
}
builder.Services.AddOptions<DatabaseSettings>().BindConfiguration(nameof(DatabaseSettings));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonSecretsManager>();
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.MapGet("/secret", async (IAmazonSecretsManager secrets) =>
{
    var request = new GetSecretValueRequest()
    {
        SecretId = "dev/HelloWorld/ConnectionString",
        //VersionStage = "AWSPREVIOUS"
    };
    var data = await secrets.GetSecretValueAsync(request);
    return Results.Ok(data.SecretString);
});

app.MapGet("/from-options", (IOptionsMonitor<DatabaseSettings> options) =>
{
    return Results.Ok(options.CurrentValue.ConnectionString);
});

app.Run();