using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PagedItemsControl.Controls
{
    public class AnagramPagedItemsControl : ItemsControl
    {
        public IPagedCollection PagedCollection
        {
            get { return (IPagedCollection)GetValue(PagedCollectionProperty); }
            set { SetValue(PagedCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PagedCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PagedCollectionProperty =
            DependencyProperty.Register("PagedCollection", typeof(IPagedCollection), typeof(AnagramPagedItemsControl), new PropertyMetadata(null, new PropertyChangedCallback(PagedCollectionChanged)));

        public Brush SelectedPageColour
        {
            get { return (Brush)GetValue(SelectedPageColourProperty); }
            set { SetValue(SelectedPageColourProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedPageColour.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedPageColourProperty =
            DependencyProperty.Register("SelectedPageColour", typeof(Brush), typeof(AnagramPagedItemsControl), new PropertyMetadata(Brushes.Red));

        #region collection changed

        private static void PagedCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnagramPagedItemsControl itemsControl = d as AnagramPagedItemsControl;

            IPagedCollection oldCol = e.OldValue as IPagedCollection;
            IPagedCollection newCol = e.NewValue as IPagedCollection;

            itemsControl.SubscribeToCollectionChanged(oldCol, newCol);
        }

        private void SubscribeToCollectionChanged(IPagedCollection oldCol, IPagedCollection newCol)
        {
            //Unsubscribe to events from old collection.
            if (oldCol != null)
            {
                oldCol.OnPageCreated -= PagedCollection_OnPageCreated;
                newCol.OnNewPageNavigated -= newCol_OnNewPageNavigated;
            }

            //Subscribe to events for new collection.
            if (newCol != null)
            {
                newCol.OnPageCreated += PagedCollection_OnPageCreated;
                newCol.OnNewPageNavigated += newCol_OnNewPageNavigated;
            }
        }

        #endregion

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(AnagramPagedItemsControl), new PropertyMetadata(null));

        public ObservableCollection<Button> DisplayedButtons
        {
            get { return (ObservableCollection<Button>)GetValue(DisplayedButtonsProperty); }
            set { SetValue(DisplayedButtonsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayedButtons.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayedButtonsProperty =
            DependencyProperty.Register("DisplayedButtons", typeof(ObservableCollection<Button>), typeof(AnagramPagedItemsControl), new PropertyMetadata(null));

        public IEnumerable<IPage> DisplayedPages
        {
            get { return (IEnumerable<IPage>)GetValue(DisplayedPagesProperty); }
            set { SetValue(DisplayedPagesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayedPages.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayedPagesProperty =
            DependencyProperty.Register("DisplayedPages", typeof(IEnumerable<IPage>), typeof(AnagramPagedItemsControl), new PropertyMetadata(null, new PropertyChangedCallback(DisplayedPagesChanged)));

        private static void DisplayedPagesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnagramPagedItemsControl itemsControl = d as AnagramPagedItemsControl;

            itemsControl.RecalculateDisplayedButtons((IEnumerable<IPage>)e.NewValue);
        }

        private void RecalculateDisplayedButtons(IEnumerable<IPage> newPages)
        {
            if (DisplayedButtons == null)
                DisplayedButtons = new ObservableCollection<Button>();

            //Go through each button and unsubscribe from the click event
            foreach (Button btn in DisplayedButtons)
                btn.Click -= PART_ButtonGoToPage_Click;

            //Clear all displayed buttons (We'll redraw them)
            DisplayedButtons.Clear();

            //Go through each displayed page and create a button for it.
            foreach (IPage page in newPages)
            {
                //Create a new button for the page
                Button btn = new Button()
                {
                    Content = page.Index.ToString(),
                    Tag = page,
                    Margin = new Thickness(0, 0, 3, 0)
                };

                //Make it apparent if it's the current page.
                if (page.Index == PagedCollection.CurrentPageIndex)
                    btn.Background = SelectedPageColour;

                //Subscribe to click and add.
                btn.Click += PART_ButtonGoToPage_Click;
                DisplayedButtons.Add(btn);
            }
        }

        public ItemsControl CollectionView
        {
            get { return (ItemsControl)GetValue(CollectionViewProperty); }
            set { SetValue(CollectionViewProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CollectionView.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CollectionViewProperty =
            DependencyProperty.Register("CollectionView", typeof(ItemsControl), typeof(AnagramPagedItemsControl), new PropertyMetadata(null, new PropertyChangedCallback(CollectionViewChanged)));

        private static void CollectionViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnagramPagedItemsControl itemsControl = d as AnagramPagedItemsControl;

            itemsControl.SetupBindings(e.NewValue as ItemsControl);
        }

        private void SetupBindings(ItemsControl PART_CollectionView)
        {
            //Set the bindings for the new control
            PART_CollectionView.SetBinding(
                ItemsControl.ItemsSourceProperty,
                new Binding("PagedCollection.CurrentPage") { Source = this });

            //Bind to SelectedItem dependency property.
            if (PART_CollectionView as Selector != null)
                PART_CollectionView.SetBinding(
                    Selector.SelectedItemProperty,
                    new Binding("SelectedItem") { Source = this });

            PART_CollectionView.SetValue(ItemsControl.ItemTemplateProperty, this.ItemTemplate);
        }

        static AnagramPagedItemsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AnagramPagedItemsControl), new FrameworkPropertyMetadata(typeof(AnagramPagedItemsControl)));
        }

        public override void OnApplyTemplate()
        {
            this.DisplayedButtons = new ObservableCollection<Button>();

            Button PART_ButtonFirst = this.GetTemplateChild("PART_ButtonFirst") as Button;
            PART_ButtonFirst.Click += PART_ButtonFirst_Click;

            Button PART_ButtonPrev = this.GetTemplateChild("PART_ButtonPrev") as Button;
            PART_ButtonPrev.Click += PART_ButtonPrev_Click;

            Button PART_ButtonNext = this.GetTemplateChild("PART_ButtonNext") as Button;
            PART_ButtonNext.Click += PART_ButtonNext_Click;

            Button PART_ButtonLast = this.GetTemplateChild("PART_ButtonLast") as Button;
            PART_ButtonLast.Click += PART_ButtonLast_Click;

            Button PART_ButtonRefreshAll = this.GetTemplateChild("PART_ButtonRefreshAll") as Button;
            PART_ButtonRefreshAll.Click += PART_RefreshAll_Click;

            ItemsControl PART_ButtonList = this.GetTemplateChild("PART_ButtonList") as ItemsControl;

            PART_ButtonList.SetBinding(
                ItemsControl.ItemsSourceProperty,
                new Binding("DisplayedButtons") { Source = this });

            ComboBox PART_ItemsPerPage = this.GetTemplateChild("PART_ItemsPerPage") as ComboBox;

            PART_ItemsPerPage.SetBinding(
                ComboBox.ItemsSourceProperty,
                new Binding("PagedCollection.ItemsPerPageOptions") { Source = this });

            PART_ItemsPerPage.SetBinding(
                ComboBox.SelectedItemProperty,
                new Binding("PagedCollection.ItemsPerPage") { Source = this });

            ContentPresenter PART_CollectionViewPresenter = this.GetTemplateChild("PART_CollectionViewPresenter") as ContentPresenter;

            base.OnApplyTemplate();
        }

        #region page navigation

        private void newCol_OnNewPageNavigated(object sender, IPage e)
        {
            RecalculateDisplayedPages();
        }

        private void PagedCollection_OnPageCreated(object sender, IPage e)
        {
            //If there is already 11 pages displayed, then 
            //there is no need to recalculate.
            if (DisplayedPages != null &&
                DisplayedPages.Count() >= 11)
                return;

            RecalculateDisplayedPages();
        }

        private void RecalculateDisplayedPages()
        {
            IPage current = PagedCollection.GetCurrentPage();

            if (current == null)
                return;

            int currentPage = current.Index;
            int left = currentPage - 5;
            int right = currentPage + 5;

            //If left is 0 or less, then add the difference to 0 to right.
            if (left <= 0)
            {
                right += (left - 1) * -1;
                left = 0;
            }

            //If right is equal or greater than the current number of pages, then add the difference to left.
            if (right >= PagedCollection.PageCount)
                left -= right - PagedCollection.PageCount;

            //Get pages left and right by X to be the displayed pages.
            DisplayedPages = PagedCollection.GetPages(
                left,
                right);

            SelectedItem = PagedCollection.GetCurrentPage().FirstOrDefault();
        }

        void PART_RefreshAll_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedItem = null;

            PagedCollection.LoadPages();
            PagedCollection.FirstPage();
        }

        void PART_ButtonFirst_Click(object sender, RoutedEventArgs e)
        {
            PagedCollection.FirstPage();
        }

        void PART_ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            PagedCollection.NextPage();
        }

        void PART_ButtonPrev_Click(object sender, RoutedEventArgs e)
        {
            PagedCollection.PreviousPage();
        }

        void PART_ButtonLast_Click(object sender, RoutedEventArgs e)
        {
            PagedCollection.LastPage();
        }

        private void PART_ButtonGoToPage_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            IPage page = btn.Tag as IPage;

            PagedCollection.GoToPage(page.Index);
        }

        #endregion
    }
}
