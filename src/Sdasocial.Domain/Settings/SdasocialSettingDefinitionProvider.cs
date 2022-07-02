using Volo.Abp.Settings;

namespace Sdasocial.Settings;

public class SdasocialSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(SdasocialSettings.MySetting1));
    }
}
