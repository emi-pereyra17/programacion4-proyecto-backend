using System;
using Microsoft.AspNetCore.Identity;

class Program
{
    static void Main()
    {
        // Cambiá esta contraseña por la que quieras usar
        var password = "123";

        // Crear hash compatible con ASP.NET Identity
        var hasher = new PasswordHasher<object>();
        string hash = hasher.HashPassword(null, password);

        Console.WriteLine("Contraseña: " + password);
        Console.WriteLine("PasswordHash: " + hash);
    }
}
