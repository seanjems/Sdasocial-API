using System;
using System.Collections.Generic;
using System.Text;
using Sdasocial.Localization;
using Volo.Abp.Application.Services;

namespace Sdasocial;

/* Inherit your application services from this class.
 */
public abstract class SdasocialAppService : ApplicationService
{
    protected SdasocialAppService()
    {
        LocalizationResource = typeof(SdasocialResource);
    }
}
