using Android.App;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.BindingContext;
using Solvberget.Core.ViewModels;
using Solvberget.Droid.ActionBar;
using Solvberget.Droid.Views.WebClients;

namespace Solvberget.Droid.Views.Fragments
{
    [Activity(Label = "Webside", Theme = "@style/MyTheme", Icon = "@android:color/transparent", ParentActivity = typeof(HomeView))]
    [MetaData("android.support.PARENT_ACTIVITY", Value = "solvberget.droid.views.HomeView")]
    public class GenericWebViewView : MvxActionBarActivity
    {
        private WebView _webView;

        private GenericWebViewViewModel _viewModel;
        public new GenericWebViewViewModel ViewModel
        {
            get { return _viewModel ?? (_viewModel = base.ViewModel as GenericWebViewViewModel); }
        }

        protected override void OnViewModelSet()
        {
            Window.RequestFeature(WindowFeatures.Progress);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetBackgroundDrawable(Resources.GetDrawable(Resource.Color.s_main_green));
            SupportActionBar.SetLogo(Resource.Drawable.logo_white);

            base.OnViewModelSet();

            SetContentView(Resource.Layout.page_webview);

            
            var set = this.CreateBindingSet<GenericWebViewView, GenericWebViewViewModel>();
            set.Bind(SupportActionBar).For(v => v.Title).To(vm => vm.Title).Mode(MvxBindingMode.OneWay);
            set.Apply();


            _webView = FindViewById<WebView>(Resource.Id.webView);
            _webView.Settings.JavaScriptEnabled = true;
            _webView.Settings.SetSupportZoom(true);


            var progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            var webChromeClient = new ProgressUpdatingWebChromeClient(progressBar);
            var webViewClient = new ProgressHandlingWebViewClient(progressBar);
            _webView.SetWebViewClient(webViewClient);
            _webView.SetWebChromeClient(webChromeClient);
    
            _webView.LoadUrl(ViewModel.Uri);
        }

        protected override void OnResume()
        {
            ViewModel.OnViewReady();
            base.OnResume();
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (e.Action == KeyEventActions.Down)
            {
                switch (keyCode)
                {
                    case Keycode.Back:
                        if(_webView.CanGoBack())
                            _webView.GoBack();
                        else
                            Finish();
                        break;
                }
                return true;
            }
            return base.OnKeyDown(keyCode, e);
        }
    }
}