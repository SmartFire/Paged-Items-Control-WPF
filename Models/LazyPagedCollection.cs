using PagedItemsControl.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagedItemsControl.Models
{
    public class LazyPagedCollection<T> : PropertyChangedBase, IPagedCollection<T>
    {
        public event EventHandler OnPagesLoaded;
        public event EventHandler<IPage> OnPageCreated;
        public event EventHandler<IPage> OnNewPageNavigated;

        private List<int> _ItemsPerPageOptions = new List<int>()
            {
                10,
                20,
                50,
                100
            };

        public List<int> ItemsPerPageOptions
        {
            get { return _ItemsPerPageOptions; }
            set
            {
                _ItemsPerPageOptions = value;
                NotifyOfPropertyChange();
            }
        }

        private int _ItemsPerPage;

        public int ItemsPerPage
        {
            get { return _ItemsPerPage; }
            set
            {
                _ItemsPerPage = value;
                NotifyOfPropertyChange();

                RecalculateTotalPages();
            }
        }

        private IEnumerable<T> _Collection;

        public IEnumerable<T> Collection
        {
            get { return _Collection; }
            set
            {
                _Collection = value;
                RecalculateTotalPages();
            }
        }


        public LazyPagedCollection(int records = 0, int itemsPerPage = 50, List<int> itemsPerPageOptions = null)
        {
            Collection = new List<T>();
            ItemsPerPage = itemsPerPage;

            if (itemsPerPageOptions != null)
                ItemsPerPageOptions = itemsPerPageOptions;

            TotalCount = records;
        }

        public LazyPagedCollection(IEnumerable<T> collection, int records = 0, int itemsPerPage = 50, List<int> itemsPerPageOptions = null)
        {
            ItemsPerPage = itemsPerPage;
            Collection = collection;

            if (itemsPerPageOptions != null)
                ItemsPerPageOptions = itemsPerPageOptions;

            if (TotalCount == 0)
            {
                TotalCount = records;
                RecalculateTotalPages();
            }
        }

        private void RecalculateTotalPages()
        {
            if (TotalCount == 0)
                if (Collection != null)
                    TotalCount = Collection.Count();

            if (TotalCount > 0)
                this.PageCount = (TotalCount / ItemsPerPage) + 1;
            else
                this.PageCount = 1;
        }

        private void LoadCurrentPage()
        {
            IPage<T> page = GetPage(CurrentPageIndex);

            if (page != null)
                this.CurrentPage = page;
        }

        public void NextPage()
        {
            if (CurrentPageIndex >= PageCount)
                return;

            CurrentPageIndex++;
        }

        public void PreviousPage()
        {
            if (CurrentPageIndex - 1 == 0)
                return;

            CurrentPageIndex--;
        }

        public void LastPage()
        {
            CurrentPageIndex = PageCount;
        }

        public void FirstPage()
        {
            CurrentPageIndex = 1;
        }

        public void GoToPage(int index)
        {
            CurrentPageIndex = index;
        }

        public void GoToPage(IPage<T> page)
        {
            this.CurrentPage = page;
        }

        private IPage<T> _CurrentPage;

        public IPage<T> CurrentPage
        {
            get { return _CurrentPage; }
            protected set
            {
                _CurrentPage = value;
                NotifyOfPropertyChange();

                if (value != null && OnNewPageNavigated != null)
                    OnNewPageNavigated(this, value);
            }
        }

        private int _CurrentPageIndex = 1;

        public int CurrentPageIndex
        {
            get { return _CurrentPageIndex; }
            private set
            {
                _CurrentPageIndex = value;
                NotifyOfPropertyChange();

                LoadCurrentPage();
            }
        }

        public int PageCount { get; private set; }

        private ObservableCollection<IPage<T>> _Pages;

        public ObservableCollection<IPage<T>> Pages
        {
            get { return _Pages; }
            set { _Pages = value; }
        }

        public IPage<T> GetPage(int index)
        {
            if (this.Pages == null)
                LoadPages();

            IPage<T> page = this.Pages.FirstOrDefault(x => x.Index == index);

            //If there is something on the page
            //then don't bother executing the query again.
            if (page == null || page.Count > 0)
                return page;

            //If there is nothing on the page, then run the query for that page.
            IEnumerable<T> result = Collection
                                        .Skip(ItemsPerPage * (index - 1))
                                        .Take(ItemsPerPage);

            page = new ObservablePage<T>(index, result);

            //Add the cached page to the page collection.
            this.Pages.RemoveAt(index - 1);
            this.Pages.Insert(index - 1, page);

            return page;
        }

        public IEnumerable<IPage<T>> GetPages(int start, int end)
        {
            if (start < 1)
                start = 1;

            if (end > PageCount + 1)
                end = PageCount + 1;

            List<IPage<T>> pages = new List<IPage<T>>();

            for (int i = start; i < end; i++)
            {
                IPage<T> next = this.Pages.FirstOrDefault(x => x.Index == i);

                if (next != null)
                    pages.Add(next);
            }

            return pages;
        }

        public IPage GetCurrentPage()
        {
            return CurrentPage;
        }

        public void LoadPages()
        {
            this.Pages = new ObservableCollection<IPage<T>>();

            //Create pages with nothing in them.
            for (int i = 0; i < PageCount; i++)
            {
                IPage<T> page = new ObservablePage<T>(i + 1);

                this.Pages.Add(page);

                if (this.OnPageCreated != null)
                    OnPageCreated(this, page);
            }

            this.CurrentPage = GetPage(1);

            if (OnPagesLoaded != null)
                OnPagesLoaded(this, EventArgs.Empty);
        }

        IPage IPagedCollection.GetPage(int index)
        {
            return GetPage(index);
        }

        IEnumerable<IPage> IPagedCollection.GetPages(int start, int end)
        {
            return GetPages(start, end);
        }

        public int TotalCount { get; private set; }
    }
}
