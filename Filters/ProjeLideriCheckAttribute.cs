using GorevTakip.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GorevTakip.Filters;

// En az "Proje Lideri" rolü gerektiren aksiyonları korur.
// Giriş yoksa Login'e, yetki yetersizse 403'e yönlendirir.
public class ProjeLideriCheckAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;

        if (!session.GirisYapildiMi())
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        if (!session.ProjeLideriVeUstu())
        {
            context.Result = new RedirectToActionResult("HataKodu", "Home", new { code = 403 });
            return;
        }

        base.OnActionExecuting(context);
    }
}
