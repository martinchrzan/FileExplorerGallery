using System;
using System.ComponentModel;

namespace FileExplorerGallery.Settings
{
    public sealed class SettingItem<T> : INotifyPropertyChanged
    {
        private T _value;
        private readonly Action<T> _setValue;

        public SettingItem(T startValue, Action<T> setValue)
        {
            _value = startValue;
            _setValue = setValue;
        }

        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                _setValue.Invoke(value);
                OnValueChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnValueChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }
    }
}
