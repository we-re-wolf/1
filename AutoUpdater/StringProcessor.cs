using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdater
{
    internal class StringProcessor
    {
        //Simple string encrypter
        public static string EncryptString(string text)
        {
            byte xorConstant = 0x32;
            byte[] data = Encoding.UTF8.GetBytes(text);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ xorConstant);
            }
            return Convert.ToBase64String(data);
        }

        //Simple string decrypter
        public static string DecryptString(string text)
        {
            byte xorConstant = 0x32;
            byte[] data = Convert.FromBase64String(text);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ xorConstant);
            }
            return Encoding.UTF8.GetString(data);
        }
    }
}
