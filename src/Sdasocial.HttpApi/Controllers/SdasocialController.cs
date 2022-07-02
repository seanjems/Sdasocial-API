using Sdasocial.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Sdasocial.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class SdasocialController : AbpControllerBase
{
    protected SdasocialController()
    {
        LocalizationResource = typeof(SdasocialResource);
    }
}
