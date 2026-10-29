using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace ChatPlatform.Api.RealTime
{
    public class ConnectionManager
    {
        private readonly ConcurrentDictionary<Guid, ConcurrentBag<ManagedSocket>> _connections = new();

        public void AddConnection(Guid userId, WebSocket socket)
        {
            ManagedSocket managedSocket = new ManagedSocket(socket);
            ConcurrentBag<ManagedSocket> bag = _connections.GetOrAdd(userId, new ConcurrentBag<ManagedSocket>());
            bag.Add(managedSocket);
        }

        public void RemoveConnection(Guid userId, WebSocket socket)
        {
            if(_connections.TryGetValue(userId, out var bag))
            {
                var remaining = bag.Where(ms => ms.Socket != socket);
                _connections[userId] = new ConcurrentBag<ManagedSocket>(remaining);
            }
        }

        public List<ManagedSocket> GetConnections(Guid userId)
        {
            return _connections.TryGetValue(userId, out var bag) ? bag.ToList() : new List<ManagedSocket>();
        }
    }
}
