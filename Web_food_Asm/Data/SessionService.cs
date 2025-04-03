using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace WebFood.Services
{
    [ApiController]
    public class SessionService : ControllerBase
    {
        /// <summary>
        /// Lấy ID của người dùng từ Session hoặc Claims
        /// </summary>
        public string? UserId => HttpContext?.Session.GetString("id")
            ?? HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        protected string? UserEmail => HttpContext?.Session.GetString("Email");

        protected string? UserHinh => HttpContext?.Session.GetString("Hinh");
    }

    public class AuthorizeUserAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.Controller as SessionService;
            if (controller != null && string.IsNullOrEmpty(controller.UserId)) // Sửa lỗi ở đây
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Bạn chưa đăng nhập." });
            }

            base.OnActionExecuting(context);
        }
    }

}
