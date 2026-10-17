using Bams.Domain;

namespace Bams.Application;

public interface ICustomerRepository
{
    void Add(Customer customer);
    Task<Customer?> FindByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken);
}
