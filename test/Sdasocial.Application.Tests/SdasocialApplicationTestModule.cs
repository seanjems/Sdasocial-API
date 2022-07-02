using Volo.Abp.Modularity;

namespace Sdasocial;

[DependsOn(
    typeof(SdasocialApplicationModule),
    typeof(SdasocialDomainTestModule)
    )]
public class SdasocialApplicationTestModule : AbpModule
{

}
