using Sdasocial.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Sdasocial;

[DependsOn(
    typeof(SdasocialEntityFrameworkCoreTestModule)
    )]
public class SdasocialDomainTestModule : AbpModule
{

}
