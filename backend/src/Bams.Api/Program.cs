using Bams.Api.DependencyInjection;
using Bams.Api.Monitoring;
using Bams.Api.Observability;
using Bams.Application;
using Bams.Infrastructure;
using Hellang.Middleware.ProblemDetails;
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5100");
var authenticationOptions = builder.Configuration.GetSection("Authentication").Get<AuthenticationServiceOptions>()
    ?? new AuthenticationServiceOptions();
authenticationOptions = new AuthenticationServiceOptions
{
    RefreshTokenLifetimeDays = authenticationOptions.RefreshTokenLifetimeDays,
    ClientBaseUrl = builder.Configuration["Account:ClientBaseUrl"],
    AllowInsecureClientUrl = authenticationOptions.AllowInsecureClientUrl
};
builder.Services.AddBamsApplication(authenticationOptions);
builder.Services.AddBamsInfrastructure(builder.Configuration, builder.Environment.IsDevelopment());
builder.Services.AddBamsApi(builder.Environment, builder.Configuration);
builder.Services.AddBamsMonitoring();
builder.Services.AddBamsObservability(builder.Configuration, builder.Environment);

var app = builder.Build();
await using (var scope = app.Services.CreateAsyncScope())
{
    var seedService = scope.ServiceProvider
        .GetRequiredService<Bams.Infrastructure.Seeding.BamsSeedService>();
    await seedService.InitialiseAsync();

    if (builder.Configuration.GetValue<bool>("Seed:Enabled"))
    {
        var adminEmail = builder.Configuration["Seed:AdminEmail"];
        var adminPassword = builder.Configuration["Seed:AdminPassword"];
        await seedService.SeedAsync(
            adminEmail ?? string.Empty,
            adminPassword ?? string.Empty);
        Console.WriteLine("BAMS demo data and Identity roles/admin were seeded successfully.");
    }
}

app.UseProblemDetails();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapBamsMonitoring();
app.UseCors("bams-spa");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.Run();
public partial class Program
{
}
