﻿using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
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

            for (var i = 0; i < searchItemModel.DisplayName.Length; i++)
            {
                if (searchItemModel.HighlightIndexes.ContainsKey(i))
                {
                    var run = new Run(searchItemModel.DisplayName[i].ToString());
                    run.Foreground = Brushes.DeepSkyBlue;
                    textBlock.Inlines.Add(run);
                }
                else
                {
                    var run = new Run(searchItemModel.DisplayName[i].ToString());
                    textBlock.Inlines.Add(run);
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