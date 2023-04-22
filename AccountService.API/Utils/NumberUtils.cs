using System;

namespace AccountService.API.Utils;

public class NumberUtils
{
    public static string GenerateRandomNDigits(int numberOfDigits)
    {
        return new Random().Next(0, (int) Math.Pow(10, numberOfDigits)).ToString("D6");
    }
}
