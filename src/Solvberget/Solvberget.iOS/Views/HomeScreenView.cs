using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Touch.Views;
using SlidingPanels.Lib;
using SlidingPanels.Lib.PanelContainers;
using Solvberget.Core.ViewModels.Base;
using Solvberget.Core.ViewModels;
using System.Linq;
using MonoTouch.CoreGraphics;
using System.Diagnostics;

namespace Solvberget.iOS
{


	public partial class HomeScreenView : NamedViewController
    {
        public HomeScreenView() : base("HomeScreenView", null)
		{
			NavigationItem.LeftBarButtonItem = CreateSliderButton("/Images/logo.white.png", PanelType.LeftPanel);
        }

		public new HomeScreenViewModel ViewModel { get { return base.ViewModel as HomeScreenViewModel; }}

		private UIBarButtonItem CreateSliderButton(string imageName, PanelType panelType)
		{
			var button = new UIBarButtonItem(UIImage.FromBundle(imageName).Scale(new SizeF(20,20)), UIBarButtonItemStyle.Plain, 

				(s,e) => {
				SlidingPanelsNavigationViewController navController = NavigationController as SlidingPanelsNavigationViewController;
				navController.TogglePanel(panelType);
				});

			return button;
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);

			if(UIHelpers.MinVersion7) return;

			NavigationItem.Title = "Hjem";
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);

