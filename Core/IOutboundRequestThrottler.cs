  public interface IOutboundRequestThrottler
    {
        Task WaitBeforeSendAsync();
    }