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
        public int WekaPredict { get; set; }
        public int WekaPredictFinal { get; set; }
        public virtual User User { get; set; }
        public DateTime UpdateDate { get; set; }
        public int VenueId { get; set; }
        public virtual Venue Venue { get; set; }
        public int status { get; set; }
        public float AlchemyScore { get; set; }
        public int AlchemyPredict { get; set; }
        public int AlchemyMixed { get; set; }
    }
}
