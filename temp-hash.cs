using System;
using System.Security.Cryptography;
using System.Text;

var salt = new byte[32];
RandomNumberGenerator.Fill(salt);
var hash = Rfc2898DeriveBytes.Pbkdf2(
    Encoding.UTF8.GetBytes("test1234"), salt, 100000, HashAlgorithmName.SHA256, 32);
Console.WriteLine($"HASH:{Convert.ToBase64String(hash)}");
Console.WriteLine($"SALT:{Convert.ToBase64String(salt)}");
