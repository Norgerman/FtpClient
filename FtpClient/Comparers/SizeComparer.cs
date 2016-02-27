using FtpClient.DataModel;
using System.ComponentModel;

namespace FtpClient.Comparers
{
    class SizeComparer : ComparerBase
    {
        public SizeComparer(ListSortDirection listSortDirection)
            : base(listSortDirection)
        {
        }

        protected override int DescSorter(object x, object y)
        {
            int result = 0;
            FtpFile filex = x as FtpFile;
            FtpFile filey = y as FtpFile;
            string tmpx = filex.Size.Trim();
            string tmpy = filey.Size.Trim();
            result = filex.Type - filey.Type;
            if (result == 0)
            {
                if (string.IsNullOrEmpty(tmpx) || string.IsNullOrEmpty(tmpy))
                {
                    result = tmpy.CompareTo(tmpx);
                }
                else
                {
                    result = filey.ByteSize.CompareTo(filex.ByteSize);
                }
            }
            tmpx = null;
            tmpy = null;
            return result;
        }

        protected override int AscSorter(object x, object y)
        {
            int result = 0;
            FtpFile filex = x as FtpFile;
            FtpFile filey = y as FtpFile;
            string tmpx = filex.Size.Trim();
            string tmpy = filey.Size.Trim();
            result = filex.Type - filey.Type;
            if (result == 0)
            {
                if (string.IsNullOrEmpty(tmpx) || string.IsNullOrEmpty(tmpy))
                {
                    result = tmpx.CompareTo(tmpy);
                }
                else
                {
                    result = filex.ByteSize.CompareTo(filey.ByteSize);
                }
            }
            tmpx = null;
            tmpy = null;
            return result;
        }
    }
}
