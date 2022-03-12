using EstudosAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EstudosAPI.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
            //options.METODOS
        }

        //DbSet permite o CRUD dentro do banco com o EF
        public DbSet<ProductModel> Products { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<UserModel> Users { get; set; }
    }
}
