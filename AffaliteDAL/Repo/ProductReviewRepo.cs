using AffaliteDAL.Data;
using AffaliteDAL.Entities;
using AffaliteDAL.IRepo;

namespace AffaliteDAL.Repo;

public class ProductReviewRepo : GenericRepository<ProductReviews>, IProductReviewRepo
{
    public ProductReviewRepo(AffaliteDBContext context) : base(context)
    {
    }

    public void Delete(ProductReviews entity, int id)
    {
        var entityToDelete = GetAllQueryable().FirstOrDefault(r => r.Id == id);
        if (entityToDelete != null)
        {
            Delete(entityToDelete);
            SaveChanges();
        }
    }

    public void Update(ProductReviews entity, int id)
    {
        var entityToUpdate = GetAllQueryable().FirstOrDefault(r => r.Id == id);
        if (entityToUpdate != null)
        {
            entityToUpdate.Comment = entity.Comment;
            entityToUpdate.Rating = entity.Rating;
            Update(entityToUpdate);
            SaveChanges();
        }
    }
}
