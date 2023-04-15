using Microsoft.EntityFrameworkCore ;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.Linq;
using web_server.DbContext;
using web_server.Models;
using web_server.Models.DBModels;
using web_server.Models.DTO;
using web_server.Models.DTO;

namespace web_server.Database
{
    public class DataContext : Microsoft.EntityFrameworkCore.DbContext
    {
  

       // public DbSet<BalanceHistory> BalanceHistories { get; set; }
        //public DbSet<CashFlow> CashFlows { get; set; }
        //public DbSet<Chat> Chats { get; set; }

        public DbSet<TutorCourse> TutorCourses { get; set; }
        //public DbSet<ConnectionToken>  ConnectionTokens { get; set; }
        public DbSet<Contact>  Contacts{ get; set; }
        public DbSet<CourseDTO>  Courses{ get; set; }
        public DbSet<ChatDTO> Chats{ get; set; }
        public DbSet<UserDate> UserDates { get; set; }
        public DbSet<GoalDTO> Goals{ get; set; }
        //public DbSet<InChat> InChats{ get; set; }
        //public DbSet<Messages> Messages{ get; set; }
        public DbSet<NotificationsDTO> Notifications{ get; set; }

       // public DbSet<NotificationTask> NotificationTasks { get; set; }
        //public DbSet<NotificationTokens> NotificationTokens{ get; set; }
        //public DbSet<PaidLesson> PaidLessons{ get; set; }
        public DbSet<RegistrationDTO> Registrations{ get; set; }
        public DbSet<RescheduledLessons> RescheduledLessons{ get; set; }
        public DbSet<ScheduleDTO> Schedules { get; set; }
        
        public DbSet<SiteContact> SiteContacts{ get; set; }
        
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
            modelBuilder.Entity<TutorDTO>().ToTable("Tutors");
            modelBuilder.Entity<ManagerDTO>().ToTable("Managers");
            modelBuilder.Entity<StudentDTO>().ToTable("Students");


            modelBuilder.Entity<GoalDTO>()
              .HasMany(pt => pt.Courses)
              .WithOne(t => t.Goal)
              .HasForeignKey(pt => pt.GoalId)
              .OnDelete(DeleteBehavior.Restrict); // Устанавл


            modelBuilder.Entity<TutorDTO>()
            .HasMany(pt => pt.Courses)
            .WithOne(t => t.Tutor)
            .HasForeignKey(pt => pt.TutorId)
            .OnDelete(DeleteBehavior.Restrict); // Устанавл


            modelBuilder.Entity<UserDTO>()
            .HasOne(pt => pt.Chat)
            .WithOne(t => t.User)
            .HasForeignKey<ChatDTO>(pt => pt.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Устанавливаем DeleteBehavior.Restrict, чтобы не было конфликтов с внешним ключом

            modelBuilder.Entity<NotificationsDTO>()
            .HasOne(pt => pt.User)
            .WithMany(t => t.Notifications)
            .HasForeignKey(pt => pt.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Устанавливаем DeleteBehavior.Restrict, чтобы не было конфликтов с внешним ключом





            modelBuilder.Entity<ScheduleDTO>()
            .HasMany(pt => pt.Tasks)
            .WithOne(t => t.Schedule)
            .HasForeignKey(pt => pt.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict); // Устанавливаем DeleteBehavior.Restrict, чтобы не было конфликтов с внешним ключом

            modelBuilder.Entity<StudentDTO>()
            .HasMany(pt => pt.Schedules)
            .WithOne(t => t.Student)
            .HasForeignKey(pt => pt.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Устанавливаем DeleteBehavior.Restrict, чтобы не было конфликтов с внешним ключом

            modelBuilder.Entity<TutorDTO>()
            .HasMany(pt => pt.Schedules)
            .WithOne(t => t.Tutor)
            .HasForeignKey(pt => pt.TutorId)
            .OnDelete(DeleteBehavior.Restrict); // Устанавливаем DeleteBehavior.Restrict, чтобы не было конфликтов с внешним ключом



        }
    }
}