			CreateMenu();
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);

			CreateMenu();
		}

		private void CreateMenu(bool animate = true)
		{
			foreach(var v in ScrollView.Subviews) v.RemoveFromSuperview();

			UIView myPageBox;

			if (View.Bounds.Width > 700)
			{
				var centeredBox = new UIView(new RectangleF(0, 0, 640, 320));
				ScrollView.Add(centeredBox);
				ScrollView.ContentSize = ScrollView.Frame.Size;

				centeredBox.Center = new PointF(ScrollView.Bounds.Width / 2, ScrollView.Bounds.Height / 2);
				centeredBox.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;

				// tablet layout

				centeredBox.Add(myPageBox = CreateBox(0f, 0f, 200f, 200f, "m"));
				centeredBox.Add(CreateBox(220f, 0f, 200f, 200f, "s"));
				centeredBox.Add(CreateBox(440f, 0f, 200f, 200f, "h"));

				centeredBox.Add(CreateBox(0f, 220f, 200f, 100f, "a"));

				centeredBox.Add(CreateBox(220f, 220f, 90f, 100f, "e"));
				centeredBox.Add(CreateBox(330f, 220f, 90f, 100f, "n"));

				centeredBox.Add(CreateBox(440f, 220f, 90f, 100f, "å"));
				centeredBox.Add(CreateBox(550f, 220f, 90f, 100f, "c"));


			}
			else if (View.Bounds.Width > 320) // phone landscape
			{
				var extraPadding = UIScreen.MainScreen.Bounds.Height > 480 ? 15f : 0f;

				// phone layout
				var centeredBox = new UIView(new RectangleF(0, 0, 390+extraPadding*3, 190+extraPadding));
				ScrollView.Add(centeredBox);

				centeredBox.Center = new PointF(ScrollView.Bounds.Width / 2, ScrollView.Bounds.Height / 2);
				centeredBox.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;

				centeredBox.Add(myPageBox = CreateBox(0f, 0f, 90f, 90f, "m"));
				centeredBox.Add(CreateBox(100f+extraPadding, 0f, 90f, 90f, "s"));
				centeredBox.Add(CreateBox(200f+extraPadding*2, 0f, 90f, 90f, "h"));
				centeredBox.Add(CreateBox(300f+extraPadding*3, 0f, 90f, 90f, "a"));

				centeredBox.Add(CreateBox(0f, 100f+extraPadding, 90f, 90f, "e"));
				centeredBox.Add(CreateBox(100f+extraPadding, 100f+extraPadding, 90f, 90f, "n"));
				centeredBox.Add(CreateBox(200f+extraPadding*2, 100f+extraPadding, 90f, 90f, "å"));
				centeredBox.Add(CreateBox(300f+extraPadding*3, 100f+extraPadding, 90f, 90f, "c"));
			}
			else // phone portrait
			{
				var extraPadding = UIScreen.MainScreen.Bounds.Height > 480 ? 15f : 0f;

				// phone layout
				var centeredBox = new UIView(new RectangleF(0, 0, 190 + extraPadding, 390 + extraPadding*3));
				ScrollView.Add(centeredBox);

				centeredBox.Center = new PointF(ScrollView.Bounds.Width / 2, ScrollView.Bounds.Height / 2);
				centeredBox.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;

				centeredBox.Add(myPageBox = CreateBox(0f, 0f, 90f, 90f, "m"));
				centeredBox.Add(CreateBox(100f + extraPadding, 0f, 90f, 90f, "s"));

				centeredBox.Add(CreateBox(0f, 100f+extraPadding, 90f, 90f, "h"));
				centeredBox.Add(CreateBox(100f + extraPadding, 100f + extraPadding, 90f, 90f, "a"));

				centeredBox.Add(CreateBox(0f, 200f+extraPadding*2, 90f, 90f, "e"));
				centeredBox.Add(CreateBox(100f + extraPadding, 200f + extraPadding*2, 90f, 90f, "n"));

				centeredBox.Add(CreateBox(0f, 300f+extraPadding*3, 90f, 90f, "å"));
				centeredBox.Add(CreateBox(100f + extraPadding, 300f + extraPadding*3, 90f, 90f, "c"));
			}

			ScrollView.ContentSize = new SizeF(
				ScrollView.Subviews.Max(s => s.Frame.Right), 
				ScrollView.Subviews.Max(s => s.Frame.Bottom + 20f));


			if (!animate)
				return;

			foreach (var v in ScrollView.Subviews)
			{
				v.Alpha = 0.0f;
				v.Transform = CGAffineTransform.MakeScale(1.1f, 1.1f);
			}

			foreach(UIView view in ScrollView.Subviews)
			{
				UIView.Animate (0.3f, 0f, UIViewAnimationOptions.CurveEaseOut,
					() => {
						view.Alpha = 1.0f;
						view.Transform = CGAffineTransform.MakeScale(1f,1f);

					}, null);
			}

			ViewModel.WaitForReady(() =>
			{
				if(String.IsNullOrEmpty(ViewModel.MyPageBadgeText)) return;

				InvokeOnMainThread(() => 
				{
					AddBadge(myPageBox, ViewModel.MyPageBadgeText);
				});
			});
		}

		UIView CreateBox(float x, float y, float width, float height, string itemChar)
		{
			var item = ViewModel.ListElements.First(l => l.IconChar == itemChar);
			UIView view = new UIView(new RectangleF(0,0, width, height));
			view.Center = new PointF(x + width / 2, y + height / 2);

			view.AddGestureRecognizer(new BoxGestureRecognizer(view));
			view.AddGestureRecognizer(new UITapGestureRecognizer(() => item.GoToCommand.Execute(null)));

			view.BackgroundColor = ColorFor(item);

			UILabel title = new UILabel();
			title.TextColor = UIColor.White;
			title.BackgroundColor = UIColor.Clear;

			if (height < 120) title.Font = Application.ThemeColors.DefaultFont.WithSize(12f);
			else if (height < 200) title.Font = Application.ThemeColors.DefaultFont;
			else title.Font = Application.ThemeColors.TitleFont;

			title.Text = item.Title;

			var size = title.SizeThatFits(new SizeF(width, 0));
			var titleX = 5;
			var titleY = height-size.Height-5f;

			title.Frame = new RectangleF(new PointF(titleX,titleY), size);

			view.Add(title);

			var icon = new UILabel();
			icon.BackgroundColor = UIColor.Clear;
			icon.Font = Application.ThemeColors.IconFont.WithSize(width / 3);


			icon.TextColor = UIColor.White;
			icon.TextAlignment = UITextAlignment.Center;
			icon.LineBreakMode = UILineBreakMode.CharacterWrap;
			icon.Frame = new RectangleF(0,0, height - 40, height - 40);
			icon.Center = new PointF(width / 2, height / 2);
			icon.Text = item.IconChar;

			view.Add(icon);

			return view;

		}

		UILabel AddBadge(UIView box, string text)
		{
			var badgeSize = box.Frame.Width > 100 ? 25f : 18f;

			UILabel badge = new UILabel();
			badge.Font = Application.ThemeColors.DefaultFontBold.WithSize(badgeSize - 6f);
			badge.TextColor = Application.ThemeColors.MainInverse;

			badge.BackgroundColor = Application.ThemeColors.Main2.ColorWithAlpha(0.35f);
			badge.Text = text;
			badge.TextAlignment = UITextAlignment.Center;
			badge.Frame = new RectangleF(new PointF(
				(box.Frame.Width - badgeSize), 0f), new SizeF(badgeSize,badgeSize));

			box.Add(badge);

			return badge;
		}

		public class BoxGestureRecognizer : UIGestureRecognizer
		{
			UIView _box;

			public BoxGestureRecognizer(UIView box)
			{
				_box = box;
			}

			public override void TouchesBegan(NSSet touches, UIEvent evt)
			{
				base.TouchesBegan(touches, evt);

				UIView.BeginAnimations(null);
				UIView.SetAnimationDuration(0.2f);
				_box.Transform = CGAffineTransform.MakeScale(.9f,.9f);
				UIView.CommitAnimations();

			}

			public override void TouchesCancelled(NSSet touches, UIEvent evt)
			{
				base.TouchesCancelled(touches, evt);
				EndAnimate();
			}

			public override void TouchesEnded(NSSet touches, UIEvent evt)
			{
				base.TouchesEnded(touches, evt);
				EndAnimate();
			}

			private void EndAnimate()
			{
				UIView.BeginAnimations(null);
				UIView.SetAnimationDuration(0.2f);
				_box.Transform = CGAffineTransform.MakeScale(1f,1f);
				UIView.CommitAnimations();
			}

		}

		UIColor ColorFor(HomeScreenElementViewModel item)
		{
			switch (item.IconChar)
			{
				case "m": // min side
					return UIColor.FromRGB(0xFF, 0x99, 0x00);
				case "a": // arrangementer
					return UIColor.FromRGB(0xCC, 0x33, 0x00);
				case "s": // søk
					return UIColor.FromRGB(0x00, 0x55, 0x22);
				case "e": // blogger
					return UIColor.FromRGB(0x88, 0xBB, 0x00);
				case "n": // nyheter
					return UIColor.FromRGB(0x88, 0xBB, 0x00);
				case "h": // anbefalinger
					return UIColor.FromRGB(0x00, 0x33, 0x66);
				case "å": // åpningstider
					return UIColor.FromRGB(0x00, 0x99, 0xCC);
				case "c": // kontakt oss
					return UIColor.FromRGB(0x00, 0x99, 0xCC);
				default : 
					return UIColor.FromRGB(0xCC, 0x33, 0x00);

			}
		}
    }
}

