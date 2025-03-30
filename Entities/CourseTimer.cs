using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningPlat.Entities
{
    public class CourseTimer
    {
        public int TimerId { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public int ElapsedTimeMinutes { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsCompleted { get; set; }
    }
}
