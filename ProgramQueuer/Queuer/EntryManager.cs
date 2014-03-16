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
		bool _clearNext;
		string _buffer;
		ProgramEntry _currentEntry;
		Process _currentProcess;
		ProcessIOManager _processManager;

		public EntryManager()
		{
			QueueList = new ObservableCollection<ProgramEntry>();
			_buffer = "";
			_currentEntry = null;
			_processManager = new ProcessIOManager();
			_processManager.StderrTextRead += new StringReadEventHandler(_processManager_StdoutTextRead);
			_processManager.StdoutTextRead += new StringReadEventHandler(_processManager_StdoutTextRead);
			_currentProcess = new Process();
			_currentProcess.StartInfo.UseShellExecute = false;
			_currentProcess.StartInfo.RedirectStandardOutput = true;
			_currentProcess.StartInfo.RedirectStandardError = true;
			_currentProcess.StartInfo.RedirectStandardInput = true;
			_currentProcess.StartInfo.CreateNoWindow = true;
			_currentProcess.EnableRaisingEvents = true;
			_currentProcess.Exited += new EventHandler(_currentProcess_Exited);
		}

		public ObservableCollection<ProgramEntry> QueueList;
		public event PropertyChangedEventHandler PropertyChanged = delegate { };

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

		public void RunQueuer()
		{
			if (QueueList.Count > 0)
			{
				Working = true;
				RunProgram(QueueList[0]);
			}
		}

		public void ForceStopCurrent()
		{
			_currentProcess.Kill();
		}

		public void ForceStop()
		{
			this.Working = false;
			_currentProcess.Kill();
		}

		private void RunProgram(ProgramEntry entry)
		{
			try
			{
				_currentEntry = entry;
				_currentEntry.Output = "";
				_currentEntry.Working = true;
				_currentEntry.Status = "Starting";
				_currentProcess.StartInfo.FileName = _currentEntry.Name;
				_currentProcess.StartInfo.WorkingDirectory = new FileInfo(_currentEntry.Name).DirectoryName;
				_currentProcess.Start();
				_processManager.RunningProcess = _currentProcess;
				_processManager.StartProcessOutputRead();
			}
			catch (Exception e)
			{
				_currentEntry.Working = false;
				_currentEntry.Finished = false;
				_currentEntry.Output += string.Format("Error while starting {0}:\n\n{1}", _currentEntry.Name, e.ToString());
				if (RunNext())
				{
					_currentEntry = null;
					this.Working = false;
				}
			}
		}

		private bool RunNext()
		{
			_currentEntry.Status = "Finished";
			if (QueueList.IndexOf(_currentEntry) >= 0 && QueueList.IndexOf(_currentEntry) < QueueList.Count - 1)
				RunProgram(QueueList[QueueList.IndexOf(_currentEntry) + 1]);
			else
				return true;
			return false;
		}

		void _currentProcess_Exited(object sender, EventArgs e)
		{
			_processManager.StopMonitoringProcessOutput();
			_currentEntry.Working = false;
			_currentEntry.Finished = true;
			if (!this.Working)
				return;
			if (RunNext())
			{
				_currentEntry = null;
				this.Working = false;
			}
		}

		void _processManager_StdoutTextRead(string text)
		{
			string[] lines = text.Split('\r');
			if (!text.EndsWith("\r") && !text.EndsWith("\n") && _clearNext)
			{
				_buffer += text;
				return;
			}
			else
			{
				text = _buffer + text;
				_buffer = "";
			}
			

			if (_clearNext && text == "\n")
				_clearNext = false;

			while (text.IndexOf('\b') >= 0)
			{
				if (_currentEntry.Output.Length > 0 && _currentEntry.Output[_currentEntry.Output.Length - 1] != '\n')
					_currentEntry.Output = _currentEntry.Output.Remove(_currentEntry.Output.Length - 1);
				text = text.Remove(text.IndexOf('\b'), 1);
			}

			if (_clearNext && text.Replace("\n", "").Replace("\r", "").Trim() != "")
				if (_currentEntry.Output.LastIndexOf('\n') < _currentEntry.Output.Length - 1)
					_currentEntry.Output = _currentEntry.Output.Remove(_currentEntry.Output.LastIndexOf('\n') + 1) + text;
				else
					_currentEntry.Output += text;
			else
				_currentEntry.Output += text;

			if (text.Replace("\n", "").Trim() != "")
				_currentEntry.Status = text.Replace("\n", "").Replace("\r", "");

			if (lines.Length == 2 && lines[1] == "")
				_clearNext = true;
			else
				_clearNext = false;
		}
	}
}
