# AdvancedRequestThrottler

**AdvancedRequestThrottler**, .NET uygulamalarında istek hızını sınırlamak ve yönetmek için geliştirilmiş bir kütüphanelidir. Hız sınırlandırma algoritmaları ve kuyruklama sistemi ile API trafiğini kontrol etmeyi kolaylaştırır.

---

## Özellikler

- **Hız Sınırlandırma Algoritmaları**
  - Sabit Pencere (Fixed Window) algoritması
  - Thread-safe hız sınırlandırma mekanizmaları
- **Kuyruklama Sistemi**
  - İş yükü yönetimi için bellek içi kuyruklama
- **Kolay Entegrasyon**
  - `IServiceCollection` ve `IHttpClientBuilder` uzantıları ile hızlı kurulum
  - Esnek hız sınırlayıcı ve kuyruk konfigürasyonu
- **Arka Plan Hizmeti**
  - Kuyruktaki işlemleri arka planda otomatik tüketme

---

## Kurulum

NuGet paketleri üzerinden gelecektir. şu anda manuel ekleyerek kullanabilirsiniz.

---

## Kullanım

### 1. Hız Sınırlandırma Mekanizması Ekleme

```csharp
services.AddRequestThrottling(options =>
{
    options.Limit = 10; // Maksimum 10 istek
    options.Window = TimeSpan.FromSeconds(60); // 60 saniyelik zaman penceresi
});
```

### 2. HttpClient ile Throttling Kullanımı

```csharp
services.AddHttpClient("ThrottledClient")
    .AddThrottling(options =>
    {
        options.Limit = 5; // Maksimum 5 istek
        options.Window = TimeSpan.FromSeconds(30); // 30 saniyelik zaman penceresi
    });
```

### 3. Kuyruk Tüketimi

```csharp
public class MyService
{
    private readonly IInMemoryThrottleQueue _queue;

    public MyService(IInMemoryThrottleQueue queue)
    {
        _queue = queue;
    }

    public async Task AddToQueueAsync(Func<Task> action)
    {
        await _queue.EnqueueAsync(action);
    }
}
```

---

## Mimari

- **RateLimiter**  
  İsteklerin hız sınırlandırmasını kontrol eder.  
  Örnek: `FixedWindowRateLimiter`

- **ThrottleQueue**  
  İstekleri kuyruklar ve sıralı şekilde işler.  
  Örnek: `InMemoryThrottleQueue`

- **Extensions**  
  Hız sınırlayıcı ve kuyruk mekanizmalarını kolayca entegre etmek için uzantılar sunar.

- **HostedService**  
  Kuyruktaki işlemleri arka planda tüketen servis: `QueueConsumerHostedService`

---

## Katkıda Bulunma

Katkıda bulunmak için:

1. Bu repo'yu fork edin.
2. Yeni bir branch oluşturun:  
   ```bash
   git checkout -b my-feature-branch
   ```
3. Değişikliklerinizi yapın ve commit edin:  
   ```bash
   git commit -m 'Yeni bir özellik ekle'
   ```
4. Branch'inizi pushlayın:  
   ```bash
   git push origin my-feature-branch
   ```
5. Pull Request oluşturun.

---

## Lisans

Bu proje [MIT Lisansı](LICENSE) ile lisanslanmıştır.

---

# AdvancedRequestThrottler (EN)

AdvancedRequestThrottler is a .NET library designed to manage and limit request rates within applications. It simplifies API traffic control with rate-limiting mechanisms and a queueing system.

---

## Features

- **Rate Limiting Algorithms**
  - Fixed Window algorithm
  - Thread-safe rate-limiting mechanisms
- **Queueing System**
  - In-memory queue management
- **Easy Integration**
  - Simple setup with `IServiceCollection` and `IHttpClientBuilder`
- **Background Service**
  - Automatic queue consumption with a hosted service

---

## Installation

Coming soon on NuGet.  
Currently, you can clone and reference manually.

---

## Usage

### 1. Add Rate Limiting Mechanism

```csharp
services.AddRequestThrottling(options =>
{
    options.Limit = 10; // Maximum 10 requests
    options.Window = TimeSpan.FromSeconds(60); // 60-second time window
});
```

### 2. Use Throttling with HttpClient

```csharp
services.AddHttpClient("ThrottledClient")
    .AddThrottling(options =>
    {
        options.Limit = 5; // Maximum 5 requests
        options.Window = TimeSpan.FromSeconds(30); // 30-second time window
    });
```

### 3. Queue Consumption

```csharp
public class MyService
{
    private readonly IInMemoryThrottleQueue _queue;

    public MyService(IInMemoryThrottleQueue queue)
    {
        _queue = queue;
    }

    public async Task AddToQueueAsync(Func<Task> action)
    {
        await _queue.EnqueueAsync(action);
    }
}
```

---

## Architecture

- **RateLimiter**  
  Controls the rate limiting of requests.  
  Example: `FixedWindowRateLimiter`

- **ThrottleQueue**  
  Queues requests and processes them sequentially.  
  Example: `InMemoryThrottleQueue`

- **Extensions**  
  Provides extension methods for easy integration.

- **HostedService**  
  Consumes tasks in the queue using `QueueConsumerHostedService`.

---

## Contribution

1. Fork this repository.
2. Create a new branch:  
   ```bash
   git checkout -b my-feature-branch
   ```
3. Make your changes and commit:  
   ```bash
   git commit -m 'Add some feature'
   ```
4. Push your branch:  
   ```bash
   git push origin my-feature-branch
   ```
5. Open a Pull Request.

---

## License

This project is licensed under the [MIT License](LICENSE).
