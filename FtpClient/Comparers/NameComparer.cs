using FtpClient.DataModel;
using System.ComponentModel;

namespace FtpClient.Comparers
{
    class NameComparer : ComparerBase
    {
        public NameComparer(ListSortDirection listSortDirection)
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
                result = filey.Name.CompareTo(filex.Name);
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
                result = filex.Name.CompareTo(filey.Name);
            }
            return result;
        }
    }
}
