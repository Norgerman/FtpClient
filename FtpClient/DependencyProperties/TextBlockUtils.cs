using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FtpClient.DependencyProperties
{
    public static class TextBlockUtils
    {
        /// <summary>
        /// Gets the value of the AutoTooltipProperty dependency property
        /// </summary>
        /// 
        /// <summary>
        /// Identified the attached AutoTooltip property. When true, this will set the
        /// TextBlock TextTrimming property to WordEllipsis, and display a tooltip with
        /// the full text whenever the text is trimmed.
        /// </summary>
        public static readonly DependencyProperty AutoTooltipProperty =
                 DependencyProperty.RegisterAttached("AutoTooltip", typeof(bool), typeof(TextBlockUtils),
                  new PropertyMetadata(false, OnAutoTooltipPropertyChanged));

        public static bool GetAutoTooltip(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoTooltipProperty);
        }

        /// <summary>
        /// Sets the value of the AutoTooltipProperty dependency property
        /// </summary>
        public static void SetAutoTooltip(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoTooltipProperty, value);
        }

        private static void OnAutoTooltipPropertyChanged(DependencyObject d,
                                           DependencyPropertyChangedEventArgs e)
        {
            TextBlock textBlock = d as TextBlock;
            if (textBlock == null)
                return;

            if (e.NewValue.Equals(true))
            {
                textBlock.TextTrimming = TextTrimming.WordEllipsis;
                textBlock.MouseEnter += TextBlock_MouseEnter;
            }
            else
            {
                textBlock.MouseEnter -= TextBlock_MouseEnter;
            }
        }

        private static void TextBlock_MouseEnter(object sender, RoutedEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            ComputeAutoTooltip(textBlock);
        }

        /// <summary>
        /// Assigns the ToolTip for the given TextBlock based on whether the text is trimmed
        /// </summary>
        private static void ComputeAutoTooltip(TextBlock textBlock)
        {
            Typeface typeface = new Typeface(textBlock.FontFamily, textBlock.FontStyle,
                textBlock.FontWeight, textBlock.FontStretch);
            FormattedText formatedText = new FormattedText(textBlock.Text, 
                System.Threading.Thread.CurrentThread.CurrentCulture,
                textBlock.FlowDirection, typeface, textBlock.FontSize, textBlock.Foreground);
            if (textBlock.ActualWidth < formatedText.Width)
            {
                ToolTipService.SetToolTip(textBlock, textBlock.Text);
            }
            else
            {
                ToolTipService.SetToolTip(textBlock, null);
            }
            typeface = null;
            formatedText = null;
        }
    }
}
