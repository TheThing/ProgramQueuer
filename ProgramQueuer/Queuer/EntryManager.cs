using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace ProgramQueuer.Queuer	
{
	public class EntryManager : INotifyPropertyChanged
	{
		bool _working;
		bool _redirectOutput;
		bool _changed;
		ProgramEntry _currentEntry;

		public EntryManager()
		{
			QueueList = new ObservableCollection<ProgramEntry>();
			_changed = false;
			_currentEntry = null;
			_redirectOutput = true;
		}

		public void Load()
		{
			var oldQueue = ProgramQueuer.Properties.Settings.Default.lastjobs;
			if (oldQueue.Length == 0) return;

			var items = oldQueue.Split(';');
			for (int i = 0; i < items.Length; i++)
			{
				var values = items[i].Split('|');
				var base64EncodedBytes = System.Convert.FromBase64String(values[2]);
				var entry = new ProgramEntry {
				    Name = values[0],
					Finished = values[1] == "true",
					Status = values[1] == "true" ? "Finished" : "Queued",
					Output = System.Text.Encoding.UTF8.GetString(base64EncodedBytes)
				};
				entry.Parent = this;
				this.QueueList.Add(entry);
			}
		}

		public void Save()
		{
			StringBuilder sb = new StringBuilder("");
			for (int i = 0; i < this.QueueList.Count; i++)
			{
				if (i > 0)
				{
					sb.Append(';');
				}
				var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(this.QueueList[i].Output != null ? this.QueueList[i].Output : "");
				sb.Append(String.Format("{0}|{1}|{2}", this.QueueList[i].Name, this.QueueList[i].Finished ? "true" : "false", System.Convert.ToBase64String(plainTextBytes)));
			}
			ProgramQueuer.Properties.Settings.Default.lastjobs = sb.ToString();
			ProgramQueuer.Properties.Settings.Default.Save();
			_changed = false;
		}

		public void TriggerSave()
		{
			if (_changed) return;

			_changed = true;

			var task = new Task(async () =>
			{
				await Task.Delay(60000);
				if (!_changed == false) return;
				_changed = false;
				this.Save();
			});
			task.Start();
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
			var finished = RunNext();
			Working = !finished;
		}

		public void ForceStop()
		{
			this.Working = false;
			foreach (var entry in QueueList)
				if (entry.Working)
				{
					ForceStopEntry(entry);
				}
			this.Save();
		}

		public void ForceStopEntry(ProgramEntry entry)
		{
			KillProcessAndChildren(entry.Process.Id);
			entry.Output += "\n\n[Process was forcefully stopped]";
			// entry.Process.CloseMainWindow();
			// entry.Process.Kill();
		}

		/// <summary>
		/// Kill a process, and all of its children, grandchildren, etc.
		/// </summary>
		/// <param name="pid">Process ID.</param>
		private static void KillProcessAndChildren(int pid)
		{
			// Cannot close 'system idle process'.
			if (pid == 0)
			{
				return;
			}
			ManagementObjectSearcher searcher = new ManagementObjectSearcher
					("Select * From Win32_Process Where ParentProcessID=" + pid);
			ManagementObjectCollection moc = searcher.Get();
			foreach (ManagementObject mo in moc)
			{
				KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
			}
			try
			{
				Process proc = Process.GetProcessById(pid);
				proc.Kill();
			}
			catch (ArgumentException)
			{
				// Process already exited.
			}
		}

		public ProgramEntry AddToQueue(ProgramEntry entry)
		{
			entry.Parent = this;
			this.QueueList.Add(entry);
			this.Save();
			return entry;
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
				entry.Output += string.Format("Error while starting {0}:\n\n{1}", entry.Name, e.ToString());
				if (RunNext())
				{
					_currentEntry = null;
					this.Working = false;
				}
				this.Save();
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
			if (curr.Redirected)
				curr.ProcessManager.StopMonitoringProcessOutput();
			curr.Working = false;
			curr.Finished = true;
			curr.Status = "Finished";

			this.Save();

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
