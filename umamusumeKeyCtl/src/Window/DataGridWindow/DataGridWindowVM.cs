using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using umamusumeKeyCtl.Annotations;

namespace umamusumeKeyCtl
{
    public class DataGridWindowVM : INotifyPropertyChanged
    {
        private List<MatchingResult> _results = new();

        public List<MatchingResult> Results
        {
            get => _results;
            set
            {
                _results = value;
                OnPropertyChanged("Results");
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