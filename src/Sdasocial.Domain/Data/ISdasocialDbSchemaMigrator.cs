using System.Threading.Tasks;

namespace Sdasocial.Data;

public interface ISdasocialDbSchemaMigrator
{
    Task MigrateAsync();
}
