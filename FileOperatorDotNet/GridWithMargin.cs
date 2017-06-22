using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FileOperatorDotNet
{
    public class GridWithMargins : Grid
    {
        public Thickness RowMargin { get; set; } = new Thickness(1, 1, 1, 1);
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var basesize = base.ArrangeOverride(arrangeSize);

            foreach (UIElement child in InternalChildren)
            {
                var pos = GetPosition(child);
                pos.X += RowMargin.Left;
                pos.Y += RowMargin.Top;

                var actual = child.RenderSize;
                actual.Width -= (RowMargin.Left + RowMargin.Right);
                actual.Height -= (RowMargin.Top + RowMargin.Bottom);
                var rec = new Rect(pos, actual);
                child.Arrange(rec);
            }
            return arrangeSize;
        }

        private Point GetPosition(UIElement element)
        {
            var posTransForm = element.TransformToAncestor(this);
            var areaTransForm = posTransForm.Transform(new Point(0, 0));
            return areaTransForm;
        }
    }
}
