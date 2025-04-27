using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RequestThrottler.Core;

namespace RequestThrottler.Extensions
{
    public class ThrottlingHandler : DelegatingHandler
    {
        private readonly IOutboundRequestThrottler _throttler;
        private readonly ThrottleOptions _options;

        public ThrottlingHandler(IOutboundRequestThrottler throttler, ThrottleOptions options)
        {
            _throttler = throttler;
            _options = options;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await _throttler.WaitBeforeSendAsync();
            return await base.SendAsync(request, cancellationToken);
        }
    }
} 