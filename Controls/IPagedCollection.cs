using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagedItemsControl.Controls
{
    public interface IPagedCollection : INotifyPropertyChanged
    {
        event EventHandler OnPagesLoaded;
        event EventHandler<IPage> OnPageCreated;
        event EventHandler<IPage> OnNewPageNavigated;

        IPage GetCurrentPage();

        int ItemsPerPage { get; set; }
        List<int> ItemsPerPageOptions { get; set; }

        void LoadPages();

        void NextPage();
        void PreviousPage();
        void LastPage();
        void FirstPage();
        void GoToPage(int index);

        IPage GetPage(int index);
        IEnumerable<IPage> GetPages(int start, int end);

        int PageCount { get; }
        int TotalCount { get; }
        int CurrentPageIndex { get; }
    }

    public interface IPagedCollection<T> : IPagedCollection
    {
        IPage<T> CurrentPage { get; }
        ObservableCollection<IPage<T>> Pages { get; }

        void GoToPage(IPage<T> page);

        new IPage<T> GetPage(int index);
        new IEnumerable<IPage<T>> GetPages(int start, int end);
    }
}
