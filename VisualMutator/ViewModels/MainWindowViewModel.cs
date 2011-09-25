﻿namespace VisualMutator.ViewModels
{
    #region Usings

    using CommonUtilityInfrastructure.WpfUtils;

    using VisualMutator.Views.Abstract;

    #endregion

    public class MainWindowViewModel : ViewModel<IMainControl>
    {
        private IView _iLMutationsView;

        private IView _unitTestsView;

        public MainWindowViewModel(IMainControl view)
            : base(view)
        {
        }

        public IView ILMutationsView
        {
            set
            {
                _iLMutationsView = value;
                RaisePropertyChanged(() => ILMutationsView);
            }
            get
            {
                return _iLMutationsView;
            }
        }

        public IView UnitTestsView
        {
            set
            {
                _unitTestsView = value;
                RaisePropertyChanged(() => UnitTestsView);
            }
            get
            {
                return _unitTestsView;
            }
        }


        private int _selectedTab;

        public int SelectedTab
        {
            get
            {
                return _selectedTab;
            }
            set
            {
                SetAndRise(ref _selectedTab, value, () => SelectedTab);
            }
        }
    }
}