using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using FtpClient.DataModel;

namespace FtpClient
{
    public class FtpServiceProvider
    {
        private const int BUFFER_LENGTH = 1048576;

        public FtpInfo Ftp { private set; get; }

        public FtpServiceProvider()
        {
            this.Ftp = new FtpInfo();
        }

        public async Task<IEnumerable<FtpFile>> GetRemoteFileListAsync(string subDir = "/")
        {
            List<FtpFile> files = new List<FtpFile>();
            FtpWebRequest request = FtpWebRequest.Create("ftp://" +
                this.Ftp.Host + ":" + this.Ftp.Port + subDir) as FtpWebRequest;
            request.Credentials = new NetworkCredential(this.Ftp.UserName, this.Ftp.Password);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            request.Timeout = 10000;
            request.ReadWriteTimeout = 10000;
            using (var response = await request.GetResponseAsync() as FtpWebResponse)
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                while (reader.Peek() > -1)
                {
                    files.Add(FtpFile.Parse(await reader.ReadLineAsync()));
                }
            }
            return files.OrderBy(item => item.Type)
                .CreateOrderedEnumerable(item => item.Name, Comparer<string>.Default, false);
        }

        public async Task<IEnumerable<FtpFile>> GetLocalFileListAsync(string dir)
        {
            List<FtpFile> files = new List<FtpFile>();
            DirectoryInfo directory = new DirectoryInfo(dir);
            FileSystemInfo[] filesysteminfos = await Task.Run<FileSystemInfo[]>(() => directory.GetFileSystemInfos());
            Parallel.ForEach<FileSystemInfo, List<FtpFile>>(filesysteminfos, () => new List<FtpFile>(),
                (FileSystemInfo fs, ParallelLoopState loopstate, List<FtpFile> localfiles) =>
                {
                    FtpFile file = new FtpFile();
                    if ((fs.Attributes & FileAttributes.System) == FileAttributes.System)
                    {
                        return localfiles;
                    }
                    if ((fs.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        file.Size = "";
                        file.Type = 1;
                    }
                    else
                    {
                        file.ByteSize = ((FileInfo)fs).Length;
                        file.Size = file.ByteSize + "Bytes";
                        file.Type = 2;
                    }
                    file.Name = fs.Name;
                    file.Modified = fs.LastWriteTime;
                    localfiles.Add(file);
                    return localfiles;
                },
                localfiles =>
                {
                    Monitor.Enter(files);
                    files.AddRange(localfiles);
                    Monitor.Exit(files);
                });
            return files.OrderBy(item => item.Type)
                .CreateOrderedEnumerable(item => item.Name, Comparer<string>.Default, false);
        }

        public async Task DownLoadFileAsync(FtpTransferResult ftpResult,
            CancellationTokenSource cancelltionTokenSource)
        {
            byte[] buffer = new byte[BUFFER_LENGTH];
            FtpWebRequest request = FtpWebRequest.Create("ftp://" +
                this.Ftp.Host + ":" + this.Ftp.Port + ftpResult.Info) as FtpWebRequest;
            request.Credentials = new NetworkCredential(this.Ftp.UserName, this.Ftp.Password);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Timeout = 10000;
            request.ReadWriteTimeout = 10000;
            using (var response = await request.GetResponseAsync() as FtpWebResponse)
            using (var stream = response.GetResponseStream())
            using (var filestream = new FileStream(ftpResult.Target, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                int readed = 0;
                while ((readed = await stream.ReadAsync(buffer, 0, BUFFER_LENGTH)) > 0)
                {
                    if (cancelltionTokenSource.IsCancellationRequested) break;
                    await filestream.WriteAsync(buffer, 0, readed);
                    ftpResult.Position += readed;
                    ftpResult.Process = (double)ftpResult.Position / ftpResult.TotalLength * 100;
                }
                await filestream.FlushAsync();
            }
        }

        public async Task ResumeDownLoadFileAsync(FtpTransferResult ftpResult,
            CancellationTokenSource cancelltionTokenSource)
        {
            byte[] buffer = new byte[BUFFER_LENGTH];
            FtpWebRequest request = FtpWebRequest.Create("ftp://" +
                this.Ftp.Host + ":" + this.Ftp.Port + ftpResult.Info) as FtpWebRequest;
            request.Credentials = new NetworkCredential(this.Ftp.UserName, this.Ftp.Password);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Timeout = 10000;
            request.ReadWriteTimeout = 10000;
            request.ContentOffset = ftpResult.Position;
            using (var response = await request.GetResponseAsync() as FtpWebResponse)
            using (var stream = response.GetResponseStream())
            using (var filestream = new FileStream(ftpResult.Target, FileMode.Open, FileAccess.Write, FileShare.None))
            {
                int readed = 0;
                filestream.Seek(ftpResult.Position, SeekOrigin.Begin);
                while ((readed = await stream.ReadAsync(buffer, 0, BUFFER_LENGTH)) > 0)
                {
                    if (cancelltionTokenSource.IsCancellationRequested) break;
                    await filestream.WriteAsync(buffer, 0, readed);
                    ftpResult.Position += readed;
                    ftpResult.Process = (double)ftpResult.Position / ftpResult.TotalLength * 100;
                }
                await filestream.FlushAsync();
            }
        }

        public async Task UpLoadFileAsync(FtpTransferResult ftpResult)
        {
            byte[] buffer = new byte[BUFFER_LENGTH];
            FtpWebRequest request = FtpWebRequest.Create("ftp://" +
                this.Ftp.Host + ":" + this.Ftp.Port + ftpResult.Target) as FtpWebRequest;
            request.Credentials = new NetworkCredential(this.Ftp.UserName, this.Ftp.Password);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Timeout = 10000;
            request.ReadWriteTimeout = 10000;
            using (var stream = await request.GetRequestStreamAsync())
            using (var filestream = new FileStream(ftpResult.Info, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                int readed = 0;
                while ((readed = await filestream.ReadAsync(buffer, 0, BUFFER_LENGTH)) > 0)
                {
                    await stream.WriteAsync(buffer, 0, readed);
                    ftpResult.Position += readed;
                    ftpResult.Process = (double)ftpResult.Position / ftpResult.TotalLength * 100;
                }
                await stream.FlushAsync();
            }
        }
    }
}
