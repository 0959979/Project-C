using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace zorgapp.Models
{
    public class Medicine
    {
		//public static object Claims { get; internal set; }
		public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date_start { get; set; }
        public DateTime Date_end { get; set; }
        public int Amount { get; set; }
        public int Patient_id { get; set; }
		public float Mg { get; set; }
    }
}
