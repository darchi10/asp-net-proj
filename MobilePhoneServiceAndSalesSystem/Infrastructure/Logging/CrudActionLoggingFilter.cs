using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MobilePhoneServiceAndSalesSystem.Infrastructure.Logging
{
    public class CrudActionLoggingFilter : IActionFilter
    {
        private static readonly HashSet<string> CrudActionNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "Create",
            "Edit",
            "Delete",
            "DeleteConfirmed",
            "Post",
            "Put"
        };

        private readonly ILogger<CrudActionLoggingFilter> _logger;

        public CrudActionLoggingFilter(ILogger<CrudActionLoggingFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!ShouldLog(context))
            {
                return;
            }

            var action = GetActionName(context);
            var controller = GetControllerName(context);

            _logger.LogInformation(
                "CRUD action started: {Controller}.{Action} {Method} {Path} User={User} RouteId={RouteId}",
                controller,
                action,
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path,
                GetUserName(context.HttpContext),
                GetRouteId(context));
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (!ShouldLog(context))
            {
                return;
            }

            var action = GetActionName(context);
            var controller = GetControllerName(context);
            var statusCode = context.HttpContext.Response.StatusCode;

            if (context.Exception is not null)
            {
                _logger.LogError(
                    context.Exception,
                    "CRUD action failed: {Controller}.{Action} {Method} {Path} User={User} RouteId={RouteId}",
                    controller,
                    action,
                    context.HttpContext.Request.Method,
                    context.HttpContext.Request.Path,
                    GetUserName(context.HttpContext),
                    GetRouteId(context));

                return;
            }

            _logger.LogInformation(
                "CRUD action completed: {Controller}.{Action} {Method} {Path} StatusCode={StatusCode} User={User} RouteId={RouteId}",
                controller,
                action,
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path,
                statusCode,
                GetUserName(context.HttpContext),
                GetRouteId(context));
        }

        private static bool ShouldLog(FilterContext context)
        {
            var requestMethod = context.HttpContext.Request.Method;
            if (HttpMethods.IsPost(requestMethod) || HttpMethods.IsPut(requestMethod) || HttpMethods.IsDelete(requestMethod))
            {
                return true;
            }

            var actionName = GetActionName(context);
            return CrudActionNames.Contains(actionName);
        }

        private static string GetActionName(FilterContext context)
        {
            return context.ActionDescriptor is ControllerActionDescriptor descriptor
                ? descriptor.ActionName
                : context.ActionDescriptor.DisplayName ?? "UnknownAction";
        }

        private static string GetControllerName(FilterContext context)
        {
            return context.ActionDescriptor is ControllerActionDescriptor descriptor
                ? descriptor.ControllerName
                : "UnknownController";
        }

        private static string GetUserName(HttpContext httpContext)
        {
            return httpContext.User.Identity?.IsAuthenticated == true
                ? httpContext.User.Identity.Name ?? "AuthenticatedUser"
                : "Anonymous";
        }

        private static string? GetRouteId(FilterContext context)
        {
            return context.RouteData.Values.TryGetValue("id", out var id)
                ? id?.ToString()
                : null;
        }
    }
}
