﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Solvberget.Core.DTOs;
using Solvberget.Core.Properties;
using Solvberget.Core.Services.Interfaces;
using Solvberget.Core.ViewModels.Base;
using Cirrious.MvvmCross.ViewModels;
using System.Windows.Input;


namespace Solvberget.Core.ViewModels
{
    public class MyPageReservationsViewModel : BaseViewModel
    {
        private readonly IUserService _service;

        public MyPageReservationsViewModel(IUserService service)
        {
			Title = "Reservasjoner";
            _service = service;
        }

        private ObservableCollection<ReservationViewModel> _reservations;
        public ObservableCollection<ReservationViewModel> Reservations
        {
            get{ return _reservations; }
            set{ _reservations = value; RaisePropertyChanged(() => Reservations); }
        }

        private bool _reservationRemoved;
        public bool ReservationRemoved
        {
            get { return _reservationRemoved; }
            set { _reservationRemoved = value; RaisePropertyChanged(() => ReservationRemoved); }
        }

		public override void OnViewReady()
		{
			base.OnViewReady();
			Load();
		}

        public async void Load()
        {
            IsLoading = true;
            var res = await _service.GetUerReservations() ?? new List<ReservationDto>();

            Reservations = new ObservableCollection<ReservationViewModel>();

            if (res.Count == 1 && res.First().Document.Title == "Ingen reservasjoner")
            {
                res.Remove(res.First());
            }

            foreach (ReservationDto r in res)
            {
                var deadline = "";

                if (r.PickupDeadline != null)
                    deadline = r.ReadyForPickup ? r.PickupDeadline.Value.ToString("dd.MM.yyyy") : null;

                

                Reservations.Add(new ReservationViewModel
                {
                    DocumentTitle = r.Document.Title,
                    DocumentNumber = r.Document.Id,
                    HoldRequestFrom = r.Reserved,
                    ReadyForPickup = r.ReadyForPickup,
                    PickupLocation = r.PickupLocation.Equals("Hovedbibl.") ? "Hovedbiblioteket" : r.PickupLocation,
                    Parent = this,
                    ListEmpty = false,
                    ButtonVisible = true,
                    CancellationButtonVisible = false,
                    ButtonText = "Fjern",
                    Status = r.ReadyForPickup ? "" : "Ikke klar for henting",
                    Image = Resources.ServiceUrl + string.Format(Resources.ServiceUrl_MediaImage, r.Document.Id),
                    PickupDeadline = deadline
                });
            }

			if (Reservations.Count == 0 && AddEmptyItemForEmptyLists)
            {
                Reservations.Add(new ReservationViewModel
                {
                    ListEmpty = true,
                    DocumentTitle = "Du har ingen reservasjoner",
                    Status = "Du kan reservere gjennom mediedetaljsiden, enten gjennom søkeresultater, eller anbefalingslistene.",
                    ButtonVisible = false

                });
            }
			IsLoading = false;
			NotifyViewModelReady();
        }

        public async void RemoveReservation(ReservationViewModel reservationViewModel)
        {
            Reservations.Remove(reservationViewModel);

            ReservationRemoved = true;

            await _service.RemoveReservation(reservationViewModel.DocumentNumber, reservationViewModel.Branch);
        }

        public void AddReservation(ReservationViewModel reservationViewModel)
        {
            Reservations.Add(reservationViewModel);
        }

		private MvxCommand<ReservationViewModel> _showDetailsCommand;
		public ICommand ShowDetailsCommand
		{
			get
			{
				return _showDetailsCommand ?? (_showDetailsCommand = new MvxCommand<ReservationViewModel>(ExecuteShowDetailsCommand));
			}
		}

		private void ExecuteShowDetailsCommand(ReservationViewModel model)
		{
			if (model.DocumentNumber != "")
			{
				ShowViewModel<MediaDetailViewModel>(new { title = model.DocumentTitle, docId = model.DocumentNumber });
			}
		}
    }
}
