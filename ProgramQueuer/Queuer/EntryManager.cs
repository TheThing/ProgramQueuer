using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace ProgramQueuer.Queuer
{
	public class EntryManager : INotifyPropertyChanged
	{
		bool _working;
		bool _redirectOutput;
		ProgramEntry _currentEntry;

		public EntryManager()
		{
			QueueList = new ObservableCollection<ProgramEntry>();
			_currentEntry = null;
			_redirectOutput = true;
		}

		public ObservableCollection<ProgramEntry> QueueList;
		public event PropertyChangedEventHandler PropertyChanged = delegate { };
		public event EventHandler OnEntryFinish = delegate { };

		public ProgramEntry CurrentEntry
		{
			get { return _currentEntry; }
			set
			{
				_currentEntry = value;
				PropertyChanged(this, new PropertyChangedEventArgs("CurrentEntry"));
			}
		}

		public bool Working
		{
			get { return _working; }
			set
			{
				_working = value;
				PropertyChanged(this, new PropertyChangedEventArgs("Working"));
			}
		}

		public bool RedirectOutput
		{
			get { return _redirectOutput; }
			set
			{
				_redirectOutput = value;
				PropertyChanged(this, new PropertyChangedEventArgs("RedirectOutput"));
			}
		}
		public void RunQueuer()
		{
			if (QueueList.Count > 0)
			{
				Working = true;
				_currentEntry = QueueList[0];
				RunProgram(QueueList[0]);
			}
		}

		public void ForceStop()
		{
			this.Working = false;
			foreach (var entry in QueueList)
				if (entry.Working)
					entry.Process.Kill();
		}

		public void RunProgram(ProgramEntry entry)
		{
			try
			{
				entry.Output = "";
				entry.Working = true;
				entry.Process.Exited += _currentProcess_Exited;
				entry.StartProcess(_redirectOutput);
				entry.Status = "Running";
			}
			catch (Exception e)
			{
				entry.Working = false;
				entry.Finished = false;
				entry.Status = string.Format("Error while starting: {0}", e.Message);
				entry.Output += string.Format("Error while starting {0}:\n\n{1}", _currentEntry.Name, e.ToString());
				if (RunNext())
				{
					_currentEntry = null;
					this.Working = false;
				}
			}
		}

		private bool RunNext()
		{
			for (int i = 0; i < QueueList.Count; i++)
			{
				if (!QueueList[i].Finished && !QueueList[i].Working)
				{
					_currentEntry = QueueList[i];
					RunProgram(_currentEntry);
					return false;
				}
			}
			return true;
		}

		void _currentProcess_Exited(object sender, EventArgs e)
		{
			string path = (sender as Process).StartInfo.FileName;
			ProgramEntry curr = null;
			foreach (var entry in QueueList)
			{
				if (entry.Process == sender as Process)
				{
					curr = entry;
					break;
				}
			}
			if (_redirectOutput)
				curr.ProcessManager.StopMonitoringProcessOutput();
			curr.Working = false;
			curr.Finished = true;
			curr.Status = "Finished";

			OnEntryFinish(curr, new EventArgs());
			
			if (curr == _currentEntry)
				if (!this.Working)
					return;
				else if (RunNext())
				{
					_currentEntry = null;
					this.Working = false;
				}
		}
	}
}
