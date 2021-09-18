using CreatureDisplayCreator.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreatureDisplayCreator.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<CreatureModelInfo> CreatureModelInfos { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CreatureModelInfo>()
                .ToTable("creature_model_info")
                .HasKey(q => q.DisplayId);
        }
    }
}
