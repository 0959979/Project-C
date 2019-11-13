using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace zorgapp.Models
{
    public class DatabaseContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // modelBuilder.Entity<Linked>()
            // .HasKey (c => new object{ c.DoctorId, c.PatientId});

            // modelBuilder.Entity<Patient>()
            // .HasMany(x => x.Doctors);
         
            // modelBuilder.Entity<Linked>()
            // .HasMany(y => y.Patients)
            // .WithOne(x => x.Doctors);
            
            // modelBuilder.Entity<Doctor>()
            // .HasMany(y => y.Patients);

        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
        public DbSet<Patient> Patients { get; set; }

        public DbSet<Doctor> Doctors { get; set; }

        public DbSet<Admin> Admins { get; set; }

        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<Case> Cases { get; set; }

        public DbSet<Message> Messages { get; set; }

        // public DbSet<Linked> linkeds { get; set; }



    }
    
}
