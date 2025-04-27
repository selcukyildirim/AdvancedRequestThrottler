namespace RequestThrottler.Core.Models
{
    public class RateLimitRule
    {
        public int Limit { get; }
        public int WindowInSeconds { get; }
        public string EndpointPattern { get; set; } = "*";
        public string ClientIdPattern { get; set; } = "*";
        public string LimitExceededMessage { get; set; } = "Too many requests.";

        public RateLimitRule(int limit, int windowInSeconds)
        {
            if (limit <= 0) throw new ArgumentOutOfRangeException(nameof(limit));
            if (windowInSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(windowInSeconds));

            Limit = limit;
            WindowInSeconds = windowInSeconds;
        }
    }
}
