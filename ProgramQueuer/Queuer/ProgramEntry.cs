using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ProgramQueuer.Queuer
{
	public class ProgramEntry : INotifyPropertyChanged
	{
		bool _error;
		bool _working;
		bool _finished;
		bool _clearNext;
		string _name;
		string _output;
		string _status;
		string _buffer;
		Process _process;
		ProcessIOManager _processManager;

		public ProgramEntry()
		{
			Finished = false;

			_process = new Process();
			_process.StartInfo.UseShellExecute = false;
			_process.EnableRaisingEvents = true;

			_processManager = new ProcessIOManager();
			_processManager.StderrTextRead += new StringReadEventHandler(_processManager_StdoutTextRead);
			_processManager.StdoutTextRead += new StringReadEventHandler(_processManager_StdoutTextRead);
		}

		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				PropertyChanged(this, new PropertyChangedEventArgs("Name"));
			}
		}

		public string Output
		{
			get { return _output; }
			set
			{
				_output = value;
				PropertyChanged(this, new PropertyChangedEventArgs("Output"));
			}
		}

		public string Status
		{
			get { return _status; }
			set
			{
				_status = value;
				PropertyChanged(this, new PropertyChangedEventArgs("Status"));
			}
		}
		
		public bool Finished 
		{
			get { return _finished; }
			set
			{
				_finished = value;
				PropertyChanged(this, new PropertyChangedEventArgs("Finished"));
			}
		}

		public Process Process
		{
			get { return _process; }
			set { _process = value; }
		}

		public ProcessIOManager ProcessManager
		{
			get { return _processManager; }
			set { _processManager = value; }
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

		public bool Error
		{
			get { return _error; }
			set
			{
				_error = value;
				PropertyChanged(this, new PropertyChangedEventArgs("Error"));
			}
		}

		public void StartProcess(bool redirect)
		{
			_process.StartInfo.RedirectStandardOutput = redirect;
			_process.StartInfo.RedirectStandardError = redirect;
			_process.StartInfo.RedirectStandardInput = redirect;
			_process.StartInfo.CreateNoWindow = redirect;
			_process.StartInfo.FileName = this.Name;
			_process.StartInfo.WorkingDirectory = new FileInfo(this.Name).DirectoryName;
			_process.Start();
			if (redirect)
			{
				_processManager.RunningProcess = _process;
				_processManager.StartProcessOutputRead();
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
				if (this.Output.Length > 0 && this.Output[this.Output.Length - 1] != '\n')
					this.Output = this.Output.Remove(this.Output.Length - 1);
				text = text.Remove(text.IndexOf('\b'), 1);
			}

			if (_clearNext && text.Replace("\n", "").Replace("\r", "").Trim() != "")
				if (this.Output.LastIndexOf('\n') < this.Output.Length - 1)
					this.Output = this.Output.Remove(this.Output.LastIndexOf('\n') + 1) + text;
				else
					this.Output += text;
			else
				this.Output += text;

			if (text.Replace("\n", "").Trim() != "")
				this.Status = text.Replace("\n", "").Replace("\r", "");

			if (lines.Length == 2 && lines[1] == "")
				_clearNext = true;
			else
				_clearNext = false;
		}
	}
}
