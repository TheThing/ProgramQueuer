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
	/// A simple boolean converter that inverts the boolean value.
	/// </summary>
	[ValueConversion(typeof(bool), typeof(bool))]
	public class BooleanInverter : IValueConverter
	{
		/// <summary>
		/// Convert a boolean value to the inverted value. Seriously, if I have to explain this, then you are in the wrong business.
		/// </summary>
		/// <param name="value">The boolean value to invert.</param>
		/// <param name="targetType">The type of the target value. This property is ignored.</param>
		/// <param name="parameter">Parameter to pass to the converter. This property is ignored.</param>
		/// <param name="culture">A reference to the target CultureInfo. This property is ignored.</param>
		/// <returns>An inverted boolean value.</returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return !(bool)value;
		}

		/// <summary>
		/// Convert an inverted boolean value to it's original value. Basically inverted.
		/// </summary>
		/// <param name="value">The boolean value to invert back.</param>
		/// <param name="targetType">The type of the target value. This property is ignored.</param>
		/// <param name="parameter">Parameter to pass to the converter. This property is ignored.</param>
		/// <param name="culture">A reference to the target CultureInfo. This property is ignored.</param>
		/// <returns>An inverted boolean value.</returns>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return !(bool)value;
		}
	}
}
