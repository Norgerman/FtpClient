using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace FtpClient.DataModel
{
    public class FtpInfo : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private const string PORT_ERROR = "Port in invalid range";
        private const string HOST_ERROR = "Host cannot be empty";

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private string _userName = "ftp";
        private string _password = "ftp";
        private string _host = "127.0.0.1";
        private Dictionary<string, List<string>> _errors;
        private int _port = 21;

        public string UserName
        {
            set
            {
                if (this._userName != value)
                {
                    this._userName = value;
                    this.OnPropertyChanged("UserName");
                }
            }
            get
            {
                return this._userName;
            }
        }

        public string Password
        {
            set
            {
                if (this._password != value)
                {
                    this._password = value;
                    this.OnPropertyChanged("Password");
                }
            }
            get
            {
                return this._password;
            }
        }

        public string Host
        {
            set
            {
                value = value.Trim();
                if (this._host != value)
                {
                    this.HostVaildation(value);
                    this._host = value;
                    this.OnPropertyChanged("Host");
                }
            }
            get
            {
                return this._host;
            }
        }

        public int Port
        {
            set
            {
                if (this._port != value)
                {
                    this.PortRangeVaildation(value);
                    this._port = value;
                    this.OnPropertyChanged("Port");
                }
            }
            get
            {
                return this._port;
            }
        }

        public FtpInfo()
        {
            this._errors = new Dictionary<string, List<string>>();
        }

        private void HostVaildation(string host)
        {
            if (string.IsNullOrEmpty(host))
            {
                this.AddError("Host", HOST_ERROR);
            }
            else
            {
                this.RemoveError("Host", HOST_ERROR);
            }
        }

        private void PortRangeVaildation(int port)
        {
            if (port >= 1 && port <= 65535)
            {
                this.RemoveError("Port", PORT_ERROR);
            }
            else
            {
                this.AddError("Port", PORT_ERROR);
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void AddError(string propertyName, string error)
        {
            if (!this._errors.ContainsKey(propertyName))
            {
                this._errors.Add(propertyName, new List<string>());
            }
            if (!this._errors[propertyName].Contains(error))
            {
                this._errors[propertyName].Add(error);
                this.OnErrorsChanged(propertyName);
            }
        }

        private void RemoveError(string propertyName, string error)
        {
            if (this._errors.ContainsKey(propertyName) &&
                this._errors[propertyName].Contains(error))
            {
                this._errors[propertyName].Remove(error);
                this.OnErrorsChanged(propertyName);
            }
        }

        private void OnErrorsChanged(string propertyName)
        {
            if (this.ErrorsChanged != null)
            {
                this.ErrorsChanged(this,
                    new DataErrorsChangedEventArgs(propertyName));
            }
        }

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            if (this._errors.ContainsKey(propertyName))
            {
                return this._errors[propertyName];
            }
            return null;
        }

        public bool HasErrors
        {
            get { return this._errors.Count(e => e.Value.Count > 0) > 0; }
        }
    }
}
