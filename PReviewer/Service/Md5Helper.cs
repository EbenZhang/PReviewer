using System.Security.Cryptography;
using System.Text;

namespace PReviewer.Service
{
    public static class Md5Helper
    {
        public static string GetMd5HashString(this MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash. 
            var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            foreach (var t in data)
            {
                sBuilder.Append(t.ToString("x2"));
            }

            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }
    }
}
