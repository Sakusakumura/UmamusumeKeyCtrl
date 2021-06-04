using System.ComponentModel;
using System.Runtime.CompilerServices;
using umamusumeKeyCtl.Annotations;

namespace umamusumeKeyCtl
{
    public class MessageWindowVM : INotifyPropertyChanged
    {
        private string _textBlockText;

        public string TextBlockText
        {
            get => _textBlockText;
            set
            {
                _textBlockText = value;
                OnPropertyChanged("TextBlockText");
            }
        }
    
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}