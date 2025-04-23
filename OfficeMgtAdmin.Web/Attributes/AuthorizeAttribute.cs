using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace OfficeMgtAdmin.Web.Attributes
{
    public class AuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    // 对于 AJAX 请求，返回 JSON 响应
                    context.Result = new JsonResult(new { success = false, message = "请先登录" })
                    {
                        StatusCode = 401
                    };
                }
                else
                {
                    // 对于普通请求，重定向到登录页面
                    context.Result = new RedirectToActionResult("Index", "Home", null);
                }
            }
        }
    }
} 