﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using zorgapp.Models;

namespace zorgapp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static string GenerateLinkCode()
        {
            Random rd = new Random();
            string code;
            code = "";
            int intcode;
            intcode = rd.Next(999999); //generates a random number (1 000 000 possible values)
            byte[] bytecode = BitConverter.GetBytes(intcode); //turns the random number into a bytelist to use as input for the hash

            using (MD5 MD5HASH = MD5.Create())
            {
                // ComputeHash - returns byte array
                byte[] bytes = MD5HASH.ComputeHash(bytecode);

                // Convert byte array to a string   
                for (int i = 0; i < bytes.Length; i++)
                {
                    code += bytes[i].ToString("x2");
                }
            }
            //System.Diagnostics.Debug.WriteLine("Input int is: " + intcode.ToString());
            //System.Diagnostics.Debug.WriteLine("Output is: " + code);
            return code;
        }

        public static string Hash256bits(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                String hash = "";
                for (int i = 0; i < bytes.Length; i++)
                {
                    hash += bytes[i].ToString("x2");
                }
                return hash;
            }
        }

        public static List<Appointment> FilterWeek(List<Appointment> List, DateTime dateTime, int Days)
        {
            List<Appointment> NewList = new List<Appointment>();
            foreach (var app in List)
            {
                DateTime day = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
                if (SameWeek(day, app.Date))//(day.AddDays(-(int)day.DayOfWeek) == app.Date.AddDays(-(int)app.Date.DayOfWeek))
                {
                    NewList.Add(app);
                }
                /*for (int d = 0; d < 7; d++)
                {
                    day = day.AddDays(1);
                    if (day.Year == app.Date)
                    //System.Diagnostics.Debug.WriteLine("RonanDayList: " + day.ToString());
                }*/
            }

            return NewList;
        }
        public static bool SameWeek(DateTime day1, DateTime day2)
        {
            DateTime Day1;
            DateTime Day2;
            int offset1;
            int offset2;

            Day1 = new DateTime(day1.Year, day1.Month, day1.Day);
            Day2 = new DateTime(day2.Year, day2.Month, day2.Day);

            switch (Day1.DayOfWeek.ToString())//offset to bring the day to monday
            {
                case "Monday":
                    offset1 = 0;
                    break;
                case "Tuesday":
                    offset1 = -1;
                    break;
                case "Wednesday":
                    offset1 = -2;
                    break;
                case "Thursday":
                    offset1 = -3;
                    break;
                case "Friday":
                    offset1 = -4;
                    break;
                case "Saturday":
                    offset1 = -5;
                    break;
                case "Sunday":
                    offset1 = -6;
                    break;
                default:
                    offset1 = 0;
                    break;
            }

            switch (Day2.DayOfWeek.ToString())//offset to bring the day to monday
            {
                case "Monday":
                    offset2 = 0;
                    break;
                case "Tuesday":
                    offset2 = -1;
                    break;
                case "Wednesday":
                    offset2 = -2;
                    break;
                case "Thursday":
                    offset2 = -3;
                    break;
                case "Friday":
                    offset2 = -4;
                    break;
                case "Saturday":
                    offset2 = -5;
                    break;
                case "Sunday":
                    offset2 = -6;
                    break;
                default:
                    offset2 = 0;
                    break;
            }

            Day1 = Day1.AddDays(offset1);
            Day2 = Day2.AddDays(offset2);
            return (Day1 == Day2);
        }


        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
