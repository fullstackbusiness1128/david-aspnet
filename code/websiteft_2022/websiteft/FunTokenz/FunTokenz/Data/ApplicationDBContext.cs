using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FunTokenz.Models.Data;
using System.Reflection.Emit;

namespace FunTokenz.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        public DbSet<FTDebits> FTDebits { get; set; }
        public DbSet<FTPurchases> FTPurchases { get; set; }
        public DbSet<FTReseller> FTReseller { get; set; }
        public DbSet<ResellerCustomers> ResellerCustomers { get; set; }
        public DbSet<ExchangeRate> ExchangeRate { get; set; }

        public DbSet<DBFiles> DBFiles { get; set; }

        public DbQuery<Transactions> Transactions { get; set; }  

        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Query<Transactions>().ToView("Transactions");
            base.OnModelCreating(builder);
        }
    }
}
