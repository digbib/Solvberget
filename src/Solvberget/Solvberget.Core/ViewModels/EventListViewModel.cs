﻿using System.Collections.Generic;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using Solvberget.Core.Services.Interfaces;
using Solvberget.Core.ViewModels.Base;
using System.Linq;

namespace Solvberget.Core.ViewModels
{
    public class EventListViewModel : BaseViewModel
    {
        private readonly IEventService _eventService;

        public EventListViewModel(IEventService eventService)
        {
            _eventService = eventService;
        }

        public void Init()
        {
			Title = "Arrangementer";
            Load();
        }

        private async void Load()
        {
            IsLoading = true;

            Events = (from ev in await _eventService.GetList()
                select new EventViewModel
                {
                    Description = ev.Description,
                    Title = ev.Name,
                    ImageUrl = ev.ImageUrl,
                    Location = ev.Location,
					Price = (ev.TicketPrice > 0.0) ? "kr. " + ev.TicketPrice.ToString("0") : "gratis",
					Date = ev.Start.ToString("d. MMM"),
					Time = ev.Start.Hour == 0 && ev.End.Hour == 0 ? null :  "kl. " + ev.Start.ToString("HH:mm") + "-" + ev.End.ToString("HH:mm"),
                    Url = ev.TicketUrl
                }).ToList();

			IsLoading = false;
			NotifyViewModelReady();
        }

        private List<EventViewModel> _events;
        public List<EventViewModel> Events 
        {
            get { return _events; }
            set { _events = value; RaisePropertyChanged(() => Events);}
        }

        private MvxCommand<EventViewModel> _showDetailsCommand;
        public ICommand ShowDetailsCommand
        {
            get
            {
                return _showDetailsCommand ?? (_showDetailsCommand = new MvxCommand<EventViewModel>(ExecuteShowDetailsCommand));
            }
        }

        private void ExecuteShowDetailsCommand(EventViewModel ev)
        {
            ShowViewModel<GenericWebViewViewModel>(new { uri = ev.Url, title = ev.Title });
        }
    }
}
