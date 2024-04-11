using Microsoft.EntityFrameworkCore;
using OMT.DataAccess.Entities;

namespace OMT.DataAccess.Context
{
    public class OMTDataContext : DbContext
    {
        public DbSet<Organization> Organization { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Teams> Teams { get; set; }
        public DbSet<TeamAssociation> TeamAssociation { get; set; }
        public DbSet<UserProfile> UserProfile { get; set; }
        public DbSet<SkillSet> SkillSet { get; set; }
        public DbSet<UserSkillSet> UserSkillSet { get; set; }
        public DbSet<Template> Template { get; set; }
        public DbSet<TemplateColumns> TemplateColumns { get; set; }
        public DbSet<SystemofRecord> SystemofRecord { get; set; }
        public DbSet<ProcessStatus> ProcessStatus { get; set; } 
        public DbSet<SkillSetHardStates> SkillSetHardStates { get; set; }
        public DbSet<DefaultTemplateColumns> DefaultTemplateColumns { get; set; }
        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        public void Update<T>(T entity) where T : class
        {
            Entry(entity).State = EntityState.Modified;
        }

        public OMTDataContext(Microsoft.EntityFrameworkCore.DbContextOptions options)
            : base(options)
        { }

        public OMTDataContext(string connectionString) : this(GetOptions(connectionString))
        {
        }

        private static Microsoft.EntityFrameworkCore.DbContextOptions GetOptions(string connectionString)
        {
            return SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder(), connectionString).Options;
        }

    }
}
