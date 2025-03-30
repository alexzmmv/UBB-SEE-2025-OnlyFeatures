using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningPlat.Entities
{
    public class PictureCoin
    {
        public int UserId { get; set; }
        public int PictureId { get; set; }
        public decimal CoinsReceived { get; set; }
    }

}
