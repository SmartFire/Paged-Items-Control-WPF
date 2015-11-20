using PagedItemsControl.Controls;
using PagedItemsControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagedItemsControl.ViewModels
{
    public class TestItemsViewModel
    {
        public LazyPagedCollection<TestItem> Models { get; set; }

        public TestItemsViewModel()
        {
            CreateTestItems();
        }

        private void CreateTestItems()
        {
            List<TestItem> items = new List<TestItem>();

            //Create a bunch of test items.
            for (int i = 0; i < 500; i++)
            {
                TestItem item = new TestItem()
                {
                    Index = i,
                    Name = string.Format("Item {0}", i)
                };

                items.Add(item);
            }

            //Create the lazy paged collection, this will be bound to the 
            //paged collection control
            Models = new LazyPagedCollection<TestItem>(items);
            Models.FirstPage();
        }
    }
}
