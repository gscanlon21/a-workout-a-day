using System.Security.Cryptography;

namespace Core.Code;

/// <summary>
/// https://www.exceedsystem.net/2021/10/04/how-to-implement-otp-in-csharp-so-easy/
/// License:MIT
/// </summary>
public static class Program
{
    /// <summary>
    /// Interval time(sec)
    /// </summary>
    private const int INTERVAL_SEC = 30;

    /// <summary>
    /// Digits()
    /// </summary>
    private const int NUM_OF_DIGITS = 6;

    /// <summary>
    /// Entry
    /// </summary>
    /// <param name="_">unused</param>
    /*private static void Main(string[] _)
    {
        Console.WriteLine("Enter the TOTP private key encoded in base32.");
        var privateKey = DecodeBase32(Console.ReadLine()).ToArray();

        var isFirstTime = true;
        while (true)
        {
            var remainingSec = (long)TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalSeconds % INTERVAL_SEC;
            if (isFirstTime || remainingSec == 0)
            {
                isFirstTime = false;
                var totp = GetTOTP(privateKey, NUM_OF_DIGITS, INTERVAL_SEC);
                Console.WriteLine(totp);
            }
            Console.Title = $"TOTP Client:      TIME REMAINING:{INTERVAL_SEC - remainingSec,2}";
            Thread.Sleep(1000);
        }
    }*/

    /// <summary>
    /// Get TOTP password
    /// </summary>
    /// <param name="privateKey">Private key</param>
    /// <param name="numOfDigits">Number of digits in generated password</param>
    /// <param name="interval">Password generation interval(sec)</param>
    /// <returns>TOTP password</returns>
    public static string GetTOTP(byte[] privateKey, int numOfDigits, int interval)
    {
        var counter = (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds / interval;
        return GetOTP(privateKey, counter, numOfDigits);
    }

    /// <summary>
    /// Get OTP password
    /// </summary>
    /// <param name="privateKey">Private key</param>
    /// <param name="iteration">Iteration number</param>
    /// <param name="numOfDigits">Number of digits in generated password</param>
    /// <returns>OTP</returns>
    private static string GetOTP(byte[] privateKey, long iteration, int numOfDigits)
    {
        var counter = BitConverter.GetBytes(iteration);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(counter);
        }

        var hmacSha1 = new HMACSHA1(privateKey);
        var hmacResult = hmacSha1.ComputeHash(counter);

        var offset = hmacResult[^1] & 0xf;
        var binCode = ((hmacResult[offset] & 0x7f) << 24)
        | ((hmacResult[offset + 1] & 0xff) << 16)
        | ((hmacResult[offset + 2] & 0xff) << 8)
        | (hmacResult[offset + 3] & 0xff);

        var password = binCode % (int)Math.Pow(10, numOfDigits);

        return password.ToString(new string('0', numOfDigits));
    }

    /// <summary>
    /// Decode Base32 string into enumerable byte data
    /// </summary>
    /// <param name="encodedString">Base32 encorded string</param>
    /// <returns>Decoded byte data</returns>
    public static IEnumerable<byte> DecodeBase32(string encodedString)
    {
        var numOfBit = 0;
        byte decoded = 0;
        foreach (var base32Char in encodedString.ToUpper())
        {
            var base32Val = 0;
            switch (base32Char)
            {
                case >= 'A' and <= 'Z':
                    base32Val = base32Char - 65;
                    break;
                case >= '2' and <= '7':
                    base32Val = base32Char - 24;
                    break;
            }
            var bitMask = 0x10;
            for (var i = 0; i < 5; ++i)
            {
                decoded |= (byte)((base32Val & bitMask) != 0 ? 1 : 0);
                if (++numOfBit == 8)
                {
                    yield return decoded;
                    numOfBit = 0;
                    decoded = 0;
                }
                decoded <<= 1;
                bitMask >>= 1;
            }
        }
    }
}
