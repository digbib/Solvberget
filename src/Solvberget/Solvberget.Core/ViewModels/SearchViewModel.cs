﻿using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using Solvberget.Core.Services.Interfaces;
using Solvberget.Core.ViewModels.Base;
using Solvberget.Core.Properties;

namespace Solvberget.Core.ViewModels
{
    public class SearchViewModel : BaseViewModel 
    {
        private readonly ISearchService _searchService;

        public SearchViewModel(ISearchService searchService)
        {
            _searchService = searchService;
            Title = "Søk";
        }

        private string _query;
        public string Query 
        {
            get { return _query; }
            set { _query = value; RaisePropertyChanged(() => Query);}
        }

        private string _lastQuery;
        public string LastQuery 
        {
            get { return _lastQuery; }
            set { _lastQuery = value; RaisePropertyChanged(() => LastQuery);}
        }

        private IList<SearchResultViewModel> _results;
        public IList<SearchResultViewModel> Results 
        {
            get { return ListOrEmptyResult(_results); }
            set { 
                _results = value; 
                RaisePropertyChanged(() => Results);
                RaisePropertyChanged(() => BookResults);
                RaisePropertyChanged(() => MovieResults);
                RaisePropertyChanged(() => AudioBookResults);
                RaisePropertyChanged(() => CDResults);
                RaisePropertyChanged(() => SheetMusicResults);
                RaisePropertyChanged(() => OtherResults);
                RaisePropertyChanged(() => MagazineResults);
                RaisePropertyChanged(() => GameResults);
            }
        }

        private MvxCommand<SearchResultViewModel> _showDetailsCommand;
        public ICommand ShowDetailsCommand
        {
            get
            {
                return _showDetailsCommand ?? (_showDetailsCommand = new MvxCommand<SearchResultViewModel>(ExecuteShowDetailsCommand));
            }
        }

        private void ExecuteShowDetailsCommand(SearchResultViewModel searchResultViewModel)
        {
            if (searchResultViewModel.DocNumber != "")
            {
                ShowViewModel<MediaDetailViewModel>(new { title = searchResultViewModel.Name, docId = searchResultViewModel.DocNumber });
            }
        }

        // Loads a a set of Documents retrieved from the service into the results list.
        public async void SearchAndLoad()
        {
            IsLoading = true;
            var lastquery = Query;

            var results = await _searchService.Search(Query);
            Results = (from document in results
                        select new SearchResultViewModel
                        {
                            Name = document.Title,
                            Type = document.Type,
                            Image = Resources.ServiceUrl + string.Format(Resources.ServiceUrl_MediaImage, document.Id),
                            Year = (document.Year != 0) ? document.Year.ToString("####") : "Ukjent år",
                            DocNumber = document.Id,
                        }).ToList();

            LastQuery = string.Format("Resultater for: {0}", lastquery);
			IsLoading = false;
        }

		public void ClearResults()
		{
			Results = null;
		}

        public IList<SearchResultViewModel> BookResults 
        {
            get { return ListOrEmptyResult(_results.Where(r => r.Type == "Book").ToList()); }
        }
        public IList<SearchResultViewModel> MovieResults
        {
            get { return ListOrEmptyResult(_results.Where(r => r.Type == "Film").ToList()); }
        }
        public IList<SearchResultViewModel> AudioBookResults
        {
            get { return ListOrEmptyResult(_results.Where(r => r.Type == "AudioBook").ToList()); }
        }
        public IList<SearchResultViewModel> CDResults
        {
            get { return ListOrEmptyResult(_results.Where(r => r.Type == "Cd").ToList()); }
        }
        public IList<SearchResultViewModel> MagazineResults
        {
            get { return ListOrEmptyResult(_results.Where(r => r.Type == "Journal").ToList()); }
        }
        public IList<SearchResultViewModel> SheetMusicResults
        {
            get { return ListOrEmptyResult(_results.Where(r => r.Type == "SheetMusic").ToList()); }
        }
        public IList<SearchResultViewModel> GameResults
        {
            get { return ListOrEmptyResult(_results.Where(r => r.Type == "Game").ToList()); }
        }
        public IList<SearchResultViewModel> OtherResults
        {
            get { return ListOrEmptyResult(_results.Where(r => r.Type == "Other" || r.Type == "Other").ToList()); }
        }

		public bool EnableListEmptyResult = true;

        private IList<SearchResultViewModel> ListOrEmptyResult(IList<SearchResultViewModel> list)
        {
			if (!EnableListEmptyResult) return list;

			return (list.Count > 0) ? list : new List<SearchResultViewModel> {new SearchResultViewModel
                {
                    DocNumber = "",
                    Id = 0,
                    Name = "Ingen resultat",
                    Type = "Ukjent",
                    Title = "Ingen resultat",
                    Year = ""
                }};
        }
    }
}
