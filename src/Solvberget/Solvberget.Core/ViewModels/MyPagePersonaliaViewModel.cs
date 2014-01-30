﻿using Solvberget.Core.Services.Interfaces;
using Solvberget.Core.ViewModels.Base;

namespace Solvberget.Core.ViewModels
{
    public class MyPagePersonaliaViewModel : BaseViewModel
    {
        private readonly IUserService _service;
        private readonly IUserAuthenticationDataService _userAuthenticationService;

        public MyPagePersonaliaViewModel(IUserService service, IUserAuthenticationDataService userAuthenticationService)
        {
			Title = "Min profil";
            _userAuthenticationService = userAuthenticationService;
            _service = service;
            Load();
        }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        private string _email;
        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                _email = value;
                RaisePropertyChanged(() => Email);
            }
        }
        private string _streetAdress;
        public string StreetAdress
        {
            get
            {
                return _streetAdress;
            }
            set
            {
                _streetAdress = value;
                RaisePropertyChanged(() => StreetAdress);
            }
        }

        private string _cityAdress;
        public string CityAdress
        {
            get
            {
                return _cityAdress;
            }
            set
            {
                _cityAdress = value;
                RaisePropertyChanged(() => CityAdress);
            }
        }

        private string _cellPhoneNumber;
        public string CellPhoneNumber
        {
            get
            {
                return _cellPhoneNumber;
            }
            set
            {
                _cellPhoneNumber = value;
                RaisePropertyChanged(() => CellPhoneNumber);
            }
        }

        private string _dateOfBirth;
        public string DateOfBirth
        {
            get
            {
                return _dateOfBirth;
            }
            set
            {
                _dateOfBirth = value;
                RaisePropertyChanged(() => DateOfBirth);
            }
        }

        private string _balance;
        public string Balance 
        {
            get { return _balance; }
            set { _balance = value; RaisePropertyChanged(() => Balance);}
        }

        private string _credit;
        public string Credit 
        {
            get { return _credit; }
            set { _credit = value; RaisePropertyChanged(() => Credit);}
        }

        private string _homeLibrary;
        public string HomeLibrary 
        {
            get { return _homeLibrary; }
            set { _homeLibrary = value; RaisePropertyChanged(() => HomeLibrary);}
        }

        public async void Load()
        {
            IsLoading = true;
            var user = await _service.GetUserInformation();

            DateOfBirth = user.DateOfBirth;
            CellPhoneNumber = user.CellPhoneNumber;
            CityAdress = user.CityAddress;
            StreetAdress = user.StreetAddress;
            Email = user.Email;
            Name = user.Name;
			Balance = user.Balance;
			Credit = user.CashLimit;
			HomeLibrary = user.HomeLibrary;

			IsLoading = false;
			NotifyViewModelReady();
        }
    }
}
