using Bams.Domain;
using Bams.Application.Abstractions.Security;
using Bams.Infrastructure.Identity;
using Bams.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bams.Infrastructure.Seeding;

public sealed class BamsSeedService(
    BamsDbContext dbContext,
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager,
    ILogger<BamsSeedService> logger)
{
    private static readonly (string Code, string Name, int DurationMinutes)[] DemoServices =
    [
        ("CONSULT", "Consultation", 30),
        ("STANDARD", "Standard service", 60),
        ("PREMIUM", "Premium service", 90)
    ];

    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred while initializing the BAMS database.");
            throw;
        }
    }

    public async Task SeedAsync(
        string adminEmail,
        string adminPassword,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await TrySeedAsync(adminEmail, adminPassword, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred while seeding BAMS data.");
            throw;
        }
    }

    public async Task TrySeedAsync(
        string adminEmail,
        string adminPassword,
        CancellationToken cancellationToken = default)
    {
        adminEmail = adminEmail.Trim();
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException("Seed:AdminEmail and Seed:AdminPassword must be configured.");
        }
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        foreach (var roleName in new[]
        {
            IdentityRoleNames.Customer,
            IdentityRoleNames.BranchStaff,
            IdentityRoleNames.BranchManager,
            IdentityRoleNames.SystemAdministrator
        })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                EnsureSucceeded(roleResult, $"create role '{roleName}'");
            }
        }

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                IsEnabled = true
            };
            var createResult = await userManager.CreateAsync(admin, adminPassword);
            EnsureSucceeded(createResult, "create the system administrator");
        }

        if (!await userManager.IsInRoleAsync(admin, IdentityRoleNames.SystemAdministrator))
        {
            var addRoleResult = await userManager.AddToRoleAsync(admin, IdentityRoleNames.SystemAdministrator);
            EnsureSucceeded(addRoleResult, "assign the system administrator role");
        }

        var branch = await dbContext.Branches.SingleOrDefaultAsync(x => x.Code == "DEMO-JHB", cancellationToken);
        if (branch is null)
        {
            branch = new Branch
            {
                Id = Guid.Parse("f82c5b20-7118-48ca-99e1-04d8a4d66b01"),
                Code = "DEMO-JHB",
                Name = "BAMS Demo Branch",
                City = "Johannesburg",
                Address = "1 Demo Street, Sandton",
                TimeZone = "Africa/Johannesburg",
                Status = BranchStatus.Active
            };
            dbContext.Branches.Add(branch);
        }

        var services = new List<Service>();
        foreach (var demoService in DemoServices)
        {
            var service = await dbContext.Services.SingleOrDefaultAsync(x => x.Code == demoService.Code, cancellationToken);
            if (service is null)
            {
                service = new Service
                {
                    Id = GuidFromCode(demoService.Code),
                    Code = demoService.Code,
                    Name = demoService.Name,
                    DurationMinutes = demoService.DurationMinutes,
                    IsActive = true
                };
                dbContext.Services.Add(service);
            }

            services.Add(service);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var service in services)
        {
            var hasMapping = await dbContext.BranchServices.AnyAsync(
                x => x.BranchId == branch.Id && x.ServiceId == service.Id,
                cancellationToken);
            if (!hasMapping)
            {
                dbContext.BranchServices.Add(new BranchService
                {
                    BranchId = branch.Id,
                    ServiceId = service.Id,
                    Capacity = 2,
                    IsEnabled = true
                });
            }
        }

        for (var day = (int)DayOfWeek.Monday; day <= (int)DayOfWeek.Friday; day++)
        {
            var hasSchedule = await dbContext.BranchSchedules.AnyAsync(
                x => x.BranchId == branch.Id && x.DayOfWeek == day,
                cancellationToken);
            if (!hasSchedule)
            {
                dbContext.BranchSchedules.Add(new BranchSchedule
                {
                    Id = GuidFromCode($"DEMO-JHB-{day}"),
                    BranchId = branch.Id,
                    DayOfWeek = day,
                    OpensAt = new TimeOnly(9, 0),
                    ClosesAt = new TimeOnly(17, 0),
                    IsActive = true
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static Guid GuidFromCode(string value)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value));
        return new Guid(bytes[..16]);
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Could not {operation}: {errors}");
        }
    }
}
