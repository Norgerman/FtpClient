using FtpClient.Comparers;
using FtpClient.DataModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace FtpClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private ObservableCollection<FtpFile> _remoteValue;
        private ObservableCollection<FtpFile> _localValue;
        private ObservableCollection<FtpTransferResult> _ftpResults;
        private List<FtpTransferResult> _queuedMissions;
        private ScrollViewer _scrollViewer;
        private StringBuilder _remotePath;
        private StringBuilder _localPath;
        private FtpFile[] _drives;
        private ObservableCollection<FtpTransferResult> _unCompletedMissions;
        private bool _connected;
        private CancellationTokenSource _cancelTokanSource;
        public FtpServiceProvider FtpService { private set; get; }
        public ObservableCollection<FtpFile> RemoteValue
        {
            get
            {
                return this._remoteValue;
            }
        }
        public ObservableCollection<FtpFile> LocalValue
        {
            get
            {
                return this._localValue;
            }
        }
        public ObservableCollection<FtpTransferResult> FtpResults
        {
            get
            {
                return this._ftpResults;
            }
        }
        public ObservableCollection<FtpTransferResult> UnCompletedMissions
        {
            get
            {
                return this._unCompletedMissions;
            }
        }

        public MainWindow()
        {
            this._remoteValue = new ObservableCollection<FtpFile>();
            this._localValue = new ObservableCollection<FtpFile>();
            this._ftpResults = new ObservableCollection<FtpTransferResult>();
            this._queuedMissions = new List<FtpTransferResult>();
            this.FtpService = new FtpServiceProvider();
            this._remotePath = new StringBuilder("/");
            this._localPath = new StringBuilder();
            this._unCompletedMissions = new ObservableCollection<FtpTransferResult>();
            this._cancelTokanSource = new CancellationTokenSource();
            this._connected = false;
            InitializeComponent();
            this._ftpResults.CollectionChanged += _ftpResults_CollectionChanged;
            this.CommandBindings.Add(new CommandBinding(CustomCommands.About,
                (sender, e) =>
                {
                    MessageBox.Show("It is not completed");
                },
                (sender, e) =>
                {
                    e.CanExecute = true;
                }));
        }

        private void _ftpResults_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this._scrollViewer.ScrollToEnd();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await this.LoadDrives();
            this._scrollViewer = this.GetDescendantByType(this.lv_results, typeof(ScrollViewer)) as ScrollViewer;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this._cancelTokanSource.Cancel();
            Monitor.Enter(this._unCompletedMissions);
            using (var filestream = new FileStream(".\\logs.json", FileMode.Create,
                FileAccess.Write, FileShare.None))
            using (StreamWriter sr = new StreamWriter(filestream, Encoding.UTF8))
            {
                JavaScriptSerializer json = new JavaScriptSerializer();
                sr.Write(json.Serialize(this._queuedMissions));
            }
            Monitor.Exit(this._unCompletedMissions);
            this._cancelTokanSource.Dispose();
        }

        private async void btn_Connect_Click(object sender, RoutedEventArgs e)
        {
            if (this._remoteValue.Count != 0)
            {
                this._remoteValue.Clear();
            }
            this._remotePath.Clear();
            this._ftpResults.Add(new FtpTransferResult
            {
                ResultType = FtpResultType.Connecting,
                Info = "Connecting Host: " + this.FtpService.Ftp.Host,
                Process = 100,
                Time = DateTime.Now
            });
            this._remotePath.Append("/");
            try
            {
                FtpFile.InitlizePaser();
                await this.ListRemoteFiles();
                this._ftpResults.Add(new FtpTransferResult
                {
                    ResultType = FtpResultType.Connected,
                    Info = "Host: " + this.FtpService.Ftp.Host + " Connected",
                    Process = 100,
                    Time = DateTime.Now
                });
                this._connected = true;
            }
            catch (Exception ex)
            {
                this._ftpResults.Add(new FtpTransferResult
                {
                    ResultType = FtpResultType.Error,
                    Info = ex.Message,
                    Process = 100,
                    Time = DateTime.Now
                });
            }
        }

        private async void btn_Resume_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in this._unCompletedMissions)
            {
                try
                {
                    await this.ResumeDownLoadFile(item);
                    this._unCompletedMissions.Remove(item);
                }
                catch (Exception ex)
                {
                    this._queuedMissions.Remove(item);
                    this._ftpResults.Add(new FtpTransferResult
                    {
                        ResultType = FtpResultType.Error,
                        Info = ex.Message,
                        Process = 100,
                        Time = DateTime.Now
                    });
                }
            }
        }

        private async void dg_Remote_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = this.dg_Remote.SelectedItem as FtpFile;
            if (selected != null)
            {
                try
                {
                    if (selected.Type == 1)
                    {
                        this._remoteValue.Clear();
                        this._remotePath.Append(selected.Name + "/");
                        this._ftpResults.Add(new FtpTransferResult
                        {
                            ResultType = FtpResultType.ListingDirectory,
                            Info = "Path: " + this._remotePath,
                            Process = 100,
                            Time = DateTime.Now
                        });
                        await this.ListRemoteFiles();
                        this._ftpResults.Add(new FtpTransferResult
                        {
                            ResultType = FtpResultType.CompleteListingDirectory,
                            Info = "Path: " + this._remotePath,
                            Process = 100,
                            Time = DateTime.Now
                        });
                    }
                    else if (selected.Type == 2)
                    {
                        if (this._localPath.Length != 0)
                        {
                            await this.DownLoadFile(selected);
                        }
                        else
                        {
                            this._ftpResults.Add(new FtpTransferResult
                            {
                                ResultType = FtpResultType.Error,
                                Info = "Cannot Write Here",
                                Process = 100,
                                Time = DateTime.Now
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    this._ftpResults.Add(new FtpTransferResult
                    {
                        ResultType = FtpResultType.Error,
                        Info = ex.Message,
                        Process = 100,
                        Time = DateTime.Now
                    });
                }
            }
        }

        private void dg_Remote_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsInValueArea(e.GetPosition(this.dg_Remote).Y, this.dg_Remote.Items.Count))
            {
                this.dg_Remote.UnselectAll();
                return;
            }
        }

        private void dg_Sorting(object sender, DataGridSortingEventArgs e)
        {
            DataGrid dg = sender as DataGrid;
            var tmpsortdirection = e.Column.SortDirection;
            e.Handled = true;
            this.CustomSort(dg,
                e.Column,
                tmpsortdirection != ListSortDirection.Ascending ?
                ListSortDirection.Ascending :
                ListSortDirection.Descending);
            dg.UnselectAll();
        }

        private async void dg_Local_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = this.dg_Local.SelectedItem as FtpFile;
            try
            {
                if (selected != null)
                {
                    if (selected.Type == 1)
                    {
                        this._localValue.Clear();
                        this._localPath.Append(selected.Name + "\\");
                        await this.ListLocalFiles();
                        this.dg_Local.UnselectAll();
                    }
                    else if (selected.Type == 2)
                    {
                        if (this._connected)
                        {
                            await this.UpLoadFile(selected);
                        }
                        else
                        {
                            this._ftpResults.Add(new FtpTransferResult
                            {
                                ResultType = FtpResultType.Error,
                                Info = "Unconnected",
                                Process = 100,
                                Time = DateTime.Now
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this._ftpResults.Add(new FtpTransferResult
                {
                    ResultType = FtpResultType.Error,
                    Info = ex.Message,
                    Process = 100,
                    Time = DateTime.Now
                });
            }
        }

        private void dg_Local_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsInValueArea(e.GetPosition(this.dg_Local).Y, this.dg_Local.Items.Count))
            {
                this.dg_Local.UnselectAll();
                return;
            }
        }

        private void cb_Any_Checked(object sender, RoutedEventArgs e)
        {
            this.FtpService.Ftp.UserName = "ftp";
            this.FtpService.Ftp.Password = "ftp";
        }

        private void CustomSort(DataGrid dataGrid, DataGridColumn column, ListSortDirection listSortDirection)
        {
            IComparer comparer;
            if (column.Header.Equals("Name"))
            {
                comparer = new NameComparer(listSortDirection);
            }
            else if (column.Header.Equals("Size"))
            {
                comparer = new SizeComparer(listSortDirection);
            }
            else
            {
                comparer = new ModifiedComparer(listSortDirection);
            }
            column.SortDirection = listSortDirection;
            ListCollectionView collectionview = CollectionViewSource
                .GetDefaultView(dataGrid.ItemsSource) as ListCollectionView;
            if (collectionview != null)
            {
                collectionview.CustomSort = comparer;
            }
            else
            {
                return;
            }
        }

        private bool IsInValueArea(double y, int count)
        {
            return y <= (25 + 20 * count);
        }

        private async Task ListRemoteFiles()
        {
            var files = await FtpService.GetRemoteFileListAsync(this._remotePath.ToString());
            foreach (var item in files)
            {
                this._remoteValue.Add(item);
            }
        }

        private async Task ListLocalFiles()
        {
            var files = await this.FtpService.GetLocalFileListAsync(this._localPath.ToString());
            foreach (var item in files)
            {
                this._localValue.Add(item);
            }
        }

        private async Task LoadDrives()
        {
            DriveInfo[] drives = await Task.Run<DriveInfo[]>(() => DriveInfo.GetDrives());
            this._drives = new FtpFile[drives.Length];
            for (int i = 0; i < drives.Length; i++)
            {
                var tmp = new FtpFile();
                tmp.Name = drives[i].RootDirectory.Name;
                tmp.Name = tmp.Name.Substring(0, tmp.Name.Length - 1);
                tmp.Type = 1;
                tmp.ByteSize = drives[i].TotalSize;
                tmp.Size = (drives[i].AvailableFreeSpace >> 9) + "GB / " +
                    (drives[i].TotalSize >> 9) + "GB";
                this._drives[i] = tmp;
                this._localValue.Add(tmp);
            }
        }

        private async Task DownLoadFile(FtpFile selected)
        {
            FtpTransferResult result = new FtpTransferResult();
            result.ResultType = FtpResultType.Downloading;
            result.Info = this._remotePath + selected.Name;
            result.Target = this._localPath + selected.Name;
            result.TotalLength = selected.ByteSize;
            result.Time = DateTime.Now;
            result.Position = 0;
            result.Process = (double)result.Position / result.TotalLength * 100;
            this._ftpResults.Add(result);
            this._queuedMissions.Add(result);
            try
            {
                await this.FtpService.DownLoadFileAsync(result, this._cancelTokanSource);
                this._ftpResults.Add(new FtpTransferResult
                {
                    ResultType = FtpResultType.Downloaded,
                    Info = result.Info,
                    Target = result.Target,
                    Process = 100,
                    Time = DateTime.Now
                });
                this._localValue.Clear();
                this._queuedMissions.Remove(result);
                await this.ListLocalFiles();
            }
            catch (Exception)
            {
                this._unCompletedMissions.Add(result);
                this._queuedMissions.Remove(result);
                File.Delete(result.Target);
                throw;
            }
        }

        private async Task ResumeDownLoadFile(FtpTransferResult resume)
        {
            this._ftpResults.Add(resume);
            this._queuedMissions.Add(resume);
            await this.FtpService.ResumeDownLoadFileAsync(resume,this._cancelTokanSource);
            this._ftpResults.Add(new FtpTransferResult
            {
                ResultType = FtpResultType.Downloaded,
                Info = resume.Info,
                Target = resume.Target,
                Process = 100,
                Time = DateTime.Now
            });
            this._queuedMissions.Remove(resume);
        }

        private async Task UpLoadFile(FtpFile selected)
        {
            FtpTransferResult result = new FtpTransferResult();
            result.ResultType = FtpResultType.Uploading;
            result.Info = this._localPath + selected.Name;
            result.Target = this._remotePath + selected.Name;
            result.TotalLength = selected.ByteSize;
            result.Time = DateTime.Now;
            result.Process = 0;
            result.Position = 0;
            this._ftpResults.Add(result);
            this._queuedMissions.Add(result);
            try
            {
                await this.FtpService.UpLoadFileAsync(result);
                this._ftpResults.Add(new FtpTransferResult
                {
                    ResultType = FtpResultType.Uploaded,
                    Info = result.Info,
                    Target = result.Target,
                    Process = 100,
                    Time = DateTime.Now
                });
                this._remoteValue.Clear();
                this._queuedMissions.Remove(result);
                await this.ListRemoteFiles();
            }
            catch (Exception)
            {
                this._queuedMissions.Remove(result);
                throw;
            }
        }

        private Visual GetDescendantByType(Visual element, Type type)
        {
            if (element == null) return null;
            if (element.GetType() == type) return element;
            Visual foundElement = null;
            if (element is FrameworkElement)
            {
                (element as FrameworkElement).ApplyTemplate();
            }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType(visual, type);
                if (foundElement != null)
                    break;
            }
            return foundElement;
        }
    }
}
