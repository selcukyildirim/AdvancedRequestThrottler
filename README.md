# RequestThrottler

[![NuGet](https://img.shields.io/nuget/v/RequestThrottler.svg)](https://www.nuget.org/packages/RequestThrottler)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

ASP.NET Core uygulamaları için sade ve güçlü bir request throttling (istek kısıtlama) kütüphanesi.

## Özellikler

- Global rate limiting (tüm uygulama için istek limiti)
- In-memory kuyruk yönetimi
- Basit ve hızlı kurulum
- HTTP Client throttling desteği
- Attribute ile action/controller bazlı throttling

## Kurulum

```bash
dotnet add package RequestThrottler
```

## Hızlı Başlangıç

### Global Rate Limiting

```csharp
// Program.cs
using RequestThrottler.Extensions;

builder.Services.AddRequestThrottling(options =>
{
    options.Limit = 100; // Dakikada 100 istek
    options.Window = TimeSpan.FromMinutes(1);
});

app.UseRequestThrottler();
```

### Controller veya Action Bazlı Throttling

```csharp
using RequestThrottler.Attributes;

[Throttle(50, 60)] // 60 saniyede 50 istek
public class MyController : ControllerBase
{
    [Throttle(10, 60)] // 60 saniyede 10 istek (sadece bu action için)
    public IActionResult Get()
    {
        return Ok();
    }
}
```

### HTTP Client Throttling

```csharp
// Program.cs
builder.Services.AddHttpClient("ExternalApi")
    .AddThrottling(options =>
    {
        options.Limit = 60; // Dakikada 60 istek
        options.Window = TimeSpan.FromMinutes(1);
    });
```

```csharp
public class ExternalApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ExternalApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ApiResponse> CallExternalApiAsync()
    {
        var client = _httpClientFactory.CreateClient("ExternalApi");
        var response = await client.GetAsync("https://external-api.com/data");
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }
}

## Gelecek Planları

### Kısa Vadeli (Sonraki Sürüm)
- Sliding Window rate limiting algorithm
- Request metrics ve analytics
- IP-based rate limiting
- User-based rate limiting
- Enhanced logging capabilities

### Orta Vadeli
- Distributed caching desteği (Redis)
- Token Bucket algorithm implementation
- Performance optimizations
- Custom rule providers
- Advanced request filtering

### Uzun Vadeli
- Multi-tenant desteği
- Real-time monitoring dashboard
- Adaptive rate limiting
- DDoS protection features
- Mobile SDK desteği

## Özellikler

- Request throttling ve rate limiting
- In-memory request queue
- Fixed window rate limiting algorithm
- Middleware ve attribute-based configuration
- ASP.NET Core entegrasyonu
- Configurable rules ve limits

## Kurulum

```bash
dotnet add package RequestThrottler
```

## Hızlı Başlangıç

### Temel Yapılandırma

```csharp
// Program.cs
builder.Services.AddRequestThrottler(options =>
{
    options.DefaultRateLimit = 100; // dakikada istek sayısı
    options.DefaultBurstLimit = 10; // eşzamanlı istek sayısı
});
```

### Middleware Kullanımı

```csharp
// Program.cs
app.UseRequestThrottler();
```

### Attribute Kullanımı

```csharp
[Throttle(RateLimit = 50, BurstLimit = 5)]
public class MyController : ControllerBase
{
    [Throttle(RateLimit = 20)]
    public IActionResult Get()
    {
        return Ok();
    }
}
```

## Gelişmiş Yapılandırma

### Özel Rate Limiting Rules

```csharp
builder.Services.AddRequestThrottler(options =>
{
    options.AddRule(new RateLimitRule
    {
        Endpoint = "/api/endpoint",
        RateLimit = 30,
        BurstLimit = 5,
        TimeWindow = TimeSpan.FromMinutes(1)
    });
});
```

### Custom Queue Implementation

```csharp
public class CustomThrottleQueue : IThrottleQueue
{
    private readonly ConcurrentDictionary<string, Queue<ThrottledRequest>> _queues;
    private readonly ILogger<CustomThrottleQueue> _logger;

    public CustomThrottleQueue(ILogger<CustomThrottleQueue> logger)
    {
        _queues = new ConcurrentDictionary<string, Queue<ThrottledRequest>>();
        _logger = logger;
    }

    public async Task<bool> TryEnqueueAsync(string key, ThrottledRequest request)
    {
        var queue = _queues.GetOrAdd(key, _ => new Queue<ThrottledRequest>());
        
        lock (queue)
        {
            if (queue.Count >= request.BurstLimit)
            {
                _logger.LogWarning($"Queue limit exceeded: {key}");
                return false;
            }

            queue.Enqueue(request);
            _logger.LogInformation($"Request enqueued: {key}");
            return true;
        }
    }

    public async Task<ThrottledRequest?> DequeueAsync(string key)
    {
        if (!_queues.TryGetValue(key, out var queue))
            return null;

        lock (queue)
        {
            if (queue.Count == 0)
                return null;

            var request = queue.Dequeue();
            _logger.LogInformation($"Request dequeued: {key}");
            return request;
        }
    }

    public async Task<int> GetQueueLengthAsync(string key)
    {
        return _queues.TryGetValue(key, out var queue) ? queue.Count : 0;
    }
}

// Registration
builder.Services.AddSingleton<IThrottleQueue, CustomThrottleQueue>();
```

## Outbound Request Örnekleri

### HTTP Client Throttling

```csharp
// Program.cs
builder.Services.AddHttpClient("ThrottledClient")
    .AddThrottling(options =>
    {
        options.RateLimit = 100; // dakikada maksimum istek sayısı
        options.BurstLimit = 10; // eşzamanlı maksimum istek sayısı
    });

// Kullanım
public class ExternalApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ExternalApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ApiResponse> GetDataAsync()
    {
        var client = _httpClientFactory.CreateClient("ThrottledClient");
        var response = await client.GetAsync("https://api.example.com/data");
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
```

### Custom Outbound Throttling Rule

```csharp
// Program.cs
builder.Services.AddRequestThrottler(options =>
{
    options.AddOutboundRule(new OutboundRateLimitRule
    {
        TargetHost = "api.example.com",
        RateLimit = 50,
        BurstLimit = 5,
        TimeWindow = TimeSpan.FromMinutes(1)
    });
});

// Kullanım
[ThrottleOutbound(TargetHost = "api.example.com", RateLimit = 30)]
public class ExternalService
{
    private readonly HttpClient _httpClient;

    public ExternalService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse> ProcessRequestAsync()
    {
        var response = await _httpClient.GetAsync("https://api.example.com/process");
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
```

### Different Limits for Different Targets

```csharp
builder.Services.AddRequestThrottler(options =>
{
    // Kritik API için daha yüksek limit
    options.AddOutboundRule(new OutboundRateLimitRule
    {
        TargetHost = "critical-api.example.com",
        RateLimit = 1000,
        BurstLimit = 100
    });

    // Normal API için daha düşük limit
    options.AddOutboundRule(new OutboundRateLimitRule
    {
        TargetHost = "normal-api.example.com",
        RateLimit = 100,
        BurstLimit = 10
    });
});
```

## Kullanım Senaryoları

### 1. API Rate Limiting
```csharp
// Program.cs
builder.Services.AddRequestThrottler(options =>
{
    // Tüm API'ler için varsayılan limit
    options.DefaultRateLimit = 100; // dakikada 100 istek
    options.DefaultBurstLimit = 10; // eşzamanlı 10 istek

    // Özel endpoint'ler için limitler
    options.AddRule(new RateLimitRule
    {
        Endpoint = "/api/auth/*",
        RateLimit = 30,  // Auth endpoint'leri için daha sıkı limit
        BurstLimit = 3
    });
});

// Controller'da kullanım
[ApiController]
[Route("api/[controller]")]
[Throttle(RateLimit = 50)] // Controller seviyesinde limit
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(products);
    }

    [HttpGet("{id}")]
    [Throttle(RateLimit = 20)] // Action seviyesinde özel limit
    public IActionResult GetById(int id)
    {
        return Ok(product);
    }
}
```

### 2. Dış API Entegrasyonları
```csharp
// Program.cs
builder.Services.AddHttpClient("ExternalApi")
    .AddThrottling(options =>
    {
        options.RateLimit = 60; // dakikada 60 istek
        options.BurstLimit = 5; // eşzamanlı 5 istek
    });

// Servis kullanımı
public class ExternalApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ExternalApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ApiResponse> CallExternalApiAsync()
    {
        var client = _httpClientFactory.CreateClient("ExternalApi");
        var response = await client.GetAsync("https://external-api.com/data");
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
```

### 3. Farklı Kullanıcı Tipleri için Farklı Limitler
```csharp
// Program.cs
builder.Services.AddRequestThrottler(options =>
{
    // Free kullanıcılar için limit
    options.AddRule(new RateLimitRule
    {
        Endpoint = "/api/free/*",
        RateLimit = 100,
        BurstLimit = 10
    });

    // Premium kullanıcılar için daha yüksek limit
    options.AddRule(new RateLimitRule
    {
        Endpoint = "/api/premium/*",
        RateLimit = 1000,
        BurstLimit = 100
    });
});
```

### 4. IP Bazlı Rate Limiting
```csharp
// Program.cs
builder.Services.AddRequestThrottler(options =>
{
    // IP bazlı rate limiting
    options.AddRule(new RateLimitRule
    {
        Endpoint = "*",
        RateLimit = 100,
        BurstLimit = 10,
        ApplyPerIp = true // IP bazlı uygula
    });
});
```

### 5. Özel Queue Implementation ile Dağıtık Sistemler
```csharp
// Redis tabanlı özel queue
public class RedisThrottleQueue : IThrottleQueue
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisThrottleQueue> _logger;

    public RedisThrottleQueue(IConnectionMultiplexer redis, ILogger<RedisThrottleQueue> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<bool> TryEnqueueAsync(string key, ThrottledRequest request)
    {
        var db = _redis.GetDatabase();
        var count = await db.StringIncrementAsync($"throttle:{key}");
        
        if (count > request.BurstLimit)
        {
            await db.StringDecrementAsync($"throttle:{key}");
            return false;
        }

        await db.KeyExpireAsync($"throttle:{key}", TimeSpan.FromMinutes(1));
        return true;
    }
}
```

## Contributing

Katkılarınızı bekliyoruz! Lütfen bir Pull Request göndermekten çekinmeyin.

## License

Bu proje MIT License altında lisanslanmıştır - detaylar için [LICENSE](LICENSE) dosyasına bakın.

---

# RequestThrottler (English)

[![NuGet](https://img.shields.io/nuget/v/RequestThrottler.svg)](https://www.nuget.org/packages/RequestThrottler)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A powerful request throttling and rate limiting library for ASP.NET Core applications.

## Feature List

### Core Features
- Request throttling and rate limiting
- In-memory request queue
- Fixed window rate limiting algorithm
- Multiple rate limiting strategies
- Concurrent request limiting

### Integration
- Easy ASP.NET Core integration
- Attribute-based configuration
- Middleware support
- DI container integration

### Configuration
- Global and endpoint-specific rules
- Configurable time windows
- Customizable rate limits
- Dynamic rule updates

### Advanced Features
- Custom queue implementations
- Multiple endpoint support
- Burst limit protection
- Request tracking and monitoring

### Performance
- High-performance implementation
- Memory-efficient design
- Thread-safe operations
- Scalable architecture

## Future Plans

### Short Term (Next Release)
- Sliding Window rate limiting algorithm
- Request metrics and analytics
- IP-based rate limiting
- User-based rate limiting
- Enhanced logging capabilities

### Medium Term
- Distributed caching support (Redis)
- Token Bucket algorithm implementation
- Performance optimizations
- Custom rule providers
- Advanced request filtering

### Long Term
- Multi-tenant support
- Real-time monitoring dashboard
- Adaptive rate limiting
- DDoS protection features
- Mobile SDK support

## Features

- Request throttling and rate limiting
- In-memory request queue
- Fixed window rate limiting algorithm
- Middleware and attribute-based configuration
- Easy integration with ASP.NET Core
- Configurable rules and limits

## Installation

```bash
dotnet add package RequestThrottler
```

## Quick Start

### Basic Configuration

```csharp
// Program.cs
builder.Services.AddRequestThrottler(options =>
{
    options.DefaultRateLimit = 100; // requests per minute
    options.DefaultBurstLimit = 10; // concurrent requests
});
```

### Using Middleware

```csharp
// Program.cs
app.UseRequestThrottler();
```

### Using Attribute

```csharp
[Throttle(RateLimit = 50, BurstLimit = 5)]
public class MyController : ControllerBase
{
    [Throttle(RateLimit = 20)]
    public IActionResult Get()
    {
        return Ok();
    }
}
```

## Advanced Configuration

### Custom Rate Limiting Rules

```csharp
builder.Services.AddRequestThrottler(options =>
{
    options.AddRule(new RateLimitRule
    {
        Endpoint = "/api/endpoint",
        RateLimit = 30,
        BurstLimit = 5,
        TimeWindow = TimeSpan.FromMinutes(1)
    });
});
```

### Custom Queue Implementation

```csharp
public class CustomThrottleQueue : IThrottleQueue
{
    private readonly ConcurrentDictionary<string, Queue<ThrottledRequest>> _queues;
    private readonly ILogger<CustomThrottleQueue> _logger;

    public CustomThrottleQueue(ILogger<CustomThrottleQueue> logger)
    {
        _queues = new ConcurrentDictionary<string, Queue<ThrottledRequest>>();
        _logger = logger;
    }

    public async Task<bool> TryEnqueueAsync(string key, ThrottledRequest request)
    {
        var queue = _queues.GetOrAdd(key, _ => new Queue<ThrottledRequest>());
        
        lock (queue)
        {
            if (queue.Count >= request.BurstLimit)
            {
                _logger.LogWarning($"Queue limit exceeded: {key}");
                return false;
            }

            queue.Enqueue(request);
            _logger.LogInformation($"Request enqueued: {key}");
            return true;
        }
    }

    public async Task<ThrottledRequest?> DequeueAsync(string key)
    {
        if (!_queues.TryGetValue(key, out var queue))
            return null;

        lock (queue)
        {
            if (queue.Count == 0)
                return null;

            var request = queue.Dequeue();
            _logger.LogInformation($"Request dequeued: {key}");
            return request;
        }
    }

    public async Task<int> GetQueueLengthAsync(string key)
    {
        return _queues.TryGetValue(key, out var queue) ? queue.Count : 0;
    }
}

// Registration
builder.Services.AddSingleton<IThrottleQueue, CustomThrottleQueue>();
```

## Usage Scenarios

### 1. API Rate Limiting
```csharp
// Program.cs
builder.Services.AddRequestThrottler(options =>
{
    // Default limits for all APIs
    options.DefaultRateLimit = 100; // 100 requests per minute
    options.DefaultBurstLimit = 10; // 10 concurrent requests

    // Custom limits for specific endpoints
    options.AddRule(new RateLimitRule
    {
        Endpoint = "/api/auth/*",
        RateLimit = 30,  // Stricter limits for auth endpoints
        BurstLimit = 3
    });
});

// Controller usage
[ApiController]
[Route("api/[controller]")]
[Throttle(RateLimit = 50)] // Controller level limit
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(products);
    }

    [HttpGet("{id}")]
    [Throttle(RateLimit = 20)] // Action level specific limit
    public IActionResult GetById(int id)
    {
        return Ok(product);
    }
}
```

### 2. External API Integrations
```csharp
// Program.cs
builder.Services.AddHttpClient("ExternalApi")
    .AddThrottling(options =>
    {
        options.RateLimit = 60; // 60 requests per minute
        options.BurstLimit = 5; // 5 concurrent requests
    });

// Service usage
public class ExternalApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ExternalApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ApiResponse> CallExternalApiAsync()
    {
        var client = _httpClientFactory.CreateClient("ExternalApi");
        var response = await client.GetAsync("https://external-api.com/data");
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
```

### 3. Different Limits for Different User Types
```csharp
// Program.cs
builder.Services.AddRequestThrottler(options =>
{
    // Limits for free users
    options.AddRule(new RateLimitRule
    {
        Endpoint = "/api/free/*",
        RateLimit = 100,
        BurstLimit = 10
    });

    // Higher limits for premium users
    options.AddRule(new RateLimitRule
    {
        Endpoint = "/api/premium/*",
        RateLimit = 1000,
        BurstLimit = 100
    });
});
```

### 4. IP-Based Rate Limiting
```csharp
// Program.cs
builder.Services.AddRequestThrottler(options =>
{
    // IP-based rate limiting
    options.AddRule(new RateLimitRule
    {
        Endpoint = "*",
        RateLimit = 100,
        BurstLimit = 10,
        ApplyPerIp = true // Apply per IP
    });
});
```

### 5. Custom Queue Implementation for Distributed Systems
```csharp
// Redis-based custom queue
public class RedisThrottleQueue : IThrottleQueue
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisThrottleQueue> _logger;

    public RedisThrottleQueue(IConnectionMultiplexer redis, ILogger<RedisThrottleQueue> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<bool> TryEnqueueAsync(string key, ThrottledRequest request)
    {
        var db = _redis.GetDatabase();
        var count = await db.StringIncrementAsync($"throttle:{key}");
        
        if (count > request.BurstLimit)
        {
            await db.StringDecrementAsync($"throttle:{key}");
            return false;
        }

        await db.KeyExpireAsync($"throttle:{key}", TimeSpan.FromMinutes(1));
        return true;
    }
}
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details. 