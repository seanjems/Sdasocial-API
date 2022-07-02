using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sdasocial.Data;
using Volo.Abp.DependencyInjection;

namespace Sdasocial.EntityFrameworkCore;

public class EntityFrameworkCoreSdasocialDbSchemaMigrator
    : ISdasocialDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreSdasocialDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the SdasocialDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<SdasocialDbContext>()
            .Database
            .MigrateAsync();
    }
}
