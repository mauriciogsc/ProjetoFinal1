using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banco.Models
{
    public class Venue
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SquareId { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public List<Category> Categories { get; set; }
        public bool HasMenu { get; set; }
        public int checkincount { get; set; }
        public int tipcount { get; set; }
        public double rateWeka { get; set; }
        public double rate { get; set; }
        public int price { get; set; }
        public DateTime updated { get; set; }
        public string tier { get; set; }
        public int likes { get; set; }
    }
}
