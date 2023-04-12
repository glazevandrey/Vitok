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
        public DbSet<UserDate> UserDates { get; set; }
        public DbSet<Goal> Goals{ get; set; }
        //public DbSet<InChat> InChats{ get; set; }
        //public DbSet<Messages> Messages{ get; set; }
        public DbSet<Notifications> Notifications{ get; set; }

       // public DbSet<NotificationTask> NotificationTasks { get; set; }
        //public DbSet<NotificationTokens> NotificationTokens{ get; set; }
        //public DbSet<PaidLesson> PaidLessons{ get; set; }
        public DbSet<Registration> Registrations{ get; set; }
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
            modelBuilder.Entity<Goal>();
            modelBuilder.Entity<TutorDTO>().ToTable("Tutors");
            modelBuilder.Entity<ManagerDTO>().ToTable("Managers");
            modelBuilder.Entity<StudentDTO>().ToTable("Students");

            //modelBuilder.Entity<Goal>().HasData(TestData.Goals);
            //modelBuilder.Entity<Course>().HasData(TestData.Courses) ;
            //modelBuilder.Entity<Schedule>().HasData(TestData.Schedules);
            //modelBuilder.Entity<Tariff>().HasData(TestData.Tariffs);

            // остальное добавляется отдельно


            //  modelBuilder.Entity<Course>()
            //.HasKey(t => new { t.GoalId, t.TutorId});

            modelBuilder.Entity<Course>()
        .HasOne(pt => pt.Tutor)
        .WithMany(p => p.Courses)
        .HasForeignKey(pt => pt.TutorId);

            modelBuilder.Entity<Course>()
                .HasOne(pt => pt.Goal)
                .WithOne(t => t.Course)
                .HasForeignKey<Goal>(pt => pt.CourseId)
                .OnDelete(DeleteBehavior.Restrict); // Устанавливаем DeleteBehavior.Restrict, чтобы не было конфликтов с внешним ключом


            modelBuilder.Entity<ScheduleDTO>()
            .HasOne(pt => pt.Course)
            .WithOne(t => t.Schedule)
            .HasForeignKey<Course>(pt => pt.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict); // Устанавливаем DeleteBehavior.Restrict, чтобы не было конфликтов с внешним ключом


            //modelBuilder.Entity<Schedule>()
            //.HasOne(pt => pt.StartDate)
            //.WithOne(t => t.Schedule)
            //.HasForeignKey<UserDate>(pt => pt.ScheduleId)
            //.OnDelete(DeleteBehavior.Restrict); // Устанавливаем DeleteBehavior.Restrict, чтобы не было конфликтов с внешним ключом




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

        // MEKDD
        //modelBuilder.Entity<StudentDTO>()
        //.HasMany(pt => pt.Schedules)
        //.WithOne(t => t.Student)
        //.HasForeignKey(pt => pt.StudentId)
        //.OnDelete(DeleteBehavior.Restrict); // Устанавливаем DeleteBehavior.Restrict, чтобы не было конфликтов с внешним ключом
        /*

        SqlException: The INSERT statement conflicted with the FOREIGN KEY constraint 
            "FK_UserDates_Schedules_ScheduleId". The conflict occurred in database "vitokdb",
            table "dbo.Schedules", column 'Id'.

             */

            modelBuilder.Entity<TutorDTO>()
            .HasMany(pt => pt.Schedules)
            .WithOne(t => t.Tutor)
            .HasForeignKey(pt => pt.TutorId)
            .OnDelete(DeleteBehavior.Restrict); // Устанавливаем DeleteBehavior.Restrict, чтобы не было конфликтов с внешним ключом

            //modelBuilder.Entity<Schedule>()
            //.HasOne(pt => pt.Tutor)
            //.WithMany(t => t.)
            //.HasForeignKey<UserDate>(pt => pt.ScheduleId)
            //.OnDelete(DeleteBehavior.Restrict); // Устанавливаем DeleteBehavior.Restrict, чтобы не было конфликтов с внешним ключом



            modelBuilder.Entity<TutorDTO>()
            .HasMany(pt => pt.UserDates)
            .WithOne(t => t.Tutor)
            .HasForeignKey(pt => pt.TutorId)
            .OnDelete(DeleteBehavior.Restrict); // Устанавливаем DeleteBehavior.Restrict, чтобы не было конфликтов с внешним ключом




            //modelBuilder.Entity<Course>()
            //    .HasOne(pt => pt.Tutor)
            //    .WithMany(p => p.Courses)
            //    .HasForeignKey(pt => pt.TutorId);

            //modelBuilder.Entity<Course>()
            //    .HasOne(pt => pt.Goal)
            //    .WithOne(t => t.Course)
            //    .HasForeignKey<Goal>(pt=>pt.CourseId);

            //    modelBuilder.Entity<StudentDTO>()
            //.HasMany(s => s.Money)
            //.WithOne()
            //.HasForeignKey("StudentDTOUserId"); // Указание имени внешнего ключа

            //    // Конфигурация отношения между StudentDTO и UserCredit
            //    modelBuilder.Entity<StudentDTO>()
            //        .HasMany(s => s.Credit)
            //        .WithOne()
            //        .HasForeignKey("StudentDTOUserId"); // Указание имени внешнего ключа




        }
    }
}
