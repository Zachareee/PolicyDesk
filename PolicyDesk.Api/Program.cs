using Microsoft.EntityFrameworkCore;
using PolicyDesk.Api.Data;
using PolicyDesk.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<PolicyDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var effectiveConnectionString = !string.IsNullOrWhiteSpace(connectionString)
        ? connectionString
        : "Server=localhost;Port=3306;Database=policydesk;User=root;";

    options.UseMySql(effectiveConnectionString, ServerVersion.AutoDetect(effectiveConnectionString));
});

builder.Services.AddScoped<IPolicyService, PolicyService>();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PolicyDbContext>();
    db.Database.Migrate();
}

app.MapControllers();

app.Run();

public partial class Program { }
