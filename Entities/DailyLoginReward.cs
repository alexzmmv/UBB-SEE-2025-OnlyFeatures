using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningPlat.Entities
{
    public class DailyLoginReward
    {
        public int RewardId { get; set; }
        public int UserId { get; set; }
        public DateTime RewardDate { get; set; }
        public decimal CoinsReceived { get; set; }
    }

}
