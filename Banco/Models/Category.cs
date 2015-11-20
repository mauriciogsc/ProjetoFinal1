﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banco.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string SquareId { get; set; }
        public string Name { get; set; }
        public List<Venue> Venues { get; set; }
    }
}