using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banco.Models
{
    public class Tip
    {
        public int Id { get; set; }
        public string SquareId { get; set; }
        public string Description { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int VenueId { get; set; }
        public virtual Venue Venue { get; set; }
    }
}
