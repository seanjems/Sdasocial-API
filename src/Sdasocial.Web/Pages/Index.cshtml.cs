using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace Sdasocial.Web.Pages;

public class IndexModel : SdasocialPageModel
{
    public void OnGet()
    {

    }

    public async Task OnPostLoginAsync()
    {
        await HttpContext.ChallengeAsync("oidc");
    }
}
