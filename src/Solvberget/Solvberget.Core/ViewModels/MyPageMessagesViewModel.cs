using System.Collections.Generic;
using System.Linq;
using Solvberget.Core.DTOs;
using Solvberget.Core.Services.Interfaces;
using Solvberget.Core.ViewModels.Base;

namespace Solvberget.Core.ViewModels
{
    public class MyPageMessagesViewModel : BaseViewModel
    {
        private readonly IUserService _service;
        private readonly IUserAuthenticationDataService _userAuthenticationService;

        public MyPageMessagesViewModel(IUserService service, IUserAuthenticationDataService userAuthenticationService)
        {
			Title = "Meldinger";
            _userAuthenticationService = userAuthenticationService;
            _service = service;
            Load();
        }

        private List<NotificationDto> _notifications;
        public List<NotificationDto> Notifications
        {
            get { return _notifications; }
            set { _notifications = value; RaisePropertyChanged(() => Notifications); }
        }

        public async void Load()
        {
            IsLoading = true;
            var user = await _service.GetUserInformation();

            Notifications = user.Notifications == null ? new List<NotificationDto>() : user.Notifications.ToList();

			if (Notifications.Count == 0 && AddEmptyItemForEmptyLists)
            {
                Notifications = new List<NotificationDto>
                {
                    new NotificationDto
                    {
                        Title = "Du har ingen meldinger",
                        Content = "Du f�r beskjed n�r l�n forfaller, n�r noe er klart til henting, og n�r du f�r et gebyr"
                    }
                };
            }
			IsLoading = false;
			NotifyViewModelReady();
        }
    }
}
