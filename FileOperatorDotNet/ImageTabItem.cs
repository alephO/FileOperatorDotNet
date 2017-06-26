using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileOperatorDotNet
{
    class ImageTabItem : TabItem
    {
        public ImageSource NorImage
        {
            get { return (ImageSource)GetValue(NorImageProperty); }
            set { SetValue(NorImageProperty, value); }
        }

        public static readonly DependencyProperty NorImageProperty =
            DependencyProperty.Register("NorImage", typeof(ImageSource), typeof(ImageTabItem));

        public ImageSource HorImage
        {
            get { return (ImageSource)GetValue(HorImageProperty); }
            set { SetValue(HorImageProperty, value); }
        }

        public static readonly DependencyProperty HorImageProperty =
            DependencyProperty.Register("HorImage", typeof(ImageSource), typeof(ImageTabItem));


        public ImageSource DownImage
        {
            get { return (ImageSource)GetValue(DownImageProperty); }
            set { SetValue(DownImageProperty, value); }
        }

        public static readonly DependencyProperty DownImageProperty =
            DependencyProperty.Register("DownImage", typeof(ImageSource), typeof(ImageTabItem));

    }
}
