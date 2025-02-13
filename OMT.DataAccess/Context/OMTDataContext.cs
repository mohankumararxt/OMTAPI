using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
        public DbSet<Utilization> Utilization { get; set; }
        public DbSet<UserInterviews> UserInterviews { get; set; }
        public DbSet<Tests> Tests { get; set; }

        public DbSet<InterviewTests> InterviewTests { get; set; }
        public DbSet<UserTest> UserTest { get; set; }

        public DbSet<BroadCastAnnouncement> BroadCastAnnouncement { get; set; }

        public DbSet<Notification> Notification { get; set; }
        public DbSet<Tasks> Tasks { get; set; }
        public DbSet<TaskHistory> TaskHistory { get; set; }
        public DbSet<TasksStatus> TasksStatus { get; set; }
        public DbSet<TaskPriority> TaskPriority { get; set; }
        public DbSet<ActivityFeed>  ActivityFeeds { get; set; }

        public DbSet<Message> Message { get; set; }
        public DbSet<MessagesStatus> MessagesStatus { get; set; }


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


        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    // Ensure DateOfBirth is treated as a DATE in the database
        //    modelBuilder.Entity<UserInterviews>()
        //        .Property(u => u.DOB)
        //        .HasColumnType("date");
        //}

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    // Value converter to map DateOnly to DateTime
        //    var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
        //        dateOnly => dateOnly.ToDateTime(new TimeOnly(0, 0)), // Convert DateOnly to DateTime
        //        dateTime => DateOnly.FromDateTime(dateTime) // Convert DateTime back to DateOnly
        //    );

        //    modelBuilder.Entity<UserInterviews>()
        //        .Property(u => u.DOB)
        //        .HasConversion(dateOnlyConverter);
        //}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserInterviews>()
                .Property(u => u.CreateTimestamp)
                .ValueGeneratedOnAdd(); // Use database default value

            modelBuilder.Entity<Tests>()
               .Property(p => p.CreateTimestamp)
               .ValueGeneratedOnAdd();
            modelBuilder.Entity<InterviewTests>()
                .Property(p => p.CreateTimestamp)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<UserTest>()
                .Property(p => p.CreateTimestamp) 
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<BroadCastAnnouncement>()
                .Property(p => p.StartDateTime)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<BroadCastAnnouncement>()
                .Property(p => p.EndDateTime)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<BroadCastAnnouncement>()
                .Property(p => p.CreateTimeStamp)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Notification>()
                .Property(p => p.CreateTimeStamp)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Message>()
                .Property(p => p.CreateTimeStamp)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Tasks>()
           .Property(t => t.Id)
           .ValueGeneratedOnAdd();

            modelBuilder.Entity<TaskHistory>()
                .Property(th => th.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<TasksStatus>()
                .Property(ts => ts.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<TaskPriority>()
                .Property(tp => tp.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<ActivityFeed>()
                .Property(af => af.Id)
                .ValueGeneratedOnAdd();
        }

        //protected override void OnModelCreatingTest(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<UserInterviews>()
        //        .Property(u => u.CreateTimestamp)
        //        .ValueGeneratedOnAddOrUpdate(); // Use database default value
        //}
    }
}
