using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace AlphaLaunch.App.Converters
{
    public class HighlightValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var searchItemModel = value as SearchItemModel;

            if (searchItemModel == null)
            {
                return value;
            }

            var textBlock = new TextBlock();

            string currentString = string.Empty;
            bool isHighlight = false;

            for (var i = 0; i < searchItemModel.Name.Length; i++)
            {
                if (searchItemModel.HighlightIndexes.Contains(i))
                {
                    if (!isHighlight)
                    {
                        textBlock.Inlines.Add(new Run(currentString));
                        currentString = string.Empty;
                    }

                    isHighlight = true;
                    currentString += searchItemModel.Name[i];
                }
                else
                {
                    if (isHighlight)
                    {
                        textBlock.Inlines.Add(new Run(currentString) { Foreground = Brushes.DeepSkyBlue });
                        currentString = string.Empty;
                    }

                    isHighlight = false;
                    currentString += searchItemModel.Name[i];
                }
            }

            if (currentString.Any())
            {
                if (isHighlight)
                {
                    textBlock.Inlines.Add(new Run(currentString) { Foreground = Brushes.DeepSkyBlue });
                }
                else
                {
                    textBlock.Inlines.Add(new Run(currentString));
                }
            }

            return textBlock;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}