using ChatPlatform.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Infrastructure.Configurations
{
    internal class ChatRoomMemberConfigurations : IEntityTypeConfiguration<ChatRoomMember>
    {
        public void Configure(EntityTypeBuilder<ChatRoomMember> builder)
        {
            builder.HasOne(m=>m.User).WithMany()
                .HasForeignKey(m=>m.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.ChatRoom).WithMany(cr=>cr.Members)
                .HasForeignKey(m=>m.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
               .HasIndex(m => new { m.ChatRoomId, m.UserId })
               .IsUnique();

            builder
                .HasIndex(m => m.UserId);
        }
    }
}
