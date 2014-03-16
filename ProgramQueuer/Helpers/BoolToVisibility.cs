using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ProgramQueuer.Helpers
{
	/// <summary>
	/// Simple boolean value converter that converts it to a visbility value where true is visible and false is collapsed.
	/// </summary>
	[ValueConversion(typeof(bool), typeof(Visibility))]
	public class BoolToVisibility : IValueConverter
	{
		/// <summary>
		/// Convert from a boolean value of true to Visibility.Visible and false to Visibility.Collapsed.
		/// </summary>
		/// <param name="value">A boolean value to convert.</param>
		/// <param name="targetType">The type of the target value. This property is ignored.</param>
		/// <param name="parameter">Parameter to pass to the converter. This property is ignored.</param>
		/// <param name="culture">A reference to the target CultureInfo. This property is ignored.</param>
		/// <returns>A Visibility value of either Visible or Collapsed depending on the value parameter.</returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			bool visible = (bool)value;
			if (visible)
				return Visibility.Visible;
			return Visibility.Collapsed;
		}

		/// <summary>
		/// Convert back from a Visibility value to a boolean value where Visibility.Visible converts to true and Visibility.Collapsed to false.
		/// </summary>
		/// <param name="value">The visibility value to convert back to a boolean value.</param>
		/// <param name="targetType">The type of the target value. This property is ignored.</param>
		/// <param name="parameter">Parameter to pass to the converter. This property is ignored.</param>
		/// <param name="culture">A reference to the target CultureInfo. This property is ignored.</param>
		/// <returns>A boolean value representing the visibility of the control.</returns>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var visible = (Visibility)value;
			if (visible == Visibility.Collapsed)
				return false;
			return true;
		}
	}
}
