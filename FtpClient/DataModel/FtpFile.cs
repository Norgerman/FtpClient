using System;
using System.Globalization;

namespace FtpClient.DataModel
{
    delegate FtpFile Parser(string ftpFileString);

    public class FtpFile
    {
        public static readonly FtpFile PreDir;
        private static Parser CurrentParser;
        public string Name { set; get; }
        public string Size { set; get; }
        public int Type { set; get; }
        public DateTime Modified { set; get; }
        public long ByteSize { set; get; }

        static FtpFile()
        {
            PreDir = new FtpFile { Name = "..", Size = "", Type = 0, Modified = DateTime.MinValue };
        }

        public static FtpFile Parse(string ftpFileString)
        {
            if (CurrentParser == null)
            {
                try
                {
                    var tmpf = MS_DOSParserWithLongYear(ftpFileString);
                    CurrentParser = MS_DOSParserWithLongYear;
                    return tmpf;
                }
                catch (Exception)
                {
                    try
                    {
                        var tmpf = MS_DOSParserWithShortYear(ftpFileString);
                        CurrentParser = MS_DOSParserWithShortYear;
                        return tmpf;
                    }
                    catch (Exception)
                    {

                        CurrentParser = UnixParser;
                        return UnixParser(ftpFileString);
                    }
                }
            }
            else
            {
                return CurrentParser(ftpFileString);
            }
        }

        private static FtpFile MS_DOSParserWithLongYear(string ftpFileString)
        {
            DateTime date;
            FtpFile tmpfile = null;
            string sizestring;
            string datestring = ftpFileString.Substring(0, 19).Trim();
            if (DateTime.TryParseExact(datestring, "MM-dd-yyyy  hh:mmtt",
                CultureInfo.CurrentCulture, DateTimeStyles.None, out date))
            {
                tmpfile = new FtpFile();
                sizestring = ftpFileString.Substring(20, 20).Trim();
                long size;
                if (long.TryParse(sizestring, out size))
                {
                    tmpfile.Type = 2;
                    tmpfile.Size = sizestring + "Bytes";
                    tmpfile.ByteSize = size;
                }
                else
                {
                    tmpfile.Type = 1;
                    tmpfile.Size = "";
                }
                tmpfile.Modified = date;
                tmpfile.Name = ftpFileString.Substring(41);
            }
            else
            {
                throw new Exception();
            }
            return tmpfile;
        }

        private static FtpFile MS_DOSParserWithShortYear(string ftpFileString)
        {
            DateTime date;
            FtpFile tmpfile = null;
            string sizestring;
            string datestring = ftpFileString.Substring(0, 17).Trim();
            if (DateTime.TryParseExact(datestring, "MM-dd-yy  hh:mmtt",
                CultureInfo.CurrentCulture, DateTimeStyles.None, out date))
            {
                tmpfile = new FtpFile();
                sizestring = ftpFileString.Substring(18, 20).Trim();
                long size;
                if (long.TryParse(sizestring, out size))
                {
                    tmpfile.Size = sizestring + "Bytes";
                    tmpfile.ByteSize = size;
                }
                else
                {
                    tmpfile.Type = 1;
                    tmpfile.Size = "";
                }
                tmpfile.Modified = date;
                tmpfile.Name = ftpFileString.Substring(39);
            }
            else
            {
                throw new Exception();
            }
            return tmpfile;
        }

        private static FtpFile UnixParser(string ftpFileString)
        {
            FtpFile tmpfile = new FtpFile();
            string sizestring = ftpFileString.Substring(30, 15).Trim();
            string datestring = ftpFileString.Substring(46, 12).Trim();
            long size = long.Parse(sizestring);
            DateTime date;
            if (!DateTime.TryParseExact(datestring, "MMM d yyyy",
                CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out date))
            {
                date = DateTime.ParseExact(datestring, "MMM dd H:mm",
                    CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces);
            }
            tmpfile.Modified = date;
            tmpfile.Name = ftpFileString.Substring(59);
            if (size != 0)
            {
                tmpfile.Size = sizestring + "Bytes";
                tmpfile.ByteSize = size;
            }
            else
            {
                tmpfile.Size = "";
                tmpfile.Type = 1;
            }
            return tmpfile;
        }

        public static void InitlizePaser()
        {
            CurrentParser = null;
        }
    }
}
