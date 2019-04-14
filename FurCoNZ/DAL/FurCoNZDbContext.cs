using FurCoNZ.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurCoNZ.DAL
{
    public class FurCoNZDbContext : DbContext
    {
        public FurCoNZDbContext(DbContextOptions<FurCoNZDbContext> options) 
            : base(options)
        { }

        public DbSet<User> Users { get; set; }
        public DbSet<LinkedAccount> LinkedAccounts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LinkedAccount>()
                .HasIndex(l => new { l.Issuer, l.Subject });
        }
    }
}
