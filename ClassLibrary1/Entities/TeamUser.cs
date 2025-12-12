using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Domain.Entities
{
    public class TeamUser
    {
        public  Guid UserId { get; set; }
        public User User { get; set; } = default!;

        public Guid TeamId { get; set; }
        public Team Team { get; set; } = default!;
    }
}
