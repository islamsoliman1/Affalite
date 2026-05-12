using AffaliteDAL.Entities;

namespace AffaliteDAL.IRepo;

public interface IOrderRepo
{
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByAffIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByMerIdAsync(int id, CancellationToken cancellationToken = default);
}
