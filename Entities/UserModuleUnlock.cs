using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningPlat.Entities
{
    public class UserModuleUnlock
    {
        public int UserId { get; set; }
        public int ModuleId { get; set; }
        public DateTime UnlockedAt { get; set; }
    }
}
