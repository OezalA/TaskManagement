using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Application.DTOs.Responses
{
    public class TeamResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
