using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Application.DTOs.Responses
{
    public class TeamMemberResponse
    {
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
    }
}
