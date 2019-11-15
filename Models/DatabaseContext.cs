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
            modelBuilder.Entity<PatientsDoctors>()
            .HasKey(x => new {x.DoctorId, x.PatientId});
            modelBuilder.Entity<PatientsDoctors>()
            .HasOne(pd => pd.Doctor)
            .WithMany(d => d.PatientsDoctorss)
            .HasForeignKey(pd => pd.DoctorId);
            modelBuilder.Entity<PatientsDoctors>()
            .HasOne(pd => pd.Patient)
            .WithMany (p => p.PatientsDoctorss)
            .HasForeignKey (pd => pd.PatientId);
    
    //https://www.learnentityframeworkcore.com/configuration/many-to-many-relationship-configuration
    
        }

        // protected override void OnModelCreating(ModelBuilder modelBuilder)
        // {

        //     // modelBuilder.Entity<Patient>()
        //     // .HasMany(x => x.Doctors)
        //     // .HasForeignKey(c => c.DoctorId);

        //     modelBuilder.Entity<PatientsDoctors>()
        //     .HasKey(x => new {x.DoctorId, x.PatientId});

        //     modelBuilder.Entity<PatientsDoctors>()
        //     .HasOne(x => x.Doctor)
        //     .WithMany(y => y.PatientsDoctorss)
        //     .HasForeignKey(w =>w.DoctorId);

        //     modelBuilder.Entity<PatientsDoctors>()
        //     .HasOne(x => x.Patient)
        //     .WithMany(y => y.PatientsDoctorss)
        //     .HasForeignKey(f => f.PatientId);


        // }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
        public DbSet<Patient> Patients { get; set; }

        public DbSet<Doctor> Doctors { get; set; }

        public DbSet<Admin> Admins { get; set; }
        
        public DbSet<Practice> Practices { get; set; }

        public DbSet<PatientsDoctors> PatientsDoctorss {get; set;}


        // public DbSet<Linked> linkeds { get; set; }
        


    }
    
}
