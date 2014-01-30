﻿using System;
using System.Collections.Generic;
using System.Linq;
using Solvberget.Core.DTOs;
using Solvberget.Core.Services.Interfaces;
using Solvberget.Core.ViewModels.Base;

namespace Solvberget.Core.ViewModels
{
    public class MyPageFinesViewModel : BaseViewModel
    {
        private readonly IUserService _service;
        private readonly IUserAuthenticationDataService _userAuthenticationService;

        public MyPageFinesViewModel(IUserService service, IUserAuthenticationDataService userAuthenticationService)
        {
			Title = "Gebyrer";
            _userAuthenticationService = userAuthenticationService;
            _service = service;
            Load();
        }

        private List<FineViewModel> _fines;
        public List<FineViewModel> Fines
        {
            get { return _fines; }
            set { _fines = value; RaisePropertyChanged(() => Fines); }
        }

        public async void Load()
        {
            IsLoading = true;
			var user = await _service.GetUserInformation();

            var finesDtos = user.Fines == null ? new List<FineDto>() : user.Fines.ToList();

            Fines = new List<FineViewModel>();

            foreach (FineDto f in finesDtos)
            {
                Fines.Add(new FineViewModel
                {
                    Description = f.Description,
                    DocumentTitle = (f.Document != null) ? f.Document.Title : "Gebyr",
                    Sum = Convert.ToInt32(f.Sum) + ",-",
                    Status = f.Status
                });
            }
			IsLoading = false;
			NotifyViewModelReady();

            //if (Fines.Count != 0)
            //{
            //    for (var i = 0; i < Fines.Count; i++)
            //    {
            //        if (Fines.ElementAt(i).Status.Equals("Cancelled") || Fines.ElementAt(i).Status.Equals("Paid"))
            //        {
            //            Fines.Remove(Fines.ElementAt(i));
            //            i--;
            //        }
            //    }
            //}

			if (Fines.Count == 0 && AddEmptyItemForEmptyLists)
            {
                Fines.Add(new FineViewModel
                    {
                        DocumentTitle = "Ingen gebyrer",
                        Description =
                            "Du har ingen gebyrer! Det kan du for eksempel få hvis du leverer noe for sent eller mister noe"
                    });
            }
        }
    }
}
