using ChatPlatform.Application.Models;
using ChatPlatform.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.IRepositories
{
    public interface IMessageRepository
    {
        Task AddAsync(Message message);
        Task<IReadOnlyList<Message>> GetRoomMessagesAsync(Guid chatRoomId, MessageVisibility visibility, int skip, int take);
        Task<IReadOnlyList<Message>> GetUnReadMessagesForUserAsync(Guid chatRoomId, Guid userId);
    }
}
