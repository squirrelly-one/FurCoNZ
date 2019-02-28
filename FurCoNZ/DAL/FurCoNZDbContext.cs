﻿using FurCoNZ.Models;
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
    }
}
