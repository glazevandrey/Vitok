using Microsoft.EntityFrameworkCore ;
using web_server.Models;
using web_server.Models.DBModels;

namespace web_server.Database
{
    public class DataContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<BalanceHistory> BalanceHistories { get; set; }
        public DbSet<CashFlow> CashFlows { get; set; }
        public DbSet<ChatUser> ChatUsers { get; set; }

        public DbSet<ConnectionToken>  ConnectionTokens { get; set; }
        public DbSet<Contact>  Contacts{ get; set; }
        public DbSet<Course>  Courses{ get; set; }
        public DbSet<UserDate> UserDates { get; set; }
        public DbSet<Goal> Goals{ get; set; }
        public DbSet<InChat> InChats{ get; set; }
        public DbSet<LessonHistory> LessonHistories{ get; set; }
        public DbSet<Messages> Messages{ get; set; }
        public DbSet<Notifications> Notifications{ get; set; }

        public DbSet<NotificationTask> NotificationTasks { get; set; }
        public DbSet<NotificationTokens> NotificationTokens{ get; set; }
        public DbSet<PaidLesson> PaidLessons{ get; set; }
        public DbSet<Registration> Registrations{ get; set; }
        public DbSet<RescheduledLessons> RescheduledLessons{ get; set; }
        public DbSet<Schedule> Schedules{ get; set; }
        public DbSet<SiteContact> SiteContacts{ get; set; }
        public DbSet<SkippedDate> SkippedDates{ get; set; }
        public DbSet<StudentPayment> StudentPayments{ get; set; }
        public DbSet<Tariff> Tariffs{ get; set; }


        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
            Database.EnsureCreated();   // создаем базу данных при первом обращении
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<User>().HasData(
            //        new User { Id = 1, Name = "Tom", Age = 37 },
            //        new User { Id = 2, Name = "Bob", Age = 41 },
            //        new User { Id = 3, Name = "Sam", Age = 24 }
            //);
        }
    }
}
