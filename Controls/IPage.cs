using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagedItemsControl.Controls
{
    public interface IPage : INotifyCollectionChanged
    {
        int Index { get; }

        object FirstOrDefault();
        object First();
    }

    public interface IPage<T> : ICollection<T>, IPage
    {
        new T FirstOrDefault();
        new T First();
    }

    public class ObservablePage<T> : ObservableCollection<T>, IPage<T>
    {
        public int Index { get; private set; }

        public ObservablePage(int index)
        {
            this.Index = index;
        }

        public ObservablePage(int index, IEnumerable<T> collection)
            : base(collection)
        {
            this.Index = index;
        }

        public T FirstOrDefault()
        {
            return this.FirstOrDefault(x => true);
        }

        public T First()
        {
            return this.First(x => true);
        }

        object IPage.FirstOrDefault()
        {
            return FirstOrDefault();
        }

        object IPage.First()
        {
            return First();
        }
    }
}
