//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Filters;
//using System.Security.Claims;

//namespace Web_food_Asm.Data
//{
//    public class AuthorizeUserAttribute : Attribute, IActionFilter
//    {
//        public void OnActionExecuting(ActionExecutingContext context)
//        {
//            var user = context.HttpContext.User;

//            if (user == null || !user.Identity.IsAuthenticated)
//            {
//                context.Result = new UnauthorizedObjectResult(new { message = "Người dùng chưa đăng nhập." });
//                return;
//            }

//            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (string.IsNullOrEmpty(userId))
//            {
//                context.Result = new UnauthorizedObjectResult(new { message = "Không xác định được tài khoản người dùng." });
//            }
//        }

//        public void OnActionExecuted(ActionExecutedContext context) { }
//    }
//}
