using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningPlat.Entities
{
    public class UserProgress
    {
        public int ProgressId { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
        public string Status { get; set; } // Changed from ProgressPercentage to Status
        public DateTime LastUpdated { get; set; }
    }
}
