using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using RequestThrottler.Core;
using RequestThrottler.Core.Models;

namespace RequestThrottler.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ThrottleAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int _limit;
        private readonly int _windowInSeconds;

        public ThrottleAttribute(int limit, int windowInSeconds)
        {
            _limit = limit;
            _windowInSeconds = windowInSeconds;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var throttler = context.HttpContext.RequestServices.GetService<IRequestThrottler>();
            if (throttler == null)
            {
                await next();
                return;
            }

            await throttler.ExecuteAsync(async () => await next());
        }
    }
}
