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
        public DbSet<OrderAudit> OrderAudits { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }

        public DbSet<StripeSession> StripeSessions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LinkedAccount>()
                .HasIndex(l => new { l.Issuer, l.Subject });

            // Disable autogeneration of primary key.
            modelBuilder.Entity<StripeSession>()
                .Property(ss => ss.Id)
                .ValueGeneratedNever(); 

            // Treat PaymentIntent as an index for lookup.
            modelBuilder.Entity<StripeSession>()
                .HasIndex(ss => ss.PaymentIntent);

            modelBuilder.Entity<OrderAudit>()
                .HasIndex(a => new { a.PaymentProvider, a.PaymentProviderReference });

        }
    }
}
