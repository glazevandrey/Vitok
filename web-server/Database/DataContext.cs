using Microsoft.EntityFrameworkCore ;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.Linq;
using web_server.DbContext;
using web_server.Models;
using web_server.Models.DBModels;
using web_server.Models.DTO;

namespace web_server.Database
{
    public class DataContext : Microsoft.EntityFrameworkCore.DbContext
    {
  

       // public DbSet<BalanceHistory> BalanceHistories { get; set; }
        //public DbSet<CashFlow> CashFlows { get; set; }
        public DbSet<ChatUser> ChatUsers { get; set; }

        //public DbSet<ConnectionToken>  ConnectionTokens { get; set; }
        public DbSet<Contact>  Contacts{ get; set; }
        public DbSet<Course>  Courses{ get; set; }
        //public DbSet<UserDate> UserDates { get; set; }
        public DbSet<Goal> Goals{ get; set; }
        //public DbSet<InChat> InChats{ get; set; }
        //public DbSet<Messages> Messages{ get; set; }
        public DbSet<Notifications> Notifications{ get; set; }

        //public DbSet<NotificationTask> NotificationTasks { get; set; }
        //public DbSet<NotificationTokens> NotificationTokens{ get; set; }
        //public DbSet<PaidLesson> PaidLessons{ get; set; }
        public DbSet<Registration> Registrations{ get; set; }
        public DbSet<RescheduledLessons> RescheduledLessons{ get; set; }
        public DbSet<Schedule> Schedules{ get; set; }
        //public DbSet<SiteContact> SiteContacts{ get; set; }
        //public DbSet<SkippedDate> SkippedDates{ get; set; }
        //public DbSet<StudentPayment> StudentPayments{ get; set; }
        public DbSet<Tariff> Tariffs{ get; set; }
        public DbSet<UserDTO> Users { get; set; }

        public DbSet<StudentDTO> Students { get; set; }
        public DbSet<ManagerDTO> Managers { get; set; }

        public DbSet<TutorDTO> Tutors { get; set; }



        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Goal>();
            modelBuilder.Entity<TutorDTO>().ToTable("Tutors");
            modelBuilder.Entity<ManagerDTO>().ToTable("Managers");
            modelBuilder.Entity<StudentDTO>().ToTable("Students");
            modelBuilder.Entity<Goal>().HasData(TestData.Goals);
            modelBuilder.Entity<Course>().HasData(TestData.Courses) ;
            modelBuilder.Entity<Schedule>().HasData(TestData.Schedules);
            modelBuilder.Entity<Tariff>().HasData(TestData.Tariffs);
            var tutors = (TestData.UserList.Where(m => m.Role == "Tutor").ToList());
            modelBuilder.Entity<TutorDTO>().HasData(tutors);

            var students = TestData.UserList.Where(m => m.Role == "Student");
            modelBuilder.Entity<StudentDTO>().HasData(students);

            var manager = TestData.UserList.FirstOrDefault(m => m.Role == "Manager");
            modelBuilder.Entity<TutorDTO>().HasData(manager);


            modelBuilder.Entity<StudentDTO>()
        .HasMany(s => s.Money)
        .WithOne()
        .HasForeignKey("StudentDTOUserId"); // Указание имени внешнего ключа

            // Конфигурация отношения между StudentDTO и UserCredit
            modelBuilder.Entity<StudentDTO>()
                .HasMany(s => s.Credit)
                .WithOne()
                .HasForeignKey("StudentDTOUserId"); // Указание имени внешнего ключа

           


        }
    }
}
