using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace FileOperatorDotNet
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private int _mDAddressOffset;
        internal IntPtr MhFile = IntPtr.Zero;
        internal IntPtr MhFileMap = IntPtr.Zero;
        internal IntPtr MhWriteFile = IntPtr.Zero;
        internal int MReadposition;
        private int _mSzMemorySize;
        internal int MWriteposition;

        public MainWindow()
        {
            InitializeComponent();
        }

        internal IntPtr PStartAddress
        {
            get => PStartAddress1;
            set => PStartAddress1 = value;
        }

        public IntPtr PStartAddress1 { get; set; } = IntPtr.Zero;


        private void button_Open_Read_Click(object sender, RoutedEventArgs e)
        {
            var filename = textBox_FileName.Text;
            if (!Utils.FileFolderExistInWindows(filename))
            {
                MessageBox.Show("Please type a valid file name!");
                return;
            }
            uint options = 0;
            if (Button_NoCached.IsChecked ?? false)
                options = options | (uint) EFileAttributes.NoBuffering;
            MhFile = WIN32API.CreateFile(
                filename,
                FileAccess.Read,
                //FILE_SHARE_WRITE | FILE_SHARE_READ | FILE_SHARE_DELETE,
                FileShare.Read | FileShare.Write | FileShare.Delete,
                //IntPtr.Zero,
                IntPtr.Zero,
                //OPEN_EXISTING,
                FileMode.Open,
                (FileAttributes) options,
                IntPtr.Zero);

            MessageBox.Show(MhFile.ToInt32() == -1 ? "Failed to open" : "Successfully openned the file!");
        }

        private void textBox_FileName_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Text == "test.txt")
                textBox.Text = "";
        }

        private void button_GetSize_Click(object sender, RoutedEventArgs e)
        {
            if (MhFile == IntPtr.Zero)
                MessageBox.Show("Open a valid file first.");

            long dwFileSize;
            if (!WIN32API.GetFileSizeEx(MhFile, out dwFileSize))
                MessageBox.Show("Failed to get file size");

            var msg = $"{dwFileSize} Bytes";
            label_Size.Content = msg;
        }

        private void button_Close_Read_Click(object sender, RoutedEventArgs e)
        {
            if (MhFile == IntPtr.Zero)
            {
                MessageBox.Show("No file openned");
                return;
            }


            _mDAddressOffset = 0;
            _mSzMemorySize = 0;
            if (PStartAddress != IntPtr.Zero)
            {
                if (!WIN32API.UnmapViewOfFile(PStartAddress))
                    MessageBox.Show("unmap file failed " + Marshal.GetLastWin32Error());
                else
                    PStartAddress = IntPtr.Zero;

                if (MhFileMap != IntPtr.Zero && !WIN32API.CloseHandle(MhFileMap))
                    MessageBox.Show("unmap file failed " + Marshal.GetLastWin32Error());
                else
                    MhFileMap = IntPtr.Zero;
            }

            var ret = WIN32API.CloseHandle(MhFile);

            if (ret)
            {
                MessageBox.Show("Closed successfully!");
            }
            else
            {
                var er = Marshal.GetLastWin32Error();
                var msg = $"Failed to close. Error code={er}";
                MessageBox.Show(msg);
            }
            MhFile = IntPtr.Zero;
        }

        private void button_FSeek_Click(object sender, RoutedEventArgs e)
        {
            if (MhFile == IntPtr.Zero)
            {
                MessageBox.Show("No file open");
                return;
            }

            var mReadfseektypeSelect = combox_Seek_Read.Text;

            //    uint type = file_begin;
            //    if (m_readfseektype_select == 0) type = file_begin;
            //    else if (m_readfseektype_select == 1) type = file_current;
            //    else if (m_readfseektype_select == 2) type = file_end;
            //    else
            //    {
            //        MessageBox.Show("type is not set properly. treat is as beging set to file_begin.");
            //    }
            uint seektype = 0;
            switch (mReadfseektypeSelect)
            {
                case "begin pos":
                    seektype = 0;
                    break;
                case "current pos":
                    seektype = 1;
                    break;
                case "end pos":
                    seektype = 2;
                    break;
            }

            int readpos;
            if (!int.TryParse(textBox_EditOffset.Text, out readpos))
            {
                MessageBox.Show("invalid input for the fseek");
                return;
            }
            if (PStartAddress != IntPtr.Zero)
                //if(false)
            {
                if (seektype == 2)
                {
                    MessageBox.Show("mapped file does not support fseek from end!");
                }
                else if (seektype == 1)
                {
                    if (_mDAddressOffset + readpos > _mSzMemorySize || _mDAddressOffset + readpos < 0)
                    {
                        MessageBox.Show("fseek beyond memory space!");
                    }
                    else
                    {
                        _mDAddressOffset += readpos;
                        MessageBox.Show("succeeded. " + _mDAddressOffset);
                    }
                }
                else
                {
                    if (readpos > _mSzMemorySize || readpos < 0)
                    {
                        MessageBox.Show("fseek beyond memory space!");
                    }
                    else
                    {
                        _mDAddressOffset = readpos;
                        MessageBox.Show("succeeded. " + _mDAddressOffset);
                    }
                }
            }
            else
            {
                var res = (int) WIN32API.SetFilePointer(MhFile, readpos, IntPtr.Zero, seektype);

                if (res == -1)
                {
                    var er = Marshal.GetLastWin32Error();
                    MessageBox.Show("failed to do seek. errorcode " + er);
                }
                else
                {
                    var s = $"Succeeded.  pointer pos = {res}";
                    MessageBox.Show(s);
                    MReadposition = res;
                }
            }
        }

        private void Button_Read_Click(object sender, RoutedEventArgs e)
        {
            var hFile = MhFile != IntPtr.Zero ? MhFile : MhWriteFile;
            if (hFile == IntPtr.Zero)
            {
                MessageBox.Show("No file open.");
                return;
            }

            if (MReadposition < 0)
            {
                MessageBox.Show("Read pos error. Reopen the file and try again.");
                return;
            }

            uint len;
            if (!uint.TryParse(textBox_ReadLen.Text, out len))
            {
                MessageBox.Show("Invalid input for the reading Length");
                return;
            }
            uint dwBytesRead;
            var retval = new byte[len];

            if (PStartAddress != IntPtr.Zero)
                //if(false)
            {
                if (_mDAddressOffset < 0)
                {
                    MessageBox.Show(" pos error. Reopen the file and try again.");
                }
                else if (_mDAddressOffset >= _mSzMemorySize)
                {
                    MessageBox.Show(" reach the end of the memory space!");
                }
                else
                {
                    //uint errorCode;
                    //__try
                    dwBytesRead = len;
                    if (_mDAddressOffset + len > _mSzMemorySize)
                        dwBytesRead = (uint) (_mSzMemorySize - _mDAddressOffset);

                    //try\
                    {
                        var pinnedArray = GCHandle.Alloc(retval, GCHandleType.Pinned);
                        var pointer = pinnedArray.AddrOfPinnedObject();
                        WIN32API.CopyMemory(pointer, PStartAddress + _mDAddressOffset, dwBytesRead);
                        _mDAddressOffset += (int) dwBytesRead;

                        var hs = Utils.ByteArrayToString(retval);

                        TextBlock_ReadResult.Text = hs;

                        var s = $"{retval.Length} bytes required, {dwBytesRead} bytes returned";
                        MessageBox.Show("Succeeded. " + s);
                        pinnedArray.Free();
                    }
                    //__except (GetExceptionCode() == EXCEPTION_IN_PAGE_ERROR ? 
                    //EXCEPTION_EXECUTE_HANDLER : EXCEPTION_CONTINUE_SEARCH)
                    //catch (int errorCode)
                    //{
                    //    // Failed to write to the view.
                    //    //errorCode = GetExceptionCode();
                    //    MessageBox.Show("an paging error happened when writing to mapped file!", errorCode);
                    //}
                }
            }
            else
            {
                var res = WIN32API.ReadFile(hFile, retval, len, out dwBytesRead, IntPtr.Zero);

                if (res)
                {
                    var hs = Utils.ByteArrayToString(retval);

                    TextBlock_ReadResult.Text = hs;

                    var s = $"{len} bytes required, {dwBytesRead} bytes returned";
                    MessageBox.Show("Succeeded. " + s);
                }
                else
                {
                    var er = Marshal.GetLastWin32Error();
                    var s = $"Error code = {er}";
                    MessageBox.Show("Failed to read. " + s);
                }
            }
        }

        private void button_Open_Write_Click(object sender, RoutedEventArgs e)
        {
            var filename = textBox_Write_FileName.Text;

            //if(!FileFolderExistInWindows(m_writefilename))
            //{
            //	MessageBox.Show("Please type a valid file name for writing!");
            //	return ;
            //}

            if (MhWriteFile != IntPtr.Zero)
            {
                MessageBox.Show("File handle has NOT been properly closed yet. Click close before open it again.");
                return;
            }

            var flag = (int) FileAttributes.Normal;
            if (Button_WriteNoCached.IsChecked ?? false)
                flag |= (int) EFileAttributes.NoBuffering;
            var option = FileMode.OpenOrCreate;
            if (Button_Overwrite.IsChecked ?? false)
                option = FileMode.Create;
            var dirName = new StringBuilder((int) WIN32API.MAX_DEEP_PATH + 3);
            //TCHAR dirName[256];
            var rlen = WIN32API.GetCurrentDirectory(WIN32API.MAX_DEEP_PATH, dirName);
            if (rlen == 0)
            {
                var lastError = Marshal.GetLastWin32Error();
                MessageBox.Show("Failed to get initial working directory; error = " + lastError);
                return;
            }

            if (rlen > WIN32API.MAX_DEEP_PATH)
            {
                MessageBox.Show(
                    $"Failed to get initial working directory; allocated buffer is shorter than required: '{WIN32API.MAX_DEEP_PATH}'<'{rlen}'");
                return;
            }

            // extend name
            if (Button_ExtendName.IsChecked ?? false)
            {
                // start with
                filename = dirName + "\\" + filename;

                if (!filename.StartsWith("\\\\"))
                {
                    // not start with, network drive 
                    if (filename.StartsWith("\\\\?\\"))
                        filename = "\\\\?\\UNC" + filename.Substring(1);
                }
                else
                {
                    filename = "\\\\?\\" + filename;
                }
            }
            else if (Button_WriteShortName.IsChecked ?? false)
            {
                // short name
                filename = dirName + "\\" + filename;

                var shortName = new StringBuilder(256);
                var len = WIN32API.GetShortPathName(filename, shortName, 256);
                if (len > 0)
                {
                    filename = shortName.ToString();
                }
                else
                {
                    var lastWin32Error = Marshal.GetLastWin32Error();
                    MessageBox.Show("Failed to get short name. " + lastWin32Error);
                    return;
                }
            }

            MhWriteFile = WIN32API.CreateFile(
                filename,
                //GENERIC_WRITE | GENERIC_READ,
                FileAccess.ReadWrite,
                //FILE_SHARE_WRITE | FILE_SHARE_READ | FILE_SHARE_DELETE,
                FileShare.ReadWrite | FileShare.Delete,
                IntPtr.Zero,
                option,
                (FileAttributes) (flag | (int) FileOptions.WriteThrough),
                IntPtr.Zero);

            if (MhWriteFile.ToInt32() == -1)
                MessageBox.Show("Failed to open for write.");
            else MessageBox.Show("Successfully openned the file! " + filename);
        }

        private void button_Close_Write_Click(object sender, RoutedEventArgs e)
        {
            if (MhWriteFile == IntPtr.Zero)
            {
                MessageBox.Show("No file openned");
                return;
            }

            //m_dAddressOffset = 0;
            //m_szMemorySize = 0;
            if (PStartAddress != IntPtr.Zero)
                //if(false)
            {
                if (!WIN32API.UnmapViewOfFile(PStartAddress))
                    MessageBox.Show("unmap file failed " + Marshal.GetLastWin32Error());
                else
                    PStartAddress = IntPtr.Zero;

                if (MhFileMap != IntPtr.Zero && !WIN32API.CloseHandle(MhFileMap))
                    MessageBox.Show("unmap file failed " + Marshal.GetLastWin32Error());
                else
                    MhFileMap = IntPtr.Zero;
            }

            var ret = WIN32API.CloseHandle(MhWriteFile);

            if (ret)
            {
                MessageBox.Show("Closed successfully!");
            }
            else
            {
                var er = Marshal.GetLastWin32Error();
                MessageBox.Show($"Failed to close.Error code = {er}");
            }
            MhWriteFile = IntPtr.Zero;
        }

        private void Button_Write_Click(object sender, RoutedEventArgs e)
        {
            if (MhWriteFile == IntPtr.Zero)
            {
                MessageBox.Show("No file open.");
                return;
            }

            if (MWriteposition < 0)
            {
                MessageBox.Show(" pos error. Reopen the file and try again.");
                return;
            }

            var writecontent = TextBlock_WriteHex.Text;
            var buf = Utils.StringToByteArray(writecontent);

            if (buf.Length == 0)
            {
                MessageBox.Show("Input content must be hex string. Invalid char detected.");
                return;
            }


            uint repeatTime;
            if (!uint.TryParse(textBox_WriteLen.Text, out repeatTime))
                repeatTime = 1;

            if (repeatTime > 1)
            {
                var nBuf = new byte[buf.Length * repeatTime];
                for (var i = 0; i < repeatTime; i++)
                    //CopyMemory(nBuf + i * len, buf, len);
                    Array.Copy(buf, 0, nBuf, i * buf.Length, buf.Length);
                buf = nBuf;
            }

            uint dwb;

            // write to mapped buffer
            if (PStartAddress != IntPtr.Zero)
                //if(false)
            {
                if (_mDAddressOffset < 0)
                {
                    MessageBox.Show(" pos error. Reopen the file and try again.");
                }
                else if (_mDAddressOffset >= _mSzMemorySize)
                {
                    MessageBox.Show(" reach the end of memory space! ");
                }
                else
                {
                    //DWORD errorCode;
                    //__try
                    dwb = (uint) buf.Length;
                    if (_mDAddressOffset + buf.Length > _mSzMemorySize)
                        dwb = (uint) (_mSzMemorySize - _mDAddressOffset);

                    //try
                    {
                        var pinnedArray = GCHandle.Alloc(buf, GCHandleType.Pinned);
                        var pointer = pinnedArray.AddrOfPinnedObject();
                        WIN32API.CopyMemory(PStartAddress + _mDAddressOffset, pointer, dwb);
                        _mDAddressOffset += (int) dwb;

                        var s = $"{buf.Length} bytes sent, {dwb} bytes written";
                        MessageBox.Show("Writing succeeded. " + s);
                        pinnedArray.Free();
                    }
                    //__except (GetExceptionCode() == EXCEPTION_IN_PAGE_ERROR ? 
                    //EXCEPTION_EXECUTE_HANDLER : EXCEPTION_CONTINUE_SEARCH)
                    //catch (int errorCode)
                    //{
                    //    // Failed to write to the view.
                    //    //errorCode = GetExceptionCode();
                    //    MessageBox.Show("an paging error happened when writing to mapped file!", errorCode);
                    //}
                }
            }
            else
            {
                var success = WIN32API.WriteFile(MhWriteFile, buf, (uint) buf.Length, out dwb, IntPtr.Zero);

                if (success)
                {
                    var s = $"{buf.Length} bytes sent, {dwb} bytes written";
                    MessageBox.Show("Writing succeeded. " + s);
                }
                else
                {
                    var er = Marshal.GetLastWin32Error();
                    var s = $"Error code = {er}";
                    MessageBox.Show("Failed to write. " + s);
                }
            }
        }

        private void button_FSeek_Write_Click(object sender, RoutedEventArgs e)
        {
            if (MhWriteFile == IntPtr.Zero)
            {
                MessageBox.Show("No file open");
                return;
            }

            //m_writefseektype_select = m_Combox_writeFseekType.GetCurSel();

            //DWORD type = FILE_BEGIN;
            //if (m_writefseektype_select == 0) type = FILE_BEGIN;
            //else if (m_writefseektype_select == 1) type = FILE_CURRENT;
            //else if (m_writefseektype_select == 2) type = FILE_END;
            //else
            //{
            //    MessageBox.Show("type is not set properly. Treat is as beging set to file_begin.");
            //}

            var mWritefseektypeSelect = combox_Seek_Write.Text;

            //    uint type = file_begin;
            //    if (m_readfseektype_select == 0) type = file_begin;
            //    else if (m_readfseektype_select == 1) type = file_current;
            //    else if (m_readfseektype_select == 2) type = file_end;
            //    else
            //    {
            //        MessageBox.Show("type is not set properly. treat is as beging set to file_begin.");
            //    }
            uint seektype = 0;
            switch (mWritefseektypeSelect)
            {
                case "begin pos":
                    seektype = 0;
                    break;
                case "current pos":
                    seektype = 1;
                    break;
                case "end pos":
                    seektype = 2;
                    break;
            }

            int pos;
            if (!int.TryParse(textBox_EditOffset_Write.Text, out pos))
            {
                MessageBox.Show("Invalid input for the position");
                return;
            }

            if (PStartAddress != IntPtr.Zero)
                //if(false)
            {
                if (seektype == 2)
                {
                    MessageBox.Show("mapped file does not support fseek from end!");
                }
                else if (seektype == 1)
                {
                    if (_mDAddressOffset + pos > _mSzMemorySize || _mDAddressOffset + pos < 0)
                    {
                        MessageBox.Show("fseek beyond memory space!");
                    }
                    else
                    {
                        _mDAddressOffset += pos;
                        MessageBox.Show("Succeeded. " + _mDAddressOffset);
                    }
                }
                else
                {
                    if (pos > _mSzMemorySize || pos < 0)
                    {
                        MessageBox.Show("fseek beyond memory space!");
                    }
                    else
                    {
                        _mDAddressOffset = pos;
                        MessageBox.Show("Succeeded. " + _mDAddressOffset);
                    }
                }
            }
            else
            {
                var res = WIN32API.SetFilePointer(MhWriteFile, pos, IntPtr.Zero, seektype);

                if ((int) res == -1)
                {
                    var er = Marshal.GetLastWin32Error();
                    var s = $"Error code = {er}";
                    MessageBox.Show("Failed to do seek. " + s);
                }
                else
                {
                    var s = $" pointer pos = {res}";
                    MessageBox.Show("Succeeded. " + s);

                    MWriteposition = (int) res;
                }
            }
        }

        private void button_Map_Click(object sender, RoutedEventArgs e)
        {
            if (MhWriteFile == IntPtr.Zero && MhFile == IntPtr.Zero)
            {
                MessageBox.Show("no valid file handle has been opened!");
            }
            else
            {
                uint dSize;
                if (!uint.TryParse(textBox_MapSize.Text, out dSize))
                {
                    MessageBox.Show("Invalid size");
                    return;
                }

                var handle = MhWriteFile == IntPtr.Zero ? MhFile : MhWriteFile;
                var options = MhWriteFile == handle ? FileMapProtection.PageReadWrite : FileMapProtection.PageReadonly;
                MhFileMap = WIN32API.CreateFileMapping(handle, IntPtr.Zero, options, (dSize >> 32) & 0xFFFF,
                    dSize & 0xFFFF, "test");

                if (MhFileMap == IntPtr.Zero)
                {
                    MessageBox.Show("error happened when creating file map " + Marshal.GetLastWin32Error());
                }
                else
                {
                    PStartAddress = WIN32API.MapViewOfFile(
                        MhFileMap,
                        options == FileMapProtection.PageReadWrite
                            ? FileMapAccess.FileMapWrite
                            : FileMapAccess.FileMapRead,
                        0,
                        0,
                        dSize);
                    if (PStartAddress == IntPtr.Zero)
                    {
                        MessageBox.Show("error happened when mapping this file " + Marshal.GetLastWin32Error());
                    }
                    else
                    {
                        _mDAddressOffset = 0;
                        _mSzMemorySize = 0;

                        var memBasicInfo = new MEMORY_BASIC_INFORMATION();

                        if (WIN32API.VirtualQuery(PStartAddress, out memBasicInfo,
                                (uint) Marshal.SizeOf(memBasicInfo)) <= 0)
                        {
                            MessageBox.Show("error happened when querying mem info " + Marshal.GetLastWin32Error());
                        }
                        else
                        {
                            _mSzMemorySize = memBasicInfo.RegionSize.ToInt32();
                            var msg = $"succeed mapping this file {PStartAddress}";
                            MessageBox.Show(msg);
                        }
                    }
                }
            }
        }

        private void button_UnMap_Click(object sender, RoutedEventArgs e)
        {
            if (PStartAddress == IntPtr.Zero)
            {
                MessageBox.Show("this file has not been mapped!");
            }
            else
            {
                if (!WIN32API.UnmapViewOfFile(PStartAddress))
                {
                    MessageBox.Show("error happened when unmapping this file " + Marshal.GetLastWin32Error());
                }
                else
                {
                    PStartAddress = IntPtr.Zero;
                    _mDAddressOffset = 0;
                    _mSzMemorySize = 0;
                    if (MhFileMap != IntPtr.Zero)
                        if (!WIN32API.CloseHandle(MhFileMap))
                        {
                            MessageBox.Show("error happened when closing file mapping handle " +
                                            Marshal.GetLastWin32Error());
                        }
                        else
                        {
                            MhFileMap = IntPtr.Zero;
                            MessageBox.Show("succeed unmapping file and closing handle!");
                        }
                }
            }
        }

        private void button_Flush_Click(object sender, RoutedEventArgs e)
        {
            if (PStartAddress == IntPtr.Zero)
            {
                MessageBox.Show("map address for writting is not valid!");
            }
            else
            {
                uint flushNum;

                if (!uint.TryParse(textBox_FlushNum.Text, out flushNum))
                {
                    MessageBox.Show("flush num must be a non-negative integer!");
                }
                else
                {
                    //DWORD errorCode;
                    //__try 
                    //try
                    //{
                    if (!WIN32API.FlushViewOfFile(PStartAddress, flushNum))
                    {
                        MessageBox.Show(" failed to flush view " + Marshal.GetLastWin32Error());
                    }
                    else
                    {
                        if (MhWriteFile != IntPtr.Zero)
                            if (!WIN32API.FlushFileBuffers(MhWriteFile))
                                MessageBox.Show(" failed to flush file " + Marshal.GetLastWin32Error());
                            else
                                MessageBox.Show("succeed flushing view and file! " + flushNum);
                        else
                            MessageBox.Show("succeed flushing view only! " + flushNum);
                    }
                    //}
                    //__except(EXCEPTION_EXECUTE_HANDLER) 
                    //catch (int errorCode)
                    //{
                    //    //errorCode = GetExceptionCode();
                    //    MessageBox.Show("paging error " + errorCode);
                    //}
                }
            }
        }

        private void button_BatchTest_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Unimplemented");
        }
    }
}