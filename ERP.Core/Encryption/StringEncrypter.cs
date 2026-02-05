using System.Security.Cryptography;
using System.Text;

namespace ERP.Core.Encryption
{
    public class StringEncrypter
    {
        public static string password = "../'?,<19>{05.";
        public static string[] split = { "falcao" };

        public string GetKey { get; set; }

        public static byte[] Encrypt(byte[] input, string password)
        {
            try
            {
                using (TripleDESCryptoServiceProvider service2 = new TripleDESCryptoServiceProvider())
                {
                    using (MD5CryptoServiceProvider md52 = new MD5CryptoServiceProvider())
                    {
                        var rgbKey = md52.ComputeHash(Encoding.ASCII.GetBytes(password));
                        var rgbIv = md52.ComputeHash(Encoding.ASCII.GetBytes(password));
                        return Transform(input, service2.CreateEncryptor(rgbKey, rgbIv));
                    }
                }
            }
            catch (Exception)
            {
                return new byte[0];
            }
        }

        public static byte[] Decrypt(byte[] input, string password)
        {
            try
            {
                using (TripleDESCryptoServiceProvider service = new TripleDESCryptoServiceProvider())
                {
                    using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                    {
                        var rgbKey = md5.ComputeHash(Encoding.ASCII.GetBytes(password));
                        var rgbIv = md5.ComputeHash(Encoding.ASCII.GetBytes(password));

                        return Transform(input, service.CreateDecryptor(rgbKey, rgbIv));
                    }
                }
            }
            catch (Exception)
            {
                return new byte[0];
            }
        }

        public static string Encrypt(string text, string password)
        {
            byte[] input = Encoding.UTF8.GetBytes(text);
            byte[] output = Encrypt(input, password);
            return Convert.ToBase64String(output);
        }

        public static string Decrypt(string text, string password)
        {
            byte[] input = Convert.FromBase64String(text);
            byte[] output = Decrypt(input, password);
            return Encoding.UTF8.GetString(output);
        }

        public static string Encrypt(string text)
        {
            return Encrypt(text, password);
        }

        public static string Decrypt(string text)
        {
            return Decrypt(text, password);
        }

        private static byte[] Transform(byte[] input, ICryptoTransform CryptoTransform)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                using (CryptoStream cryptStream = new CryptoStream(memStream, CryptoTransform, CryptoStreamMode.Write))
                {
                    cryptStream.Write(input, 0, input.Length);
                    cryptStream.FlushFinalBlock();

                    memStream.Position = 0;
                    byte[] result = new byte[Convert.ToInt32(memStream.Length)];
                    memStream.Read(result, 0, Convert.ToInt32(result.Length));

                    memStream.Close();
                    cryptStream.Close();

                    return result;
                }
            }
        }
    }
}
