using Bams.Application;
using Bams.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bams.Infrastructure;

public sealed class CustomerRepository(BamsDbContext db) : ICustomerRepository
{
    public void Add(Customer customer) => db.Customers.Add(customer);

    public Task<Customer?> FindByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken) =>
        db.Customers.SingleOrDefaultAsync(customer => customer.IdentityUserId == identityUserId, cancellationToken);
}
