using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RequestThrottler.Core.Models;
using RequestThrottler.Core;
using RequestThrottler.Services;
using RequestThrottler.Utils;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Net.Http;

namespace RequestThrottler.Extensions
{
    public class ThrottleOptions
    {
        public int Limit { get; set; }
        public TimeSpan Window { get; set; }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRequestThrottling(this IServiceCollection services, Action<ThrottleOptions> configure)
        {
            services.Configure(configure);
            services.AddSingleton<ITimeProvider, SystemTimeProvider>();
            services.AddSingleton<IRateLimiter>(sp => {
                var opts = sp.GetRequiredService<IOptions<ThrottleOptions>>().Value;
                return new FixedWindowRateLimiter(
                    new RateLimitRule(opts.Limit, (int)opts.Window.TotalSeconds),
                    sp.GetRequiredService<ITimeProvider>());
            });
            services.AddSingleton<IInMemoryThrottleQueue, InMemoryThrottleQueue>();
            services.AddSingleton<IRequestThrottler, DefaultRequestThrottler>();
            services.AddHostedService<QueueConsumerHostedService>();
            return services;
        }

        public static IHttpClientBuilder AddThrottling(this IHttpClientBuilder builder, Action<ThrottleOptions> configure)
        {
            builder.Services.Configure(configure);
            builder.Services.AddSingleton<IOutboundRequestThrottler, OutboundRequestThrottler>();
            
            builder.AddHttpMessageHandler(sp => 
                new ThrottlingHandler(
                    sp.GetRequiredService<IOutboundRequestThrottler>(),
                    sp.GetRequiredService<IOptions<ThrottleOptions>>().Value
                )
            );
            
            return builder;
        }
    }

    public class QueueConsumerHostedService : BackgroundService
    {
        private readonly IInMemoryThrottleQueue _queue;
        private readonly CancellationTokenSource _stoppingCts = new();

        public QueueConsumerHostedService(IInMemoryThrottleQueue queue)
        {
            _queue = queue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _stoppingCts.Token);
            while (!linkedCts.IsCancellationRequested)
            {
                var work = await _queue.DequeueAsync(linkedCts.Token);
                await work();
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _queue.Complete();
            _stoppingCts.Cancel();
            await base.StopAsync(cancellationToken);
        }
    }
}
