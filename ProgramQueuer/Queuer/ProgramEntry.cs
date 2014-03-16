using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgramQueuer.Queuer
{
	public class ProgramEntry : INotifyPropertyChanged
	{
		bool _error;
		bool _working;
		bool _finished;
		string _name;
		string _output;
		string _status;

		public ProgramEntry()
		{
			Finished = false;
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
	}
}
