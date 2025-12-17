using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Application.DTOs.Responses
{
    public class UserProjectResponse
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
    }
}
