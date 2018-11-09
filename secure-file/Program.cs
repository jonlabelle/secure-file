using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AppVeyor.Tools.SecureFile
{
    internal static class Program
    {
        private const string Salt = "{0F4AACD6-3C07-4330-92A5-64297265128E}";

        private static void ShowUsage()
        {
            Console.Write(@"
USAGE:

    Encrypting file:

        secure-file -encrypt <filename.ext> -secret <keyphrase> -out [filename.ext.enc]

    Decrypting file:

        secure-file -decrypt <filename.ext.enc> -secret <keyphrase> -out [filename.ext]
");
        }

        private static int Main(string[] args)
        {
            string operation = null;
            string fileName = null;
            string secret = null;
            string outFileName = null;

            #region parse parameters

            if (args.Length == 0)
            {
                ShowUsage();
                return 1;
            }

            var argPosition = 0;
            while (argPosition < args.Length)
            {
                var parsedArg = args[argPosition];

                if (parsedArg.Equals("-decrypt", StringComparison.OrdinalIgnoreCase))
                {
                    // is it last parameter?
                    if (argPosition == args.Length - 1)
                    {
                        Console.WriteLine("Input file name is missing.");
                        return 1;
                    }

                    operation = "decrypt";
                    fileName = args[++argPosition];
                }
                else if (parsedArg.Equals("-encrypt", StringComparison.OrdinalIgnoreCase))
                {
                    // is it last parameter?
                    if (argPosition == args.Length - 1)
                    {
                        Console.WriteLine("Input file name is missing.");
                        return 1;
                    }

                    operation = "encrypt";
                    fileName = args[++argPosition];
                }
                else if (parsedArg.Equals("-secret", StringComparison.OrdinalIgnoreCase))
                {
                    // is it last parameter?
                    if (argPosition == args.Length - 1)
                    {
                        Console.WriteLine("Secret pass phrase is missing.");
                        return 1;
                    }

                    secret = args[++argPosition];
                }
                else if (parsedArg.Equals("-out", StringComparison.OrdinalIgnoreCase))
                {
                    // is it last parameter?
                    if (argPosition == args.Length - 1)
                    {
                        Console.WriteLine("Out file name is missing.");
                        return 1;
                    }

                    outFileName = args[++argPosition];
                }

                argPosition++;
            }

            if (operation == null)
            {
                Console.WriteLine("No operation specified. It should be either -encrypt or -decrypt.");
                return 1;
            }

            #endregion

            #region validate file names

            if (outFileName == null && operation == "encrypt")
            {
                outFileName = fileName + ".enc";
            }
            else if (outFileName == null && operation == "decrypt")
            {
                if (Path.GetExtension(fileName).Equals(".enc", StringComparison.OrdinalIgnoreCase))
                {
                    outFileName = fileName.Substring(0, fileName.Length - 4); // trim .enc
                }
                else
                {
                    outFileName = fileName + ".dec";
                }
            }

            var basePath = Environment.CurrentDirectory;

            // convert relative paths to absolute
            if (!Path.IsPathRooted(fileName))
            {
                fileName = Path.GetFullPath(Path.Combine(basePath, fileName));
            }

            if (!Path.IsPathRooted(outFileName))
            {
                outFileName = Path.GetFullPath(Path.Combine(basePath, outFileName));
            }

            // in and out file names should not be the same
            if (fileName.Equals(outFileName, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Input and output files cannot be the same.");
                return 1;
            }

            if (!File.Exists(fileName))
            {
                Console.WriteLine("File not found: '{0}'.", fileName);
                return 1;
            }

            #endregion

            switch (operation)
            {
                case "encrypt":
                    try
                    {
                        Encrypt(fileName, outFileName, secret);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error encrypting file: {0}.", ex.Message);
                        return 2;
                    }

                    break;
                case "decrypt":
                    try
                    {
                        Decrypt(fileName, outFileName, secret);
                    }
                    catch (CryptographicException)
                    {
                        Console.WriteLine("Error decrypting file.");
                        return 3;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error decrypting file: {0}.", ex.Message);
                        return 3;
                    }

                    break;
            }

            return 0;
        }

        private static void Encrypt(string fileName, string outFileName, string secret)
        {
            var alg = GetRijndael(secret);

            using (var inStream = File.OpenRead(fileName))
            {
                using (var outStream = File.Create(outFileName))
                {
                    using (var cryptoStream = new CryptoStream(outStream, alg.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        inStream.CopyTo(cryptoStream);
                    }
                }
            }
        }

        private static void Decrypt(string fileName, string outFileName, string secret)
        {
            var alg = GetRijndael(secret);

            using (var inStream = File.OpenRead(fileName))
            {
                using (var outStream = File.Create(outFileName))
                {
                    using (var cryptoStream = new CryptoStream(outStream, alg.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        inStream.CopyTo(cryptoStream);
                    }
                }
            }
        }

        private static Rijndael GetRijndael(string secret)
        {
            var saltBytes = Encoding.UTF8.GetBytes(Salt);
//            var saltBytes = GetSalt();

            var pbkdf2 = new Rfc2898DeriveBytes(secret, saltBytes, 10000);
            var alg = Rijndael.Create();

            alg.Key = pbkdf2.GetBytes(32);
            alg.IV = pbkdf2.GetBytes(16);

            return alg;
        }

        #region - Random Salt Generation -

        // https://codereview.stackexchange.com/a/93622

        private const int SaltLengthLimit = 32;

        private static byte[] GetSalt(int maximumSaltLength = SaltLengthLimit)
        {
            var salt = new byte[maximumSaltLength];

            using (var random = new RNGCryptoServiceProvider())
            {
                random.GetNonZeroBytes(salt);
            }

            return salt;
        }

        #endregion
    }
}
