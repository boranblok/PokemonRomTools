using GalaSoft.MvvmLight.Command;
using PkmnAdvanceTranslation.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Data;
using System.Windows.Threading;

namespace PkmnAdvanceTranslation.ViewModels
{
    public class FilterViewModel : ViewModelBase
    {
        private ContainsViewModel _currentContainsMode;
        private Boolean _inverseGroupFilter;
        private String _addressFilter;
        private String _contentFilter;
        private String[] _contentFilters;
        private Nullable<Boolean> _translatedFilter;
        private Nullable<Boolean> _unsavedFilter;

        private Dispatcher dispatcher;
        private DateTime filterStart;
        private Timer filterDelayTimer;

        public event EventHandler FilterChanged;

        public FilterViewModel()
        {
            LoadContainsModes();

            Groups = new ObservableCollection<String>();
            GroupsView = CollectionViewSource.GetDefaultView(Groups);
            GroupsView.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));
            SelectedGroups = new ObservableCollection<String>();
            SelectedGroups.CollectionChanged += SelectedGroups_CollectionChanged;

            filterDelayTimer = new Timer();
            filterDelayTimer.Interval = 50;
            filterDelayTimer.AutoReset = true;
            filterDelayTimer.Elapsed += FilterDelayTimer_Elapsed;

            dispatcher = Dispatcher.CurrentDispatcher;
        }

        private void OnFilterChanged()
        {
            OnPropertyChanged("ClearFilterCommand");
            FilterChanged?.Invoke(this, new EventArgs());
        }

        private void LoadContainsModes()
        {
            ContainsModes = new List<ContainsViewModel>();
            foreach (var value in Enum.GetValues(typeof(ContainFilterMode)))
            {
                var enumValue = (ContainFilterMode)value;
                ContainsModes.Add(new ContainsViewModel(enumValue.ToString(), enumValue));
            }
            _currentContainsMode = ContainsModes[0];
        }

        public ObservableCollection<String> Groups { get; private set; }
        public ICollectionView GroupsView { get; private set; }
        public ObservableCollection<String> SelectedGroups { get; private set; }
        public List<ContainsViewModel> ContainsModes { get; private set; }

        public Boolean MatchesFilter(TranslationItemViewModel translationLine)
        {
            if (translationLine == null)
                return false;
            return (!TranslatedFilter.HasValue || translationLine.IsTranslated == TranslatedFilter.Value)
                && (!EditingFilter.HasValue || translationLine.HasChangesInEditor == EditingFilter.Value)
                && (String.IsNullOrWhiteSpace(AddressFilter) || translationLine.Address.StartsWith(AddressFilter, StringComparison.InvariantCultureIgnoreCase))
                && (SelectedGroups.Count == 0 || (InverseGroupFilter && !SelectedGroups.Contains(translationLine.Group)) || (!InverseGroupFilter && SelectedGroups.Contains(translationLine.Group)))
                && MatchesContentFilter(translationLine)
                ;
        }


        private Boolean MatchesContentFilter(TranslationItemViewModel translationLine)
        {
            if (String.IsNullOrWhiteSpace(ContentFilter))
                return true;
            switch (CurrentContainsMode.FilterMode)
            {
                case ContainFilterMode.Both:
                    foreach (var filter in _contentFilters)
                    {
                        if (translationLine.TranslatedSingleLine.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0
                            && translationLine.UnTranslatedSingleLine.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0)
                            return true;
                    }
                    return false;
                case ContainFilterMode.Translated:
                    foreach (var filter in _contentFilters)
                    {
                        if (translationLine.TranslatedSingleLine.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0)
                            return true;
                    }
                    return false;
                case ContainFilterMode.Untranslated:
                    foreach (var filter in _contentFilters)
                    {
                        if (translationLine.UnTranslatedSingleLine.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0)
                            return true;
                    }
                    return false;
                default:
                    return true;
            }
        }

        public Nullable<Boolean> TranslatedFilter
        {
            get
            {
                return _translatedFilter;
            }
            set
            {
                if (value == _translatedFilter)
                    return;
                _translatedFilter = value;
                OnPropertyChanged("TranslatedFilter");
                OnFilterChanged();
            }
        }

        public Nullable<Boolean> EditingFilter
        {
            get
            {
                return _unsavedFilter;
            }
            set
            {
                if (value == _unsavedFilter)
                    return;
                _unsavedFilter = value;
                OnPropertyChanged("EditingFilter");
                OnFilterChanged();
            }
        }

        private void SelectedGroups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnFilterChanged();
        }

        public Boolean InverseGroupFilter
        {
            get
            {
                return _inverseGroupFilter;
            }
            set
            {
                if (value == _inverseGroupFilter)
                    return;
                _inverseGroupFilter = value;
                OnPropertyChanged("InverseGroupFilter");
                OnFilterChanged();
            }
        }

        public String AddressFilter
        {
            get { return _addressFilter; }
            set
            {
                if (value == _addressFilter)
                    return;

                _addressFilter = value;
                OnPropertyChanged("AddressFilter");
                FilterChangedAsync();
            }
        }

        public ContainsViewModel CurrentContainsMode
        {
            get { return _currentContainsMode; }
            set
            {
                if (value == _currentContainsMode)
                    return;

                _currentContainsMode = value;
                OnPropertyChanged("CurrentContainsMode");
                OnFilterChanged();
            }
        }

        public String ContentFilter
        {
            get { return _contentFilter; }
            set
            {
                if (value == _contentFilter)
                    return;

                _contentFilter = value;
                OnPropertyChanged("ContentFilter");
                FilterChangedAsync();
            }
        }

        private void FilterChangedAsync()
        {
            filterStart = DateTime.Now;

            if (!filterDelayTimer.Enabled)
            {
                filterDelayTimer.Enabled = true;
                filterDelayTimer.Start();
            }
        }

        private void FilterDelayTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if ((DateTime.Now - filterStart).TotalSeconds > .3) //We only apply the typed filter 0.3 seconds after the user stopped typing.
            {
                dispatcher.BeginInvoke((Action)delegate ()
                {
                    filterDelayTimer.Stop();
                    _contentFilters = ContentFilter.Split(new Char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    OnFilterChanged();
                });
            }
        }

        public RelayCommand ClearFilterCommand
        {
            get
            {
                return new RelayCommand(ClearFilter, CanClearFilter);
            }
        }

        private void ClearFilter()
        {
            _translatedFilter = null;            
            _unsavedFilter = null;            
            SelectedGroups.Clear();
            _addressFilter = null;            
            _contentFilter = null;            
            _currentContainsMode = ContainsModes[0];            
            _inverseGroupFilter = false;

            OnPropertyChanged("TranslatedFilter");
            OnPropertyChanged("EditingFilter");
            OnPropertyChanged("AddressFilter");
            OnPropertyChanged("ContentFilter");
            OnPropertyChanged("CurrentContainsMode");
            OnPropertyChanged("InverseGroupFilter");

            OnFilterChanged();
        }

        private bool CanClearFilter()
        {
            return TranslatedFilter.HasValue
                || EditingFilter.HasValue
                || SelectedGroups.Count > 0
                || !String.IsNullOrWhiteSpace(AddressFilter)
                || !String.IsNullOrWhiteSpace(ContentFilter)
                || _currentContainsMode != ContainsModes[0];
        }
    }

}
