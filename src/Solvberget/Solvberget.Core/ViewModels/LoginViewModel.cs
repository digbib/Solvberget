﻿using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using Solvberget.Core.Services.Interfaces;
using Solvberget.Core.ViewModels.Base;

namespace Solvberget.Core.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IUserService _service;
        private readonly IUserAuthenticationDataService _userAuthenticationService;

        public LoginViewModel(IUserService service, IUserAuthenticationDataService userAuthenticationDataService)
        {
            _service = service;
            _userAuthenticationService = userAuthenticationDataService;

			Title = "Logg inn";
			NotifyViewModelReady();
        }

		bool _navigateBackOnLogin;

		public void Init(bool navigateBackOnLogin = false)
		{
			_navigateBackOnLogin = navigateBackOnLogin;
		}

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; RaisePropertyChanged(() => UserName); }
        }

        private string _pin;
        public string Pin
        {
            get { return _pin; }
            set { _pin = value; RaisePropertyChanged(() => Pin); }
        }

        private bool _loggedIn;
        public bool LoggedIn
        {
            get { return _loggedIn; }
            set { _loggedIn = value; RaisePropertyChanged(() => LoggedIn); }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set { _message = value; RaisePropertyChanged(() => Message); }
        }

        private bool _buttonPressed;
        public bool ButtonPressed 
        {
            get { return _buttonPressed; }
            set { _buttonPressed = value; RaisePropertyChanged(() => ButtonPressed);}
        } 

        private MvxCommand<MyPageViewModel> _loginCommand;
        public ICommand LoginCommand
        {
            get
            {
                return _loginCommand ?? (_loginCommand = new MvxCommand<MyPageViewModel>(ExecuteLoginCommand));
            }
        }

        private async void ExecuteLoginCommand(MyPageViewModel page)
        {
            IsLoading = true;

            ButtonPressed = true;
            _userAuthenticationService.SetUser(UserName);
            _userAuthenticationService.SetPassword(Pin);

            var response = await _service.Login(UserName, Pin);
            IsLoading = false;

            if (response.Message.Equals("Autentisering vellykket."))
            {
                LoggedIn = true;
				if (_navigateBackOnLogin) Close(this);
				else ShowViewModel<MyPageViewModel>();
            }
            else if (response.Message.Equals("The remote server returned an error: (401) Unauthorized."))
            {
                Message = "Feil brukernavn eller passord";
                _userAuthenticationService.RemoveUser();
                _userAuthenticationService.RemovePassword();
            }
            else
            {
                Message = "Noe gikk galt. Prøv igjen senere";
                _userAuthenticationService.RemoveUser();
                _userAuthenticationService.RemovePassword();
            }
        }

        private MvxCommand<string> _forgotPasswordCommand;
        public ICommand ForgotPasswordCommand
        {
            get
            {
                return _forgotPasswordCommand ?? (_forgotPasswordCommand = new MvxCommand<string>(ExecuteForgotPasswordCommand));
            }
        }

        private async void ExecuteForgotPasswordCommand(string userId)
        {
            IsLoading = true;
            if (string.IsNullOrEmpty(userId))
            {
                Message = "Ugyldig lånernummer";
                IsLoading = false;
                return;
            }

            var result = await _service.RequestPinCode(userId);
            if (result.Success)
            {
                Message = "SMS med din PIN-kode har blitt sendt.";
            }
            else
            {
                Message = result.Reply.Contains("Vennligst sjekk lånenummeret.") 
                    ? "Kunne ikke sende PIN-kode. Lånernummeret ble ikke funnet." 
                    : "Kunne ikke sende PIN-kode. Prøv igjen senere eller ta kontakt med Sølvberget.";
            }

            IsLoading = false;
        }
    }
}
