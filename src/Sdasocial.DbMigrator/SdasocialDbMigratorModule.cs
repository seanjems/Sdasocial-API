using Sdasocial.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

namespace Sdasocial.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(SdasocialEntityFrameworkCoreModule),
    typeof(SdasocialApplicationContractsModule)
    )]
public class SdasocialDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
    }
}
