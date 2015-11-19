using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banco.Models
{
    public class BancoContext: DbContext
    {
        public DbSet<User> Users { get; set; } 
        public DbSet<Venue> Venues { get; set; } 
        public DbSet<Tip> Tips { get; set; } 
    }
}
