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
        public DbSet<BusinessGroup> BusinessGroup { get; set; }
        public DbSet<ProductDescription> ProductDescription { get; set; }
        public DbSet<ResWareProductDescriptions> ResWareProductDescriptions { get; set; }
        public DbSet<ResWareProductDescriptionMap> ResWareProductDescriptionMap { get; set; }
        public DbSet<ProcessType> ProcessType { get; set; }
        public DbSet<SourceType> SourceType { get; set; }
        public DbSet<Business> Business { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Workflowstatus> Workflowstatus { get; set; }
        public DbSet<TotalOrderFees> TotalOrderFees { get; set; }
        public DbSet<InvoiceJointResware> InvoiceJointResware { get; set; }
        public DbSet<InvoiceJointSci> InvoiceJointSci { get; set; }
        public DbSet<CostCenter> CostCenter { get; set; }
        public DbSet<InvoiceDump> InvoiceDump { get; set; }
        public DbSet<Timeline> Timeline { get; set; }
        public DbSet<InvoiceSkillSet> InvoiceSkillSet { get; set; }
        public DbSet<Keywordstable> Keywordstable { get; set; }
        public DbSet<LiveReportTiming> LiveReportTiming { get; set; }
        public DbSet<ReportColumns> ReportColumns { get; set; }
        public DbSet<DocType> DocType { get; set; }
        public DbSet<TrdMap> TrdMap { get; set; }
        public DbSet<TrackTrdOrders> TrackTrdOrders { get; set; }
        public DbSet<MasterReportColumns> MasterReportColumns { get; set; }
        public DbSet<SciException> SciException { get; set; }
        public DbSet<GetOrderCalculation> GetOrderCalculation { get; set; }
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
