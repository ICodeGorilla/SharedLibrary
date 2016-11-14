using System.Security.Cryptography;
using System.Text;

namespace Shared.Helper
{
    public static class Md5Encrypt
    {
        public static string Md5EncryptString(string data)
        {
            var encoding = new ASCIIEncoding();
            var bytes = encoding.GetBytes(data);
            var hashed = MD5.Create().ComputeHash(bytes);
            return Encoding.UTF8.GetString(hashed);
        }
    }
}