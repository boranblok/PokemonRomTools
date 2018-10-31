using GalaSoft.MvvmLight.CommandWpf;
using PkmnAdvanceTranslation.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PkmnAdvanceTranslation.ViewModels
{
    public class ChangeGroupViewModel : DialogViewModelBase
    {
        private String _selectedGroup;
        public String _newGroup;

        public ChangeGroupViewModel(ObservableCollection<String> groups)
        {
            Title = "Select new group for selected lines";
            Groups = groups;
            GroupsView = CollectionViewSource.GetDefaultView(Groups);
            GroupsView.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));
        }

        public ObservableCollection<String> Groups { get; private set; }
        public ICollectionView GroupsView { get; private set; }

        public String SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                _selectedGroup = value;
                OnPropertyChanged("SelectedGroup");
            }
        }
        
        public String NewGroup
        {
            set
            {
                _newGroup = value;                
            }
        }

        public Boolean Confirmed { get; set; }

        public RelayCommand OkCommand
        {
            get
            {
                return new RelayCommand(Confirm);
            }
        }

        private void Confirm()
        {
            if (!String.IsNullOrEmpty(_newGroup))
            {
                Groups.Add(_newGroup);
                SelectedGroup = _newGroup;
            }
            Confirmed = true;
            ShowDialog = false;
        }

        public RelayCommand CancelCommand
        {
            get
            {
                return new RelayCommand(Cancel);
            }
        }

        private void Cancel()
        {
            Confirmed = false;
            ShowDialog = false;
        }
    }
}
