using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProgramQueuer.Queuer;
using WPF.JoshSmith.ServiceProviders.UI;

namespace ProgramQueuer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		EntryManager _manager;

		public MainWindow()
		{
			InitializeComponent();

			_manager = new EntryManager();
			this.DataContext = _manager;
			this.listPrograms.ItemsSource = _manager.QueueList;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			new ListViewDragDropManager<ProgramEntry>(this.listPrograms);
		}

		private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
		{
			ProgramQueuer.Properties.Settings.Default.split_height = (int)((sender as GridSplitter).Parent as Grid).RowDefinitions[2].Height.Value;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (_manager.Working)
				if (MessageBox.Show("Are you sure you want to stop current worker and batch and exit application?", "You sure you want to exit?", MessageBoxButton.YesNo) == MessageBoxResult.No)
				{
					e.Cancel = true;
					return;
				}
			ProgramQueuer.Properties.Settings.Default.Save();
		}

		private void ButtonExit_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void ButtonWork_Click(object sender, RoutedEventArgs e)
		{
			_manager.RunQueuer();
		}

		private void ButtonStop_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Are you sure you want to stop current batch?", "Stop worker", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				_manager.ForceStop();
			}
		}

		private void ListView_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effects = DragDropEffects.Link;
			else
				e.Effects = DragDropEffects.None;
		}

		private void ListView_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
				foreach (string file in files)
				{
					_manager.QueueList.Add(new ProgramEntry { Name = file , Status = "Queued"});
				}
			}
		}

		private void buttonRemove_Click(object sender, RoutedEventArgs e)
		{
			_manager.QueueList.Remove((sender as Control).DataContext as ProgramEntry);
		}

		private void buttonStopCurrent_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Are you sure you want to stop current running process and continue to next?", "Stop current worker?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				_manager.ForceStopCurrent();
			}
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			textboxStatus.CaretIndex = textboxStatus.Text.Length;
			textboxStatus.ScrollToEnd();
		}

		private void buttonAdd_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
