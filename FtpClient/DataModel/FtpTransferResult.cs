using System;
using System.ComponentModel;

namespace FtpClient.DataModel
{
    public enum FtpResultType
    {
        Error,
        Connecting,
        Connected,
        Status,
        Downloading,
        Uploading,
        Downloaded,
        Uploaded,
        ListingDirectory,
        CompleteListingDirectory
    }

    public class FtpTransferResult : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private double _process;

        public FtpResultType ResultType { set; get; }
        public string Info { set; get; }
        public string Target { set; get; }
        public DateTime Time { set; get; }
        public long TotalLength { set; get; }
        public long Position { set; get; }
        public double Process
        {
            set
            {
                if (this._process != value)
                {
                    this._process = value;
                    this.OnPropertyChanged("Process");
                }
            }
            get
            {
                return this._process;
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
