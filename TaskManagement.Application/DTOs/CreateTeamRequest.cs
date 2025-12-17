using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Application.DTOs
{
    public class CreateTeamRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
    }
}
