using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace AlphaLaunch.App
{
    public class SmartObservableCollection<T> : ObservableCollection<T>
    {
        public SmartObservableCollection()
        {
        }

        public SmartObservableCollection(IEnumerable<T> collection)
            : base(collection)
        {
        }

        public SmartObservableCollection(List<T> list)
            : base(list)
        {
        }

        public void AddRange(IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                Items.Add(item);
            }

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Reset(IEnumerable<T> range)
        {
            Items.Clear();

            AddRange(range);
        }
    }
}