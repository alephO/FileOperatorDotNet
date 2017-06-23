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
                //NULL,
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
                    m_pStartAddress = NULL;
                }

                if (m_hFileMap && !CloseHandle(m_hFileMap))
                {
                    ShowErrorMsgAndCode("unmap file failed ", Marshal.GetLastWin32Error());
                }
                else
                {
                    m_hFileMap = NULL;
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

            //if (m_pStartAddress != NULL)
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

            uint flag = (uint)System.IO.FileAttributes.Normal;
            if (Button_WriteNoCached.IsChecked ?? false)
            {
                flag |= (uint)EFileAttributes.NoBuffering;
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
                MessageBox.Show("Failed to get initial working directory; allocated buffer is shorter than required: '{0}'<'{1}'", MAX_DEEP_PATH, folderNameLength);
                return;
            }

            uint erroCode = 0;
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

                TCHAR shortName[256];
                uint len = GetShortPathName((LPCWSTR)m_writefilename, shortName, 256);
                if (len > 0)
                {
                    m_writefilename.Format("%s", shortName);
                }
                else
                {
                    erroCode = Marshal.GetLastWin32Error();
                    MessageBox.Show("Failed to get short name.");
                    return;
                }
            }

            m_hwriteFile = CreateFile(
                m_writefilename,
                GENERIC_WRITE | GENERIC_READ,
                FILE_SHARE_WRITE | FILE_SHARE_READ | FILE_SHARE_DELETE,
                NULL,
                option,
                flag | FILE_FLAG_WRITE_THROUGH,
                NULL);

            if (m_hwriteFile == INVALID_HANDLE_VALUE)
                MessageBox.Show("Failed to open for write.");
            else MessageBox.Show("Successfully openned the file! " + m_writefilename);
        }
    }
}
