﻿using Solvberget.Core.Services.Interfaces;
using Solvberget.Core.ViewModels.Base;

namespace Solvberget.Core.ViewModels
{
    public class MyPageViewModel : BaseViewModel
    {
        readonly IUserAuthenticationDataService _userAuthenticationDataService;

        public MyPageViewModel(IUserService service, IUserAuthenticationDataService userAuthenticationDataService)
        {
			_userAuthenticationDataService = userAuthenticationDataService;

            MyPageLoansViewModel = new MyPageLoansViewModel(service, userAuthenticationDataService);
            MyPagePersonaliaViewModel = new MyPagePersonaliaViewModel(service, userAuthenticationDataService);
            MyPageReservationsViewModel = new MyPageReservationsViewModel(service);
            MyPageFinesViewModel = new MyPageFinesViewModel(service, userAuthenticationDataService);
            MyPageMessagesViewModel = new MyPageMessagesViewModel(service, userAuthenticationDataService);
            MyPageFavoritesViewModel = new MyPageFavoritesViewModel(service);

			Title = "Min side";
			IsLoading = false;
			NotifyViewModelReady();
        }

        private MyPageLoansViewModel _myPageLoansViewModel;
        public MyPageLoansViewModel MyPageLoansViewModel
        {
            get { return _myPageLoansViewModel; }
            set { _myPageLoansViewModel = value; RaisePropertyChanged(() => MyPageLoansViewModel); }
        }

        private MyPagePersonaliaViewModel _myPagePersonaliaViewModel;
        public MyPagePersonaliaViewModel MyPagePersonaliaViewModel
        {
            get { return _myPagePersonaliaViewModel; }
            set { _myPagePersonaliaViewModel = value; RaisePropertyChanged(() => MyPagePersonaliaViewModel); }
        }

        private MyPageReservationsViewModel _myPageReservationsViewModel;
        public MyPageReservationsViewModel MyPageReservationsViewModel
        {
            get { return _myPageReservationsViewModel; }
            set { _myPageReservationsViewModel = value; RaisePropertyChanged(() => MyPagePersonaliaViewModel); }
        }

        private MyPageFinesViewModel _myPageFinesViewModel;
        public MyPageFinesViewModel MyPageFinesViewModel
        {
            get { return _myPageFinesViewModel; }
            set { _myPageFinesViewModel = value; RaisePropertyChanged(() => MyPageFinesViewModel); }
        }

        private MyPageMessagesViewModel _myPageMessagesViewModel;
        public MyPageMessagesViewModel MyPageMessagesViewModel
        {
            get { return _myPageMessagesViewModel; }
            set { _myPageMessagesViewModel = value; RaisePropertyChanged(() => MyPageMessagesViewModel); }
        }
        private MyPageFavoritesViewModel _myPageFavoritesViewModel;
        public MyPageFavoritesViewModel MyPageFavoritesViewModel
        {
            get { return _myPageFavoritesViewModel; }
            set { _myPageFavoritesViewModel = value; RaisePropertyChanged(() => MyPageFavoritesViewModel); }
        }

		public void Logout()
		{
			_userAuthenticationDataService.RemoveUser();
			_userAuthenticationDataService.RemovePassword();

			ShowViewModel<HomeScreenViewModel>();
		}
    }
}
