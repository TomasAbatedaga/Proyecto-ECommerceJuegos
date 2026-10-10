using Microsoft.Extensions.Configuration;

namespace ECommerceJuegos.Repositories
{
    public abstract class RepositorioBase
    {
        protected readonly string ConnectionString;

        protected RepositorioBase(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection")!;
        }
    }
}