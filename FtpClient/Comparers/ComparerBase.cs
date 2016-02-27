using System.Collections;
using System.ComponentModel;

namespace FtpClient.Comparers
{
    delegate int Sorter(object x, object y);

    abstract class ComparerBase : IComparer
    {
        protected ListSortDirection _listSortDirection;
        private Sorter _curSorter;

        public ComparerBase(ListSortDirection listSortDirection)
        {
            this._listSortDirection = listSortDirection;
            if (this._listSortDirection == ListSortDirection.Descending)
            {
                this._curSorter = DescSorter;
            }
            else
            {
                this._curSorter = AscSorter;
            }
        }

        public int Compare(object x, object y)
        {
            return this._curSorter(x, y);
        }

        protected abstract int DescSorter(object x, object y);
        protected abstract int AscSorter(object x, object y);
    }
}
