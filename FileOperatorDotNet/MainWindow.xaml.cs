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
        internal IntPtr m_hFile;
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
            /*
            m_readfseektype_select = m_readfseek_type.GetCurSel();

            DWORD type = FILE_BEGIN;
            if (m_readfseektype_select == 0) type = FILE_BEGIN;
            else if (m_readfseektype_select == 1) type = FILE_CURRENT;
            else if (m_readfseektype_select == 2) type = FILE_END;
            else
            {
                MessageBox.Show("type is not set properly. Treat is as beging set to file_begin.");
            }

            CString readPos;
            m_Edit_ReadPos.GetWindowText(readPos);
            LONG pos = CString_2_int(readPos);

            if (readPos.IsEmpty())
            {
                MessageBox.Show("Invalid input for the fseek");
                return;
            }

            if (m_pStartAddress)
            {
                if (type == FILE_END)
                {
                    MessageBox.Show("mapped file does not support fseek from end!");
                }
                else if (type == FILE_CURRENT)
                {
                    if (m_dAddressOffset + pos > m_szMemorySize || m_dAddressOffset + pos < 0)
                    {
                        MessageBox.Show("fseek beyond memory space!");
                    }
                    else
                    {
                        m_dAddressOffset += pos;
                        ShowErrorMsgAndCode("Succeeded. ", m_dAddressOffset);
                    }
                }
                else
                {
                    if (pos > m_szMemorySize || pos < 0)
                    {
                        MessageBox.Show("fseek beyond memory space!");
                    }
                    else
                    {
                        m_dAddressOffset = pos;
                        ShowErrorMsgAndCode("Succeeded. ", m_dAddressOffset);
                    }
                }
            }
            else
            {

                DWORD res = SetFilePointer(m_hFile, pos, NULL, type);

                if (res == INVALID_SET_FILE_POINTER)
                {
                    DWORD e = GetLastError();
                    CString s;
                    s.Format("Error code = %d", e);
                    MessageBox.Show("Failed to do seek. " + s);
                    return;
                }
                else
                {
                    CString s;
                    s.Format(" pointer pos = %d", res);
                    MessageBox.Show("Succeeded. " + s);

                    m_readposition = res;
                }

            }*/
        }
    }
}
