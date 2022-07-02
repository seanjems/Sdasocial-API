using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;

namespace Sdasocial;

[Dependency(ReplaceServices = true)]
public class SdasocialBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "Sdasocial";
}
