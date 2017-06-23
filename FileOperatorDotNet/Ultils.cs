using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FileOperatorDotNet
{
    class Utils
    {
        internal static bool FileFolderExistInWindows(string filefullpath)
        {
            uint d = WIN32API.GetFileAttributes(filefullpath); // from winbase.h
            int e = Marshal.GetLastWin32Error();
            if (WIN32API.INVALID_FILE_ATTRIBUTES == d)
            {
                //File not found
                return false;
            }
            else return true;
        }

        internal static string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        internal static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

    }
}
