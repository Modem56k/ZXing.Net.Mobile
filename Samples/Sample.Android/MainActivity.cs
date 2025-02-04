using System.Collections.Generic;
using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.Widget;
using Android.OS;
using ZXing;
using ZXing.Mobile;
using System;

namespace Sample.Android
{
	[Activity(Label = "ZXing.Net.Mobile", MainLauncher = true, Theme = "@style/Theme.AppCompat.Light", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden)]
	public class Activity1 : AndroidX.AppCompat.App.AppCompatActivity
	{
		Button buttonScanCustomView;
		Button buttonScanDefaultView;
		Button buttonContinuousScan;
		Button buttonFragmentScanner;
		Button buttonGenerate;

		MobileBarcodeScanner scanner;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			Xamarin.Essentials.Platform.Init(Application);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			//Create a new instance of our Scanner
			scanner = new MobileBarcodeScanner();

			buttonScanDefaultView = this.FindViewById<Button>(Resource.Id.buttonScanDefaultView);
			buttonScanDefaultView.Click += async delegate
			{

				//Tell our scanner to use the default overlay
				scanner.UseCustomOverlay = false;

				//We can customize the top and bottom text of the default overlay
				scanner.TopText = "Hold the camera up to the barcode\nAbout 6 inches away";
				scanner.BottomText = "Wait for the barcode to automatically scan!";

				//Start scanning
				var result = await scanner.Scan( new MobileBarcodeScanningOptions 
				{ 
					ScanningArea = ScanningArea.From(0f, 0.49f, 1f, 0.51f)
				});

				HandleScanResult(result);
			};

			buttonContinuousScan = FindViewById<Button>(Resource.Id.buttonScanContinuous);
			buttonContinuousScan.Click += delegate
			{

				scanner.UseCustomOverlay = false;

				//We can customize the top and bottom text of the default overlay
				scanner.TopText = "Hold the camera up to the barcode\nAbout 6 inches away";
				scanner.BottomText = "Wait for the barcode to automatically scan!";

				var opt = new MobileBarcodeScanningOptions();
				opt.DelayBetweenContinuousScans = 3000;

				//Start scanning
				scanner.ScanContinuously(opt, HandleScanResult);
			};

			Button flashButton;
			View zxingOverlay;

			buttonScanCustomView = this.FindViewById<Button>(Resource.Id.buttonScanCustomView);
			buttonScanCustomView.Click += async delegate
			{

				//Tell our scanner we want to use a custom overlay instead of the default
				scanner.UseCustomOverlay = true;

				//Inflate our custom overlay from a resource layout
				zxingOverlay = LayoutInflater.FromContext(this).Inflate(Resource.Layout.ZxingOverlay, null);

				//Find the button from our resource layout and wire up the click event
				flashButton = zxingOverlay.FindViewById<Button>(Resource.Id.buttonZxingFlash);
				flashButton.Click += (sender, e) => scanner.ToggleTorch();

				//Set our custom overlay
				scanner.CustomOverlay = zxingOverlay;

				//Start scanning!
				var result = await scanner.Scan(new MobileBarcodeScanningOptions { AutoRotate = true });

				HandleScanResult(result);
			};

			buttonFragmentScanner = FindViewById<Button>(Resource.Id.buttonFragment);
			buttonFragmentScanner.Click += delegate
			{
				StartActivity(typeof(FragmentActivity));
			};

			buttonGenerate = FindViewById<Button>(Resource.Id.buttonGenerate);
			buttonGenerate.Click += delegate
			{
				StartActivity(typeof(ImageActivity));
			};
		}

		void HandleScanResult(ZXing.Result result)
		{
			var msg = "";

			if (result != null && !string.IsNullOrEmpty(result.Text))
				msg = "Found Barcode: " + result.Text;
			else
				msg = "Scanning Canceled!";

			RunOnUiThread(() => Toast.MakeText(this, msg, ToastLength.Short).Show());
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		[Java.Interop.Export("UITestBackdoorScan")]
		public Java.Lang.String UITestBackdoorScan(string param)
		{
			var expectedFormat = BarcodeFormat.QR_CODE;
			Enum.TryParse(param, out expectedFormat);
			var opts = new MobileBarcodeScanningOptions
			{
				PossibleFormats = new List<BarcodeFormat> { expectedFormat }
			};
			var barcodeScanner = new MobileBarcodeScanner();

			Console.WriteLine("Scanning " + expectedFormat);

			//Start scanning
			barcodeScanner.Scan(opts).ContinueWith(t =>
			{

				var result = t.Result;

				var format = result?.BarcodeFormat.ToString() ?? string.Empty;
				var value = result?.Text ?? string.Empty;

				RunOnUiThread(() =>
				{

					AlertDialog dialog = null;
					dialog = new AlertDialog.Builder(this)
									.SetTitle("Barcode Result")
									.SetMessage(format + "|" + value)
									.SetNeutralButton("OK", (sender, e) =>
									{
										dialog.Cancel();
									}).Create();
					dialog.Show();
				});
			});

			return new Java.Lang.String();
		}
	}
}


