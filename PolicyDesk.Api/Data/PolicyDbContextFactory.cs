using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PolicyDesk.Api.Data;

public class PolicyDbContextFactory : IDesignTimeDbContextFactory<PolicyDbContext>
{
    public PolicyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PolicyDbContext>();
        var connectionString = "Server=localhost;Port=3306;Database=policydesk;User=root;";
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new PolicyDbContext(optionsBuilder.Options);
    }
}
