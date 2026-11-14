using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace Wrblo.SAL.Shared.Helpers;

public static class PasswordHelper
{
    private static readonly PasswordHasher<object> _hasher = new();

    public static string HashPassword(this string plainPassword)
    {
        return _hasher.HashPassword(null, plainPassword);
    }

    // Verify a provided password against a stored hash
    public static bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(null, hashedPassword, providedPassword);
        return result == PasswordVerificationResult.Success;
    }

    // Generate a secure random password
    public static string GenerateRandomPassword()
    {
        const int length = 8;
        const string allowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";

        var password = new char[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] buffer = new byte[sizeof(uint)];

            for (int i = 0; i < length; i++)
            {
                rng.GetBytes(buffer);
                uint num = BitConverter.ToUInt32(buffer, 0);
                password[i] = allowedChars[(int)(num % (uint)allowedChars.Length)];
            }
        }

        return new string(password);
    }
}
