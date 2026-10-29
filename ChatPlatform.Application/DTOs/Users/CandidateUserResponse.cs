using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatPlatform.Application.DTOs.Users
{
    public class CandidateUserResponse
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public bool IsCandidate { get; set; } 
    }
}
