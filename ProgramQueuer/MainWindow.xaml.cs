using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Resources;
using System.Windows.Threading;
using ProgramQueuer.Queuer;
using WPF.JoshSmith.ServiceProviders.UI;
using Microsoft.Win32;
using NotifyIcon = System.Windows.Forms.NotifyIcon;

namespace ProgramQueuer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		EntryManager _manager;
		OpenFileDialog _openFile;
		NotifyIcon _icon;

		public MainWindow()
		{
			InitializeComponent();

			_manager = new EntryManager();
			_openFile = new OpenFileDialog { Filter = "All Files (*.*)|*.*",
											 CheckFileExists = true,
											 Multiselect = true,
											 InitialDirectory = ProgramQueuer.Properties.Settings.Default.lastPath };
			this._icon = new NotifyIcon { Icon = Properties.Resources.program, Visible = true };
			this._icon.MouseClick += new System.Windows.Forms.MouseEventHandler(_icon_MouseClick);
			this.DataContext = _manager;
			this.listPrograms.ItemsSource = _manager.QueueList;
			mainGrid.RowDefinitions[2].Height = new GridLength(ProgramQueuer.Properties.Settings.Default.split_height);
		}

		void _icon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			this.Visibility = System.Windows.Visibility.Visible;
			Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
				new Action(delegate()
				{
					this.WindowState = WindowState.Normal;
					this.Activate();
				})
			);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			_manager.RedirectOutput = ProgramQueuer.Properties.Settings.Default.redirectOutput;
			_manager.Load();
			new ListViewDragDropManager<ProgramEntry>(this.listPrograms);
		}

		private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
		{
			ProgramQueuer.Properties.Settings.Default.split_height = mainGrid.RowDefinitions[2].ActualHeight;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			bool working = false;
			for (int i = 0; i < _manager.QueueList.Count; i++)
			{
				if (_manager.QueueList[i].Working)
				{
					working = true;
					break;
				}
			}
			if (working)
				if (MessageBox.Show("Are you sure you want to stop current worker and batch and exit application?", "You sure you want to exit?", MessageBoxButton.YesNo) == MessageBoxResult.No)
				{
					e.Cancel = true;
					return;
				}
				else
					_manager.ForceStop();
			this._icon.Visible = false;
			ProgramQueuer.Properties.Settings.Default.redirectOutput = _manager.RedirectOutput;
			ProgramQueuer.Properties.Settings.Default.lastPath = _openFile.InitialDirectory;
			_manager.Save();
			// ProgramQueuer.Properties.Settings.Default.Save();
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
					_manager.AddToQueue(new ProgramEntry { Name = file , Status = "Queued"});
					_openFile.InitialDirectory = new FileInfo(file).DirectoryName;
					_manager.Save();
				}
			}
		}

		private void buttonRemove_Click(object sender, RoutedEventArgs e)
		{
			_manager.QueueList.Remove((sender as Control).DataContext as ProgramEntry);
		}

		private void buttonStopCurrent_Click(object sender, RoutedEventArgs e)
		{
			if (_manager.CurrentEntry == ((sender as Control).DataContext as ProgramEntry))
			{
				if (MessageBox.Show("Are you sure you want to kill this process and continue with the next one available?", "Stop current process?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
				{
					_manager.ForceStopEntry((sender as Control).DataContext as ProgramEntry);
				}
			}
			else
			{
				if (MessageBox.Show("Are you sure you want to kill this process?", "Stop selected process?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
				{
					_manager.ForceStopEntry((sender as Control).DataContext as ProgramEntry);
				}
			}
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			textboxStatus.CaretIndex = textboxStatus.Text.Length;
			textboxStatus.ScrollToEnd();
		}

		private void buttonAdd_Click(object sender, RoutedEventArgs e)
		{
			if (_openFile.ShowDialog() == true)
			{
				foreach (string file in _openFile.FileNames)
				{
					_manager.AddToQueue(new ProgramEntry { Name = file, Status = "Queued" });
					_openFile.InitialDirectory = new FileInfo(file).DirectoryName;
				}
			}
		}

		private void checkboxOverrideOutput_Checked(object sender, RoutedEventArgs e)
		{
			if (textboxStatus != null)
				textboxStatus.Visibility = Visibility.Visible;
		}

		private void checkboxOverrideOutput_Unchecked(object sender, RoutedEventArgs e)
		{
			if (textboxStatus != null)
				textboxStatus.Visibility = Visibility.Collapsed;
		}

		private void ButtonHelp_Click(object sender, RoutedEventArgs e)
		{
			// popupHelp.IsOpen = true;
		}

		private void buttonStartCurrent_Click(object sender, RoutedEventArgs e)
		{
			_manager.RunProgram((sender as Control).DataContext as ProgramEntry);
		}

		private void buttonClear_Click(object sender, RoutedEventArgs e)
		{
			popupEmpty.IsOpen = true;/*
			*/
		}

		private void Window_StateChanged(object sender, EventArgs e)
		{
			if (this.WindowState == System.Windows.WindowState.Minimized)
			{
				this.Visibility = System.Windows.Visibility.Collapsed;
			}
		}

		private void buttonClearFinished_Click(object sender, RoutedEventArgs e)
		{
			bool changed = false;
			for (int i = 0; i < _manager.QueueList.Count; i++)
			{
				if (_manager.QueueList[i].Finished == true)
				{
					changed = true;
					_manager.QueueList.Remove(_manager.QueueList[i]);
					i--;
				}
			}
			if (changed)
			{
				_manager.Save();
			}
			popupEmpty.IsOpen = false;
		}

		private void buttonClearAll_Click(object sender, RoutedEventArgs e)
		{
			for (int i = 0; i < _manager.QueueList.Count; i++)
			{
				if (_manager.QueueList[i].Working)
				{
					MessageBox.Show("Cannot clear list while program are still in working mode. Please stop all running instances.", "Processes still running.");
					return;
				}
			}
			if (MessageBox.Show("Are you sure you want to clear list?", "Clear list?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				_manager.QueueList.Clear();
				_manager.Save();
			}
			popupEmpty.IsOpen = false;
		}

		private void expanderStatus_Expanded(object sender, RoutedEventArgs e)
		{
			mainGrid.RowDefinitions[2].Height = new GridLength(ProgramQueuer.Properties.Settings.Default.split_height);
			gridSplitter.Visibility = Visibility.Visible;
		}

		private void expanderStatus_Collapsed(object sender, RoutedEventArgs e)
		{
			mainGrid.RowDefinitions[2].Height = new GridLength(0, GridUnitType.Auto);
			gridSplitter.Visibility = Visibility.Collapsed;
		}

		private void buttonResetCurrent_Click(object sender, RoutedEventArgs e)
		{
			var entry = (sender as Control).DataContext as ProgramEntry;
			entry.Finished = false;
			entry.Status = "Queued";
			_manager.Save();
		}

		private void buttonMarkFinishedCurrent_Click(object sender, RoutedEventArgs e)
		{
			var entry = (sender as Control).DataContext as ProgramEntry;
			entry.Finished = true;
			entry.Status = "Marked finished";
			_manager.Save();
		}
	}
}
