using System;
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

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
