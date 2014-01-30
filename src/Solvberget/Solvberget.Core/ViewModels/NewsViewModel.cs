using System;
using Solvberget.Core.ViewModels.Base;

namespace Solvberget.Core.ViewModels
{
    public class NewsViewModel : BaseViewModel
    {
        private string _newsTitle;
        public string NewsTitle 
        {
            get { return _newsTitle; }
            set { _newsTitle = value; RaisePropertyChanged(() => NewsTitle);}
        }

        private string _ingress;
        public string Ingress 
        {
            get { return _ingress; }
            set { _ingress = value; RaisePropertyChanged(() => Ingress);}
        }

        private DateTime _published;
        public DateTime Published 
        {
            get { return _published; }
            set { _published = value; RaisePropertyChanged(() => Published);}
        }

        private Uri _uri;
        public Uri Uri 
        {
            get { return _uri; }
            set { _uri = value; RaisePropertyChanged(() => Uri);}
        }
    }
}