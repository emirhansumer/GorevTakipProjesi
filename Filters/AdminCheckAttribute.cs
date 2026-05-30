using GorevTakip.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GorevTakip.Filters;

// Sadece admin (IsAdmin) kullanıcıların erişebileceği aksiyonları korur.
// Giriş yoksa Login'e, giriş var ama admin değilse 403'e yönlendirir.
public class AdminCheckAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;

        if (!session.GirisYapildiMi())
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        if (!session.AdminMi())
        {
            context.Result = new RedirectToActionResult("HataKodu", "Home", new { code = 403 });
            return;
        }

        base.OnActionExecuting(context);
    }
}
