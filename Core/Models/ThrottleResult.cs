namespace RequestThrottler.Core.Models
{
    public class ThrottleResult
    {
        public bool IsAllowed { get; }
        public int Remaining { get; }
        public int RetryAfterSeconds { get; }
        public string Message { get; }

        public ThrottleResult(bool isAllowed, int remaining, int retryAfterSeconds, string message)
        {
            IsAllowed = isAllowed;
            Remaining = remaining;
            RetryAfterSeconds = retryAfterSeconds;
            Message = message;
        }
    }
}
