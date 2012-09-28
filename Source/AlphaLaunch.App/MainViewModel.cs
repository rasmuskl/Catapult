using System;
using System.ComponentModel;

namespace AlphaLaunch.App
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _search;

        public MainViewModel()
        {
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
        }

        public string Search
        {
            get { return _search; }
            set
            {
                if (_search != value)
                {
                    _search = value;
                    OnPropertyChanged("Search");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}