using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using umamusumeKeyCtl.Annotations;

namespace umamusumeKeyCtl
{
    public class DataGridWindowVM
    {
        private ObservableCollection<DataGridItem> _dataGridItems = new();

        public ObservableCollection<DataGridItem> DataGridItems
        {
            get => _dataGridItems;
            set
            {
                _dataGridItems = value;
            }
        }
        
        public DataGridWindowVM()
        {
        }

        public void UpdateResults(List<MatchingResult> results)
        {
            var copiedList = DataGridItems.ToList();
            foreach (var dataGridItem in copiedList)
            {
                if (!results.Exists(val => val.SceneName == dataGridItem.SceneName))
                {
                    DataGridItems.Remove(dataGridItem);
                }
            }

            foreach (var matchingResult in results)
            {
                if (!DataGridItems.Any(val => val.SceneName == matchingResult.SceneName))
                {
                    DataGridItems.Add(new DataGridItem(matchingResult.Result, matchingResult.Score, matchingResult.SceneName, matchingResult.Matches.Count));
                    continue;
                }
                var first = DataGridItems.First(val => val.SceneName == matchingResult.SceneName);

                first.MatchCount = matchingResult.Matches.Count;
                first.Result = matchingResult.Result;
                first.Score = matchingResult.Score;
                first.SceneName = matchingResult.SceneName;
            }

            var sorted = DataGridItems.OrderByDescending(val => val.Result).ThenBy(val => val.Score).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                DataGridItems.Move(DataGridItems.IndexOf(sorted[i]), i);
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

        private int _matchCount;

        public int MatchCount
        {
            get => _matchCount;
            set
            {
                _matchCount = value;
                OnItemPropertyChanged("MatchCount");
            }
        }

        public DataGridItem()
        {
        }

        public DataGridItem(bool result, double score, string sceneName, int matchCount)
        {
            Result = result;
            Score = score;
            SceneName = sceneName;
            MatchCount = matchCount;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnItemPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}