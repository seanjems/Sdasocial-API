using Sdasocial.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Sdasocial.Web.Pages;

public abstract class SdasocialPageModel : AbpPageModel
{
    protected SdasocialPageModel()
    {
        LocalizationResourceType = typeof(SdasocialResource);
    }
}
