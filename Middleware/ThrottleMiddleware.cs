using Microsoft.AspNetCore.Http;
using RequestThrottler.Core;

namespace RequestThrottler.Middleware
{
    public class ThrottleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRequestThrottler _throttler;

        public ThrottleMiddleware(RequestDelegate next, IRequestThrottler throttler)
        {
            _next = next;
            _throttler = throttler;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _throttler.ExecuteAsync(async () =>
            {
                await _next(context);
            });
        }
    }
}
