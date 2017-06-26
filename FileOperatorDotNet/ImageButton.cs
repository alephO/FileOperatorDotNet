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
    class ImageButton: Button
    {
        public ImageSource BNorImage
        {
            get { return (ImageSource)GetValue(BNorImageProperty); }
            set { SetValue(BNorImageProperty, value); }
        }

        public static readonly DependencyProperty BNorImageProperty =
            DependencyProperty.Register("BNorImage", typeof(ImageSource), typeof(ImageTabItem));

        public ImageSource BHorImage
        {
            get { return (ImageSource)GetValue(BHorImageProperty); }
            set { SetValue(BHorImageProperty, value); }
        }

        public static readonly DependencyProperty BHorImageProperty =
            DependencyProperty.Register("BHorImage", typeof(ImageSource), typeof(ImageTabItem));


        public ImageSource BDownImage
        {
            get { return (ImageSource)GetValue(BDownImageProperty); }
            set { SetValue(BDownImageProperty, value); }
        }

        public static readonly DependencyProperty BDownImageProperty =
            DependencyProperty.Register("BDownImage", typeof(ImageSource), typeof(ImageTabItem));

    }
}
