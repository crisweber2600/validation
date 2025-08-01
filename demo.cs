using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace DemoApp
{
    // Simple demo of the MD5 hash deterministic GUID generation
    public static class Program
    {
        public static void Main()
        {
            Console.WriteLine("WithEntityIdSelector Pattern Demonstration");
            Console.WriteLine("==========================================");
            
            // Server names as in the requirement
            var serverNames = new[] { "ServerA", "ServerB", "ServerC", "ServerA" };
            
            Console.WriteLine("Deterministic GUID generation from Server names:");
            Console.WriteLine("-------------------------------------------------");
            
            foreach (var name in serverNames)
            {
                var guid = CreateDeterministicGuid(name);
                Console.WriteLine($"Server: {name,-10} => EntityId: {guid}");
            }
            
            Console.WriteLine();
            Console.WriteLine("Notice that servers with the same name get the same GUID!");
            Console.WriteLine("This enables SaveAudit records to be grouped by Server.Name.");
        }
        
        private static Guid CreateDeterministicGuid(string input)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input ?? string.Empty));
            return new Guid(hash);
        }
    }
}