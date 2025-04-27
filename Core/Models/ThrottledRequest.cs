namespace RequestThrottler.Core.Models
{
    public class ThrottledRequest
    {
        public Func<Task> Action { get; }

        public ThrottledRequest(Func<Task> action)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
        }
    }
}
