using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using umamusumeKeyCtl.Annotations;

namespace umamusumeKeyCtl
{
    public class DataGridWindowVM
    {
        private ObservableCollection<DataGridItem> _results = new();

        public ObservableCollection<DataGridItem> Results
        {
            get => _results;
            set
            {
                _results = value;
            }
        }
        
        public DataGridWindowVM()
        {
        }

        public void UpdateResults(List<MatchingResult> results)
        {
            foreach (var dataGridItem in Results.ToArray())
            {
                if (!results.Exists(val => val.SceneName == dataGridItem.SceneName))
                {
                    Results.Remove(dataGridItem);
                }
            }

            foreach (var matchingResult in results)
            {
                var find = Results.ToList().Find(val => val.SceneName == matchingResult.SceneName);
                if (find == null)
                {
                    Results.Add(new DataGridItem(matchingResult.Result, matchingResult.Score, matchingResult.SceneName));
                    continue;
                }

                if (find.Result != matchingResult.Result) find.Result = matchingResult.Result;
                if (find.Score != matchingResult.Score) find.Score = matchingResult.Score;
                if (find.SceneName != matchingResult.SceneName) find.SceneName = matchingResult.SceneName;
            }

            var sorted = Results.OrderByDescending(val => val.Result).ThenBy(val => val.Score).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                Results.Move(Results.IndexOf(sorted[i]), i);
            }
        }
    }

    public class DataGridItem : INotifyPropertyChanged
    {
        private bool _result;

        public bool Result
        {
            get => _result;
            set
            {
                _result = value;
                OnItemPropertyChanged("Result");
            }
        }

        private double _score;

        public double Score
        {
            get => _score;
            set
            {
                _score = value;
                OnItemPropertyChanged("Score");
            }
        }

        private string _sceneName;

        public string SceneName
        {
            get => _sceneName;
            set
            {
                _sceneName = value;
                OnItemPropertyChanged("SceneName");
            }
        }

        public DataGridItem()
        {
        }

        public DataGridItem(bool result, double score, string sceneName)
        {
            Result = result;
            Score = score;
            SceneName = sceneName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnItemPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}