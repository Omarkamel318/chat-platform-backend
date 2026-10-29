using System.Net.WebSockets;

namespace ChatPlatform.Api.RealTime
{
    public class ManagedSocket
    {
        public WebSocket Socket { get; }
        public SemaphoreSlim SendLock { get;} = new SemaphoreSlim(1, 1);

        public ManagedSocket(WebSocket socket)
        {
            Socket = socket;
        }
    }
}
