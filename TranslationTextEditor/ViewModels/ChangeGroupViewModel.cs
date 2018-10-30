﻿using GalaSoft.MvvmLight.CommandWpf;
using PkmnAdvanceTranslation.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PkmnAdvanceTranslation.ViewModels
{
    public class ChangeGroupViewModel : DialogViewModelBase
    {
        private ObservableCollection<String> _groups;
        private String _selectedGroup;
        public String _newGroup;

        public ChangeGroupViewModel()
        {
            Title = "Select new group for selected lines";
        }

        public ObservableCollection<String> Groups
        {
            get
            {
                if (_groups == null)
                {
                    _groups = new ObservableCollection<String>();
                }
                return _groups;
            }
            set
            {
                _groups = value;
            }
        }
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