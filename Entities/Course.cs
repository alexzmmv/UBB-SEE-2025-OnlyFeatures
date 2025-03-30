using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningPlat.Entities
{
    public class Course
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int TypeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int DifficultyLevel { get; set; }
        public int TimerDurationMinutes { get; set; }
        public decimal TimerCompletionReward { get; set; }
        public decimal CompletionReward { get; set; }
    }
}
