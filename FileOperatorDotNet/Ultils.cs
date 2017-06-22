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

    }
}
