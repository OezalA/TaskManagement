using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Application.DTOs.Responses
{
    public class UserTaskResponse
    {
        public Guid TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; }
    }
}
