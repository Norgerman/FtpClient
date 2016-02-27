using FtpClient.DataModel;
using System.ComponentModel;

namespace FtpClient.Comparers
{
    class ModifiedComparer : ComparerBase
    {
        public ModifiedComparer(ListSortDirection listSortDirection)
            : base(listSortDirection)
        {
        }

        protected override int DescSorter(object x, object y)
        {
            int result = 0;
            FtpFile filex = x as FtpFile;
            FtpFile filey = y as FtpFile;
            result = filex.Type - filey.Type;
            if (result == 0)
            {
                result = filey.Modified.CompareTo(filex.Modified);
            }
            return result;
        }

        protected override int AscSorter(object x, object y)
        {
            int result = 0;
            FtpFile filex = x as FtpFile;
            FtpFile filey = y as FtpFile;
            result = filex.Type - filey.Type;
            if (result == 0)
            {
                result = filex.Modified.CompareTo(filey.Modified);
            }
            return result;
        }
    }
}
