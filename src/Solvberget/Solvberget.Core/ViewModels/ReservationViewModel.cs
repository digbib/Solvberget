﻿using System;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using Solvberget.Core.ViewModels.Base;

namespace Solvberget.Core.ViewModels
{
    public class ReservationViewModel : BaseViewModel
    {
        private string _branch;
        public string Branch 
        {
            get { return _branch; }
            set { _branch = value; RaisePropertyChanged(() => Branch);}
        }

        private string _documentNumber;
        public string DocumentNumber
        {
            get { return _documentNumber; }
            set { _documentNumber = value; RaisePropertyChanged(() => DocumentNumber); }
        }

        private string _documentTitle;
        public string DocumentTitle
        {
            get { return _documentTitle; }
            set { _documentTitle = value; RaisePropertyChanged(() => DocumentTitle); }
        }

        private string _pickupLocation;
        public string PickupLocation
        {
            get { return _pickupLocation; }
            set { _pickupLocation = value; RaisePropertyChanged(() => PickupLocation); }
        }

        private string _image;
        public string Image 
        {
            get { return _image; }
            set { _image = value; RaisePropertyChanged(() => Image);}
        }

        private DateTime? _holdRequestFrom;
        public DateTime? HoldRequestFrom
        {
            get { return _holdRequestFrom; }
            set { _holdRequestFrom = value; RaisePropertyChanged(() => HoldRequestFrom); }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set { _status = value; RaisePropertyChanged(() => Status); }
        }

        private MyPageReservationsViewModel _parent;
        public MyPageReservationsViewModel Parent
        {
            get { return _parent; }
            set { _parent = value; RaisePropertyChanged(() => Parent); }
        }

        private bool _buttonVisible;
        public bool ButtonVisible
        {
            get { return _buttonVisible; }
            set { _buttonVisible = value; RaisePropertyChanged(() => ButtonVisible); }
        }

        private string _buttonText;
        public string ButtonText
        {
            get { return _buttonText; }
            set { _buttonText = value; RaisePropertyChanged(() => ButtonText); }
        }

        
        private bool _readyForPickup;
        public bool ReadyForPickup
        {
            get { return _readyForPickup; }
            set { _readyForPickup = value; RaisePropertyChanged(() => ReadyForPickup); }
        }

        private bool _confirmed;
        public bool Confirmed
        {
            get { return _confirmed; }
            set { _confirmed = value; RaisePropertyChanged(() => Confirmed); }
        }

        private bool _cancellationButtonVisible;
        public bool CancellationButtonVisible
        {
            get { return _cancellationButtonVisible; }
            set { _cancellationButtonVisible = value; RaisePropertyChanged(() => CancellationButtonVisible); }
        }

        private string _itemDocumentNumber;
        public string ItemDocumentNumber
        {
            get { return _itemDocumentNumber; }
            set { _itemDocumentNumber = value; RaisePropertyChanged(() => ItemDocumentNumber); }
        }

        private string _pickupDeadline;
        public string PickupDeadline 
        {
            get { return _pickupDeadline; }
            set { _pickupDeadline = value; RaisePropertyChanged(() => PickupDeadline);}
        } 

        private bool _listEmpty;
        public bool ListEmpty
        {
            get { return _listEmpty; }
            set { _listEmpty = value; RaisePropertyChanged(() => ListEmpty); }
        }
        
        private MvxCommand<ReservationViewModel> _cancelRemoveCommand;
        public ICommand CancelRemoveCommand
        {
            get
            {
                return _cancelRemoveCommand ??
                       (_cancelRemoveCommand = new MvxCommand<ReservationViewModel>(ExecuteCancelReservationCommand));
            }
        }

        private MvxCommand<ReservationViewModel> _removeReservationCommand;
        public ICommand RemoveReservationCommand
        {
            get
            {
                return _removeReservationCommand ?? (_removeReservationCommand = new MvxCommand<ReservationViewModel>(ExecuteRemoveReservationCommand));
            }
        }

        private void ExecuteRemoveReservationCommand(ReservationViewModel reservation)
        {
            Confirmed = ButtonText.Equals("Bekreft");

            if (Confirmed)
            {
                Parent.RemoveReservation(this);

                if (Parent.Reservations.Count == 0)
                {
                    Parent.AddReservation(new ReservationViewModel
                    {
                        DocumentTitle = "Du har ingen reservasjoner",
                        Status = "Du kan reservere gjennom mediedetaljsiden, enten gjennom søkeresultater, eller anbefalingslistene.",
                        ButtonVisible = false,
                        ListEmpty = true
                    });
                    ListEmpty = true;
                }
            }
            else
            {
                ButtonText = "Bekreft";

                DocumentTitle = "Vil du fjerne reservasjonen?";
                Status = "Da mister du din plass i køen for å låne denne!";

                CancellationButtonVisible = true;
                ReadyForPickup = false;
            }
        }

        private void ExecuteCancelReservationCommand(ReservationViewModel reservation)
        {
            Parent.Load();
        }
    }
}
