using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace zorgapp.Models
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
        public DbSet<Patient> Patients { get; set; }

        public DbSet<Doctor> Doctors { get; set; }

		private DbSet<DoInfo> doInfo;

		public DbSet<DoInfo> GetDoInfo()
		{
			return doInfo;
		}

		public void SetDoInfo(DbSet<DoInfo> value)
		{
			doInfo = value;
		}
	}
}
