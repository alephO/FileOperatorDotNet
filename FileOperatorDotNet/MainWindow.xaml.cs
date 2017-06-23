using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileOperatorDotNet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal IntPtr m_hFile = IntPtr.Zero;
        internal IntPtr m_hWriteFile = IntPtr.Zero;
        internal int m_readposition = 0;
        internal int m_writeposition = 0;
        public MainWindow()
        {
            InitializeComponent();
        }


        private void button_Open_Read_Click(object sender, RoutedEventArgs e)
        {
            var filename = textBox_FileName.Text;
            if (!Utils.FileFolderExistInWindows(filename))
            {
                MessageBox.Show("Please type a valid file name!");
                return;
            }
            uint options = 0;
            if(Button_NoCached.IsChecked ?? false)
            {
                options = options | (uint)EFileAttributes.NoBuffering;
            }
            m_hFile = WIN32API.CreateFile(
                filename,
                System.IO.FileAccess.Read,
                //FILE_SHARE_WRITE | FILE_SHARE_READ | FILE_SHARE_DELETE,
                System.IO.FileShare.Read | System.IO.FileShare.Write | System.IO.FileShare.Delete,
                //IntPtr.Zero,
                IntPtr.Zero,
                //OPEN_EXISTING,
                System.IO.FileMode.Open,
                (System.IO.FileAttributes)options,
                IntPtr.Zero);

            if (m_hFile.ToInt32() == -1)
                MessageBox.Show("Failed to open");
            else MessageBox.Show("Successfully openned the file!");
        }

        private void textBox_FileName_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox.Text == "test.txt")
            {
                textBox.Text = "";
            }

        }

        private void button_GetSize_Click(object sender, RoutedEventArgs e)
        {
            if (m_hFile == IntPtr.Zero)
                MessageBox.Show("Open a valid file first.");

            long dwFileSize;
            if (!WIN32API.GetFileSizeEx(m_hFile, out dwFileSize))
            {
                MessageBox.Show("Failed to get file size");
            }

            string msg = String.Format("{0} Bytes", dwFileSize);
            label_Size.Content = msg;
        }

        private void button_Close_Read_Click(object sender, RoutedEventArgs e)
        {
            if (m_hFile == IntPtr.Zero)
            {
                MessageBox.Show("No file openned");
                return;
            }

            /*
            m_dAddressOffset = 0;
            m_szMemorySize = 0;
            if (m_pStartAddress)
            {
                if (!UnmapViewOfFile(m_pStartAddress))
                {
                    ShowErrorMsgAndCode("unmap file failed ", Marshal.GetLastWin32Error());
                }
                else
                {
                    m_pStartAddress = IntPtr.Zero;
                }

                if (m_hFileMap && !CloseHandle(m_hFileMap))
                {
                    ShowErrorMsgAndCode("unmap file failed ", Marshal.GetLastWin32Error());
                }
                else
                {
                    m_hFileMap = IntPtr.Zero;
                }
            }
            */
            bool ret = WIN32API.CloseHandle(m_hFile);

            if (ret) MessageBox.Show("Closed successfully!");
            else
            {
                int er = Marshal.GetLastWin32Error();
                string msg = String.Format("Failed to close. Error code={0}", er);
                MessageBox.Show(msg);
            }
            m_hFile = IntPtr.Zero;
        }

        private void button_FSeek_Click(object sender, RoutedEventArgs e)
        {
            if (m_hFile == IntPtr.Zero)
            {
                MessageBox.Show("No file open");
                return;
            }

            var m_readfseektype_select = combox_Seek_Read.Text;

            //    uint type = file_begin;
            //    if (m_readfseektype_select == 0) type = file_begin;
            //    else if (m_readfseektype_select == 1) type = file_current;
            //    else if (m_readfseektype_select == 2) type = file_end;
            //    else
            //    {
            //        messagebox.show("type is not set properly. treat is as beging set to file_begin.");
            //    }
            uint seektype = 0;
            switch (m_readfseektype_select)
            {
                case "begin pos": seektype = 0; break;
                case "current pos": seektype = 1; break;
                case "end pos": seektype = 2; break;
                default:break;
            }

            int readpos;
            if (!Int32.TryParse(textBox_EditOffset.Text, out readpos))
            {
                MessageBox.Show("invalid input for the fseek");
                return;
            }
            //    if (m_pstartaddress)
            if(false)
            {
                //if (type == file_end)
                //{
                //    messagebox.show("mapped file does not support fseek from end!");
                //}
                //else if (type == file_current)
                //{
                //    if (m_daddressoffset + pos > m_szmemorysize || m_daddressoffset + pos < 0)
                //    {
                //        messagebox.show("fseek beyond memory space!");
                //    }
                //    else
                //    {
                //        m_daddressoffset += pos;
                //        showerrormsgandcode("succeeded. ", m_daddressoffset);
                //    }
                //}
                //else
                //{
                //    if (pos > m_szmemorysize || pos < 0)
                //    {
                //        messagebox.show("fseek beyond memory space!");
                //    }
                //    else
                //    {
                //        m_daddressoffset = pos;
                //        showerrormsgandcode("succeeded. ", m_daddressoffset);
                //    }
                //}
            }
            else
            {
                int res = (int)WIN32API.SetFilePointer(m_hFile, readpos, IntPtr.Zero, seektype);

                if (res == -1)
                {
                    int er = Marshal.GetLastWin32Error();
                    MessageBox.Show("failed to do seek. errorcode " + er);
                    return;
                }
                else
                {
                    var s = String.Format("Succeeded.  pointer pos = {0}", res);
                    MessageBox.Show(s);
                    m_readposition = res;
                }

            }
        }

        private void Button_Read_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Add your control notification handler code here
            IntPtr hFile = m_hFile!=IntPtr.Zero ? m_hFile : m_hWriteFile;
            if (hFile == IntPtr.Zero)
            {
                MessageBox.Show("No file open.");
                return;
            }

            if (m_readposition < 0)
            {
                MessageBox.Show("Read pos error. Reopen the file and try again.");
                return;
            }

            uint len;
            if ( !UInt32.TryParse(textBox_ReadLen.Text,out len))
            {
                MessageBox.Show("Invalid input for the reading Length");
                return;
            }
            uint dwBytesRead;
            byte[] retval = new byte[len];

            //if (m_pStartAddress != IntPtr.Zero)
            if(false)
            {
                //if (m_dAddressOffset < 0)
                //{
                //    MessageBox.Show(" pos error. Reopen the file and try again.");
                //}
                //else if (m_dAddressOffset >= m_szMemorySize)
                //{
                //    MessageBox.Show(" reach the end of the memory space!");
                //}
                //else
                //{
                //    //uint errorCode;
                //    //__try
                //    dwBytesRead = len;
                //    if (m_dAddressOffset + len > m_szMemorySize)
                //    {
                //        dwBytesRead = m_szMemorySize - m_dAddressOffset;
                //    }

                //    try
                //    {
                //        CopyMemory(retval, (PCH)m_pStartAddress + m_dAddressOffset, dwBytesRead);
                //        m_dAddressOffset += dwBytesRead;

                //        CString hs = bytesToHex(retval, dwBytesRead);

                //        m_Edit_ReadResult.SetWindowTextW(hs);

                //        CString s;
                //        s.Format("%d bytes required, %d bytes returned", len, dwBytesRead);
                //        MessageBox.Show("Succeeded. " + s);
                //    }
                //    //__except (GetExceptionCode() == EXCEPTION_IN_PAGE_ERROR ? 
                //    //EXCEPTION_EXECUTE_HANDLER : EXCEPTION_CONTINUE_SEARCH)
                //    catch (int errorCode)
                //    {
                //        // Failed to write to the view.
                //        //errorCode = GetExceptionCode();
                //        ShowErrorMsgAndCode("an paging error happened when writing to mapped file!", errorCode);
                //    }
                //}
            }
            else
            {

                bool res = WIN32API.ReadFile(hFile, retval, len, out dwBytesRead, IntPtr.Zero);

                if (res)
                {
                    string hs = Utils.ByteArrayToString(retval);

                    TextBlock_ReadResult.Text = hs;

                    var s = String.Format("{0} bytes required, {1} bytes returned", len, dwBytesRead);
                    MessageBox.Show("Succeeded. " + s);

                }
                else
                {
                    int er = Marshal.GetLastWin32Error();
                    string s = String.Format("Error code = %d", e);
                    MessageBox.Show("Failed to read. " + s);
                }

            }
            return;

        }

        private void button_Open_Write_Click(object sender, RoutedEventArgs e)
        {
            var filename = textBox_Write_FileName.Text;

            //if(!FileFolderExistInWindows(m_writefilename))
            //{
            //	MessageBox.Show("Please type a valid file name for writing!");
            //	return ;
            //}

            if (m_hWriteFile != IntPtr.Zero)
            {
                MessageBox.Show("File handle has NOT been properly closed yet. Click close before open it again.");
                return;
            }

            int flag = (int)System.IO.FileAttributes.Normal;
            if (Button_WriteNoCached.IsChecked ?? false)
            {
                flag |= (int)EFileAttributes.NoBuffering;
            }
            System.IO.FileMode option = System.IO.FileMode.OpenOrCreate;
            if (Button_Overwrite.IsChecked ?? false)
            {
                option = System.IO.FileMode.Create;
            }
            StringBuilder dirName = new StringBuilder((int)WIN32API.MAX_DEEP_PATH + 3);
            //TCHAR dirName[256];
            uint rlen = WIN32API.GetCurrentDirectory(WIN32API.MAX_DEEP_PATH, dirName);
            if (rlen == 0)
            {
                int lastError = Marshal.GetLastWin32Error();
                MessageBox.Show("Failed to get initial working directory; error = " + lastError);
                return;
            }

            if (rlen > WIN32API.MAX_DEEP_PATH)
            {
                MessageBox.Show(String.Format("Failed to get initial working directory; allocated buffer is shorter than required: '{0}'<'{1}'", WIN32API.MAX_DEEP_PATH, rlen));
                return;
            }

            int erroCode = 0;
            // extend name
            if (Button_ExtendName.IsChecked ?? false)
            {
                // start with
                filename = dirName + "\\" + filename;

                if (!filename.StartsWith("\\\\"))
                {
                    // not start with, network drive 
                    if (filename.StartsWith("\\\\?\\"))
                    {
                        filename = "\\\\?\\UNC" + filename.Substring(1);
                    }
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

                StringBuilder shortName = new StringBuilder(256);
                uint len = WIN32API.GetShortPathName(filename, shortName, 256);
                if (len > 0)
                {
                    filename = shortName.ToString();
                }
                else
                {
                    erroCode = Marshal.GetLastWin32Error();
                    MessageBox.Show("Failed to get short name.");
                    return;
                }
            }

            m_hWriteFile = WIN32API.CreateFile(
                filename,
                //GENERIC_WRITE | GENERIC_READ,
                System.IO.FileAccess.ReadWrite,
                //FILE_SHARE_WRITE | FILE_SHARE_READ | FILE_SHARE_DELETE,
                System.IO.FileShare.ReadWrite | System.IO.FileShare.Delete,
                IntPtr.Zero,
                option,
                (System.IO.FileAttributes)(flag | (int)System.IO.FileOptions.WriteThrough),
                IntPtr.Zero);

            if (m_hWriteFile.ToInt32() == -1)
                MessageBox.Show("Failed to open for write.");
            else MessageBox.Show("Successfully openned the file! " + filename);
        }

        private void button_Close_Write_Click(object sender, RoutedEventArgs e)
        {
            if (m_hWriteFile == IntPtr.Zero)
            {
                MessageBox.Show("No file openned");
                return;
            }

            //m_dAddressOffset = 0;
            //m_szMemorySize = 0;
            //if (m_pStartAddress)
            if(false)
            {
                //if (!UnmapViewOfFile(m_pStartAddress))
                //{
                //    ShowErrorMsgAndCode("unmap file failed ", GetLastError());
                //}
                //else
                //{
                //    m_pStartAddress = IntPtr.Zero;
                //}

                //if (m_hFileMap && !CloseHandle(m_hFileMap))
                //{
                //    ShowErrorMsgAndCode("unmap file failed ", GetLastError());
                //}
                //else
                //{
                //    m_hFileMap = IntPtr.Zero;
                //}
            }

            bool ret = WIN32API.CloseHandle(m_hWriteFile);

            if (ret) MessageBox.Show("Closed successfully!");
            else
            {
                int er = Marshal.GetLastWin32Error();
                MessageBox.Show(String.Format("Failed to close.Error code = {0}",er));
            }
            m_hWriteFile = IntPtr.Zero;
        }

        private void Button_Write_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Add your control notification handler code here
            if (m_hWriteFile == IntPtr.Zero)
            {
                MessageBox.Show("No file open.");
                return;
            }

            if (m_writeposition < 0)
            {
                MessageBox.Show(" pos error. Reopen the file and try again.");
                return;
            }

            string writecontent = TextBlock_WriteHex.Text;
            byte[] buf = Utils.StringToByteArray(writecontent);

            if (buf.Length == 0)
            {
                MessageBox.Show("Input content must be hex string. Invalid char detected.");
                return;
            }


            uint repeatTime;
            if (!UInt32.TryParse(textBox_WriteLen.Text, out repeatTime))
            {
                repeatTime = 1;
            }

            if (repeatTime > 1)
            {
                byte[] nBuf = new byte[buf.Length * repeatTime];
                for (int i = 0; i < repeatTime; i++)
                {
                    //CopyMemory(nBuf + i * len, buf, len);
                    Array.Copy(buf, 0, nBuf, i * buf.Length, buf.Length);
                }
                buf = nBuf;
            }

            uint dwb;

            // write to mapped buffer
            //if (m_pStartAddress != IntPtr.Zero)
            if(false)
            {
                //if (m_dAddressOffset < 0)
                //{
                //    MessageBox.Show(" pos error. Reopen the file and try again.");
                //}
                //else if (m_dAddressOffset >= m_szMemorySize)
                //{
                //    MessageBox.Show(" reach the end of memory space! ");
                //}
                //else
                //{
                //    //DWORD errorCode;
                //    //__try
                //    dwb = len;
                //    if (m_dAddressOffset + len > m_szMemorySize)
                //    {
                //        dwb = m_szMemorySize - m_dAddressOffset;
                //    }

                //    try
                //    {
                //        CopyMemory((PCH)m_pStartAddress + m_dAddressOffset, buf, dwb);
                //        m_dAddressOffset += dwb;

                //        CString s;
                //        s.Format("%d bytes sent, %d bytes written", len, dwb);
                //        MessageBox.Show("Writing succeeded. " + s);
                //    }
                //    //__except (GetExceptionCode() == EXCEPTION_IN_PAGE_ERROR ? 
                //    //EXCEPTION_EXECUTE_HANDLER : EXCEPTION_CONTINUE_SEARCH)
                //    catch (int errorCode)
                //    {
                //        // Failed to write to the view.
                //        //errorCode = GetExceptionCode();
                //        ShowErrorMsgAndCode("an paging error happened when writing to mapped file!", errorCode);
                //    }
                //}
            }
            else
            {


                bool success = WIN32API.WriteFile(m_hWriteFile, buf, (uint)buf.Length, out dwb, IntPtr.Zero);

                if (success)
                {
                    var s = String.Format("{0} bytes sent, {1} bytes written", buf.Length, dwb);
                    MessageBox.Show("Writing succeeded. " + s);
                }
                else
                {
                    int er = Marshal.GetLastWin32Error();
                    var s = String.Format("Error code = {0}", e);
                    MessageBox.Show("Failed to write. " + s);

                }

            }
            return;
        }

        private void button_FSeek_Write_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Add your control notification handler code here
            if (m_hWriteFile == IntPtr.Zero)
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

            var m_writefseektype_select = combox_Seek_Write.Text;

            //    uint type = file_begin;
            //    if (m_readfseektype_select == 0) type = file_begin;
            //    else if (m_readfseektype_select == 1) type = file_current;
            //    else if (m_readfseektype_select == 2) type = file_end;
            //    else
            //    {
            //        messagebox.show("type is not set properly. treat is as beging set to file_begin.");
            //    }
            uint seektype = 0;
            switch (m_writefseektype_select)
            {
                case "begin pos": seektype = 0; break;
                case "current pos": seektype = 1; break;
                case "end pos": seektype = 2; break;
                default: break;
            }

            int pos;
            if(!Int32.TryParse(textBox_EditOffset_Write.Text, out pos))
            {
                MessageBox.Show("Invalid input for the position");
                return;
            }

            //if (m_pStartAddress)
            if(false)
            {
                //if (type == FILE_END)
                //{
                //    MessageBox.Show("mapped file does not support fseek from end!");
                //}
                //else if (type == FILE_CURRENT)
                //{
                //    if (m_dAddressOffset + pos > m_szMemorySize || m_dAddressOffset + pos < 0)
                //    {
                //        MessageBox.Show("fseek beyond memory space!");
                //    }
                //    else
                //    {
                //        m_dAddressOffset += pos;
                //        ShowErrorMsgAndCode("Succeeded. ", m_dAddressOffset);
                //    }
                //}
                //else
                //{
                //    if (pos > m_szMemorySize || pos < 0)
                //    {
                //        MessageBox.Show("fseek beyond memory space!");
                //    }
                //    else
                //    {
                //        m_dAddressOffset = pos;
                //        ShowErrorMsgAndCode("Succeeded. ", m_dAddressOffset);
                //    }
                //}
            }
            else
            {


                uint res = WIN32API.SetFilePointer(m_hWriteFile, pos, IntPtr.Zero, seektype);

                if ((int)res == -1)
                {
                    int er = Marshal.GetLastWin32Error();
                    var s = String.Format("Error code = {0}", e);
                    MessageBox.Show("Failed to do seek. " + s);
                    return;
                }
                else
                {
                    var s = String.Format(" pointer pos = {0}", res);
                    MessageBox.Show("Succeeded. " + s);

                    m_writeposition = (int)res;
                }

            }
        }

        private void button_Map_Click(object sender, RoutedEventArgs e)
        {
            if (m_hWriteFile == NULL && m_hFile == NULL)
            {
                MessageBox.Show("no valid file handle has been opened!");
            }
            else
            {
                if (!UInt32.TryParse())
                {

                }
                if (dSize < 0) MessageBox.Show("size must be non-negative");

                HANDLE handle = (m_hWriteFile == NULL) ? m_hFile : m_hWriteFile;
                DWORD options = (m_hWriteFile == handle) ? PAGE_READWRITE : PAGE_READONLY;
                m_hFileMap = CreateFileMapping(handle, NULL, options, (dSize >> 32) & 0xFFFF, dSize & 0xFFFF, "test");

                if (m_hFileMap == NULL)
                {
                    ShowErrorMsgAndCode("error happened when creating file map ", GetLastError());
                }
                else
                {
                    m_pStartAddress = MapViewOfFile(
                        m_hFileMap,
                        options == PAGE_READWRITE ? FILE_MAP_WRITE : FILE_MAP_READ,
                        0,
                        0,
                        dSize);
                    if (m_pStartAddress == NULL)
                    {
                        ShowErrorMsgAndCode("error happened when mapping this file ", GetLastError());
                    }
                    else
                    {
                        m_dAddressOffset = 0;
                        m_szMemorySize = 0;

                        MEMORY_BASIC_INFORMATION memBasicInfo;

                        if (VirtualQuery(m_pStartAddress, &memBasicInfo, sizeof(MEMORY_BASIC_INFORMATION)) <= 0)
                        {
                            ShowErrorMsgAndCode("error happened when querying mem info ", GetLastError());
                        }
                        else
                        {
                            m_szMemorySize = memBasicInfo.RegionSize;

                            CString msg;
                            msg.Format("succeed mapping this file 0x%lx", (ULONG_PTR)m_pStartAddress);
                            MessageBox.Show(msg);
                        }

                    }

                }
            }
        }
    }
}
