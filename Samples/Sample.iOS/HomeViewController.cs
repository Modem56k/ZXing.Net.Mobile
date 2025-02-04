using System;
using MonoTouch.Dialog;
using UIKit;
using ZXing;
using ZXing.Mobile;
using System.Collections.Generic;

namespace Sample.iOS
{
	public class HomeViewController : DialogViewController
	{
		public HomeViewController() : base(UITableViewStyle.Grouped, new RootElement("ZXing.Net.Mobile"), false)
		{
		}

		MobileBarcodeScanner scanner;
		CustomOverlayView customOverlay;

		public override void ViewDidLoad()
		{
			//Create a new instance of our scanner
			scanner = new MobileBarcodeScanner(this.NavigationController);

			Root = new RootElement("ZXing.Net.Mobile")
			{
				new Section {
					new StyledStringElement("Scan with Default View", async () =>
					{
						//Tell our scanner to use the default overlay
						scanner.UseCustomOverlay = false;
						//We can customize the top and bottom text of the default overlay
						scanner.TopText = "Hold camera up to barcode to scan";
						scanner.BottomText = "Barcode will automatically scan";

						//Start scanning
						var result = await scanner.Scan(new MobileBarcodeScanningOptions
						{
							ScanningArea = ScanningArea.From(0f, 0.3f, 1f, 0.7f)
						});

						HandleScanResult(result);
					}),
					new StyledStringElement("Scan with Default View using laser point", async () =>
					{
						//Tell our scanner to use the default overlay
						scanner.UseCustomOverlay = false;
						//We can customize the top and bottom text of the default overlay
						scanner.TopText = "Hold camera up to barcode to scan";
						scanner.BottomText = "Barcode will automatically scan";

						//Start scanning
						var result = await scanner.Scan(new MobileBarcodeScanningOptions
						{
							ScanningArea = ScanningArea.From(0f, 0.49f, 1f, 0.51f)
						});

						HandleScanResult(result);
					}),

					new StyledStringElement ("Scan Continuously", () => {
						//Tell our scanner to use the default overlay
						scanner.UseCustomOverlay = false;

						//Tell our scanner to use our custom overlay
						scanner.UseCustomOverlay = true;
						scanner.CustomOverlay = customOverlay;


						var opt = new MobileBarcodeScanningOptions ();
						opt.DelayBetweenContinuousScans = 3000;

						//Start scanning
						scanner.ScanContinuously (opt, false, HandleScanResult);
					}),

					new StyledStringElement ("Scan with Custom View", async () => {
						//Create an instance of our custom overlay
						customOverlay = new CustomOverlayView();
						//Wireup the buttons from our custom overlay
						customOverlay.ButtonTorch.TouchUpInside += delegate {
							scanner.ToggleTorch();
						};
						customOverlay.ButtonCancel.TouchUpInside += delegate {
							scanner.Cancel();
						};

						//Tell our scanner to use our custom overlay
						scanner.UseCustomOverlay = true;
						scanner.CustomOverlay = customOverlay;

						var result = await scanner.Scan (new MobileBarcodeScanningOptions { AutoRotate = true });

						HandleScanResult(result);
					}),

					new StyledStringElement ("Scan with AVCapture Engine", async () => {
						//Tell our scanner to use the default overlay
						scanner.UseCustomOverlay = false;
						//We can customize the top and bottom text of the default overlay
						scanner.TopText = "Hold camera up to barcode to scan";
						scanner.BottomText = "Barcode will automatically scan";

						//Start scanning
						var result = await scanner.Scan (true);

						HandleScanResult (result);
					}),

					new StyledStringElement ("Generate Barcode", () => {
						NavigationController.PushViewController (new ImageViewController (), true);
					})
				}
			};
		}

		void HandleScanResult(ZXing.Result result)
		{
			var msg = "";

			if (result != null && !string.IsNullOrEmpty(result.Text))
				msg = "Found Barcode: " + result.Text;
			else
				msg = "Scanning Canceled!";

			this.InvokeOnMainThread(() =>
			{
				var av = new UIAlertView("Barcode Result", msg, null, "OK", null);
				av.Show();
			});
		}

		public void UITestBackdoorScan(string param)
		{
			var expectedFormat = BarcodeFormat.QR_CODE;
			Enum.TryParse(param, out expectedFormat);
			var opts = new MobileBarcodeScanningOptions
			{
				PossibleFormats = new List<BarcodeFormat> { expectedFormat }
			};

			//Create a new instance of our scanner
			scanner = new MobileBarcodeScanner(this.NavigationController);
			scanner.UseCustomOverlay = false;

			Console.WriteLine("Scanning " + expectedFormat);

			//Start scanning
			scanner.Scan(opts).ContinueWith(t =>
			{
				var result = t.Result;

				var format = result?.BarcodeFormat.ToString() ?? string.Empty;
				var value = result?.Text ?? string.Empty;

				BeginInvokeOnMainThread(() =>
				{
					var av = UIAlertController.Create("Barcode Result", format + "|" + value, UIAlertControllerStyle.Alert);
					av.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Cancel, null));
					PresentViewController(av, true, null);
				});
			});
		}
	}
}