using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banco.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SquareId { get; set; }
        public string Sexo { get; set; }
        public int countAmigos { get; set; }
        public int countCheckin { get; set; }
        public int countTip { get; set; }
        public float weight { get; set; }
        public double mediaComentarios { get; set; }
        public string cidadeNatal { get; set; }
    }
}
