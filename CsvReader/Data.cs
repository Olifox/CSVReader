using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CsvReader
{
    class Data : INotifyPropertyChanged
    {
        private string _tagName;
        private string _type;
        private string _value;

        public string TagName
        {
            get { return _tagName; }
            set
            {
                _tagName = value;
                OnPropertyChanged("TagName");
            }
        }

        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged("Type");
            }
        }

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged("Value");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
