using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Sdasocial.Data;

/* This is used if database provider does't define
 * ISdasocialDbSchemaMigrator implementation.
 */
public class NullSdasocialDbSchemaMigrator : ISdasocialDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
