using System;
using System.Diagnostics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using System.Threading;
using Microsoft.Maui.Platform;
using System.Runtime.CompilerServices;
using System.Globalization;

/*
	factor in factors:
		pops = factor.pops

		pop in pops:
			center = pop.center
			
			
*/


namespace Gali;

public class Toggles {
	public bool PlatePoints { get; set; }
	public bool PlateLines { get; set; }
	public bool Activities { get; set; }
	public bool Populations { get; set; }
	public bool Factors { get; set; }

	public Toggles() { }

	public Toggles(bool platePoints, bool plateLines, bool activities, bool populations, bool factors) {
		PlatePoints = platePoints;
		PlateLines = plateLines;
		Activities = activities;
		Populations = populations;
		Factors = factors;
	}
}

public partial class MainPage : ContentPage {
    ScrollView MainView = new();
    AbsoluteLayout MainLayout = new();

	public static Rect WorldRect;

	public static double WidthScale  = 4;
	public static double HeightScale = 4;
	public static double FrameRate = 60;

	public static Color FullColor = Colors.Azure;
	public static Color NullColor = Colors.Black;
	public static int StartTimeRate = 750_000;


	Planet Planet;

	Thread MainThreadStart;
	ManualResetEvent MainPause = new ManualResetEvent(false);
	CancellationTokenSource MainCancelSrc = new CancellationTokenSource();

    public MainPage() {
		WorldRect = new Rect(175, 110, 360, 180);

		var seedrnd = new Random((int)DateTime.Now.Ticks);

		Planet = new Planet("Earth", seedrnd.Next(0, 100_000), this, 20, 5, 30, 12, (45, 60), WorldRect, WidthScale, HeightScale);
		
        InitializeComponent();

		var bgImg = new Image {
			Source = "wave_bg.jpg",
			Aspect = Aspect.Fill,
			ZIndex = -1
		};

		MainLayout.SetLayoutFlags(bgImg, AbsoluteLayoutFlags.All);
		MainLayout.SetLayoutBounds(bgImg, new Rect(0, 0, 1, 1));

		Content = MainView;
		MainView.Content = MainLayout;

		Planet.PlatePrecision = 4;
		Planet.TimeRate = StartTimeRate;

		var memlbl = new Label() {
			Text = "null",
			TextColor = FullColor,
			FontSize = 10,
			IsVisible = true,
			ZIndex = 2,
		};

		MainLayout.SetLayoutFlags(memlbl, AbsoluteLayoutFlags.PositionProportional);
		MainLayout.SetLayoutBounds(memlbl, new Rect(0.99, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

		var years = new Label {
			Text = "Year: 0",
			TextColor = FullColor,
			FontSize = 20,
			IsVisible = true,
			ZIndex = 2,
		};

		MainLayout.SetLayoutBounds(years, new Rect(500, WorldRect.Y + (WorldRect.Height * HeightScale) + 10, 1000, 100));

		#region View Buttons
		Button createBtn(string txt, EventHandler e) {
			var btn = new Button {
				Text = txt,
				FontSize = 16,
				 
				WidthRequest = 225,
				HeightRequest = 25,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Start,

				BackgroundColor = Colors.Azure,

				Background = Planet.GeneralGradientBrush,

				CornerRadius = 12,
				BorderWidth = 2
			};

			btn.Clicked += e;

			return btn;
		}

		var vT = new Toggles{ // ViewToggles
			PlatePoints = false,
			PlateLines = false,
			Activities = false,
			Populations = true,
			Factors = true,
		};

		var viewBtns = new Button[] {
			createBtn($"Plate Points {       (vT.PlatePoints  ? "(On)" : "(Off)")}", (s, e)  => { vT.PlatePoints  = !vT.PlatePoints;  var b = s as Button; b.Text = $"Plate Points {       (vT.PlatePoints ? "(On)" : "(Off)")}"; }),
			createBtn($"Plate Lines {        (vT.PlateLines   ? "(On)" : "(Off)")}", (s, e)  => { vT.PlateLines   = !vT.PlateLines;   var b = s as Button; b.Text = $"Plate Lines {        (vT.PlateLines  ? "(On)" : "(Off)")}"; }),
			createBtn($"Geologic Activities {(vT.Activities   ? "(On)" : "(Off)")}", (s, e)  => { vT.Activities   = !vT.Activities;   var b = s as Button; b.Text = $"Geologic Activities {(vT.Activities  ? "(On)" : "(Off)")}"; }),
			//createBtn($"Populations {        (vT.Populations  ? "(On)" : "(Off)")}", (s, e)  => { vT.Populations  = !vT.Populations;  var b = s as Button; b.Text = $"Populations {        (vT.Populations ? "(On)" : "(Off)")}"; }),
			createBtn($"Factors {            (vT.Factors      ? "(On)" : "(Off)")}", (s, e)  => { vT.Factors      = !vT.Factors;      var b = s as Button; b.Text = $"Factors {            (vT.Factors     ? "(On)" : "(Off)")}"; }),

		};

		var views = new HorizontalStackLayout() {
			Padding = 2,
		};

		foreach (var btn in viewBtns) {
			views.Children.Add(btn);
		}

		MainLayout.SetLayoutBounds(views, new Rect(0, 0, 2000, 100));

		#endregion

		#region Settings

		var timeRateEntry = new Entry {
			Placeholder = "Time Rate",
			Text = "",
			FontSize = 16,
			TextColor = FullColor,
			PlaceholderColor = NullColor,
			HeightRequest = 30,
			HorizontalOptions = LayoutOptions.Start,
			VerticalOptions = LayoutOptions.Center,
			//Background = Planet.GeneralGradientBrush,
		};
		MainLayout.SetLayoutBounds(timeRateEntry, new Rect(500, WorldRect.Y + (WorldRect.Height * HeightScale) + 35, 200, 50));

		var timeRateLbl = new Label {
			Text = Planet.TimeRate.ToString("N0"),
			TextColor = FullColor,
			FontSize = 20,
			IsVisible = true,
			ZIndex = 2,
		};
		MainLayout.SetLayoutBounds(timeRateLbl, new Rect(510, WorldRect.Y + (WorldRect.Height * HeightScale) + 90, 1000, 100));

		timeRateEntry.Completed += (s, e) => {
			var validRate = int.TryParse(timeRateEntry.Text, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var timeRate);

			if (validRate) {
				Planet.TimeRate = Math.Clamp(timeRate, 10_000, 5_000_000);
			}
			else {
				Planet.TimeRate = StartTimeRate;
			}

			timeRateLbl.Text = $"{Planet.TimeRate.ToString("N0")}";
		};

		var factorSToggles = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase) {
			{ "Temperature", false },
			{ "Depth", false },
			{ "Salinity", false },
			{ "Oxygen", false },
			{ "Calcium", false },
			{ "Nitrogen", false },
			{ "Acidity", false },
			{ "Populations", true },
			{ "Plant", true },
			{ "Bacteria", false },
			{ "Plankton", false },
			{ "Charnia", true },
			{ "JawlessFish", true },
			{ "JawedFish", true },
			{ "Cephalopod", true },
			{ "Eurypterid", false },
			{ "Radiodont", true },
			{ "Crustacean", true },
			{ "Arachnid", false },
			{ "JellyFish", false },
			{ "CombJelly", false },
			{ "Starfish", false },
			{ "Full", false }
		};

		var factorSettingsEntry = new Entry {
			Placeholder = "Add / Remove ",
			Text = "",
			FontSize = 16,
			TextColor = FullColor,
			PlaceholderColor = NullColor,
			HeightRequest = 30,
			HorizontalOptions = LayoutOptions.Start,
			VerticalOptions = LayoutOptions.Center,
		};
		MainLayout.SetLayoutBounds(factorSettingsEntry, new Rect(WorldRect.X + (WorldRect.Width * WidthScale) + 50, WorldRect.Y, 200, 50));

		var factorSettingsLbl = new Label {
			FormattedText = new(),

			FontSize = 20,
			IsVisible = true,
			ZIndex = 2,
		};
		MainLayout.SetLayoutBounds(factorSettingsLbl, new Rect(WorldRect.X + (WorldRect.Width * WidthScale) + 50, WorldRect.Y + 70, 1000, 1000));

		var visibleFactorsLbl = new Label {
			Text = "Visible Factors",

			FontSize = 24,
			IsVisible = true,
			ZIndex = 2,
		};
		MainLayout.SetLayoutBounds(visibleFactorsLbl, new Rect(WorldRect.X + (WorldRect.Width * WidthScale) + 50, WorldRect.Y - 35, 1000, 35));


		factorSettingsEntry.Completed += (s, e) => {
			var ent = s as Entry;

			var txt = ent.Text;

			if (factorSToggles.ContainsKey(txt)) {
				factorSToggles[txt] = !factorSToggles[txt];
			}

			if ( txt.ToLower() == "stats" || txt.ToLower() == "factors" || txt.ToLower() == "stat" || txt.ToLower() == "factor") {
				factorSToggles["Temperature"] = !factorSToggles["Temperature"];
				factorSToggles["Depth"] = !factorSToggles["Depth"];
				factorSToggles["Salinity"] = !factorSToggles["Salinity"];
				factorSToggles["Oxygen"] = !factorSToggles["Oxygen"];
				factorSToggles["Calcium"] = !factorSToggles["Calcium"];
				factorSToggles["Nitrogen"] = !factorSToggles["Nitrogen"];
				factorSToggles["Acidity"] = !factorSToggles["Acidity"];
			}

			if (txt.ToLower() == "temp") {
				factorSToggles["Temperature"] = !factorSToggles["Temperature"];
			}

			if (txt.ToLower() == "acid") {
				factorSToggles["Acidity"] = !factorSToggles["Acidity"];
			}

			if (txt.ToLower() == "salt" || txt.ToLower() == "salin") {
				factorSToggles["Salinity"] = !factorSToggles["Salinity"];
			}

			if (txt.ToLower() == "oxy") {
				factorSToggles["Oxygen"] = !factorSToggles["Oxygen"];
			}

			if (txt.ToLower() == "calc") {
				factorSToggles["Calcium"] = !factorSToggles["Calcium"];
			}

			if (txt.ToLower() == "dep") {
				factorSToggles["Depth"] = !factorSToggles["Depth"];
			}

			if (txt.ToLower() == "nitro") {
				factorSToggles["Nitrogen"] = !factorSToggles["Nitrogen"];
			}

			if (txt.ToLower() == "pop" || txt.ToLower() == "pops" || txt.ToLower() == "population") {
				factorSToggles["Populations"] = !factorSToggles["Populations"];
			}

			if (txt.ToLower() == "jawless" ) {
				factorSToggles["JawlessFish"] = !factorSToggles["JawlessFish"];
			}

			if (txt.ToLower() == "jawed" || txt.ToLower() == "jaw" || txt.ToLower() == "jaws") {
				factorSToggles["JawedFish"] = !factorSToggles["JawedFish"];
			}

			if (txt.ToLower() == "eur" || txt.ToLower() == "eurypt") {
				factorSToggles["Eurypterid"] = !factorSToggles["Eurypterid"];
			}

			if (txt.ToLower() == "rad" || txt.ToLower() == "radio") {
				factorSToggles["Radiodont"] = !factorSToggles["Radiodont"];
			}

			if (txt.ToLower() == "star") {
				factorSToggles["StarFish"] = !factorSToggles["StarFish"];
			}

			if (txt.ToLower() == "comb") {
				factorSToggles["CombJelly"] = !factorSToggles["CombJelly"];
			}

			if (txt.ToLower() == "bac" || txt.ToLower() == "bact") {
				factorSToggles["Bacteria"] = !factorSToggles["Bacteria"];
			}

			if (txt.ToLower() == "plank") {
				factorSToggles["Plankton"] = !factorSToggles["Plankton"];
			}

			if (txt.ToLower() == "charn") {
				factorSToggles["Charnia"] = !factorSToggles["Charnia"];
			}

			if (txt.ToLower() == "jelly") {
				factorSToggles["JellyFish"] = !factorSToggles["JellyFish"];
			}

			if (txt.ToLower() == "crust" || txt.ToLower() == "crab") {
				factorSToggles["Crustacean"] = !factorSToggles["Crustacean"];
			}

			if (txt.ToLower() == "arach" || txt.ToLower() == "spider") {
				factorSToggles["Arachnid"] = !factorSToggles["Arachnid"];
			}

			if (txt.ToLower() == "ceph" || txt.ToLower() == "cephalo") {
				factorSToggles["Cephalopod"] = !factorSToggles["Cephalopod"];
			}

			if (txt.ToLower() == "flora" || txt.ToLower() == "producers" || txt.ToLower() == "producer") {
				factorSToggles["Plant"] = !factorSToggles["Plant"];
				factorSToggles["Bacteria"] = !factorSToggles["Bacteria"];
				factorSToggles["Plankton"] = !factorSToggles["Plankton"];
				factorSToggles["Charnia"] = !factorSToggles["Charnia"];
			}

			if (txt.ToLower() == "fauna" || txt.ToLower() == "animals" || txt.ToLower() == "animal") {
				factorSToggles["JawlessFish"] = !factorSToggles["JawlessFish"];
				factorSToggles["JawedFish"] = !factorSToggles["JawedFish"];
				factorSToggles["Cephalopod"] = !factorSToggles["Cephalopod"];
				factorSToggles["Eurypterid"] = !factorSToggles["Eurypterid"];
				factorSToggles["Radiodont"] = !factorSToggles["Radiodont"];
				factorSToggles["Crustacean"] = !factorSToggles["Crustacean"];
				factorSToggles["Arachnid"] = !factorSToggles["Arachnid"];
				factorSToggles["JellyFish"] = !factorSToggles["JellyFish"];
				factorSToggles["CombJelly"] = !factorSToggles["CombJelly"];
				factorSToggles["Starfish"] = !factorSToggles["Starfish"];
			}
			
			if (txt.ToLower() == "ful") {
				factorSToggles["Full"] = !factorSToggles["Full"];
			}


			factorSettingsLbl.FormattedText.Spans.Clear();

			foreach (var pair in factorSToggles) {
				factorSettingsLbl.FormattedText.Spans.Add(new Span { Text = pair.Key + "\n", TextColor = pair.Value ? FullColor : NullColor });
			}
		};

		#endregion

		#region Controls

		var paused = true;

		var controlsBorder = new HorizontalStackLayout {
			Padding = new Thickness(20),

			Background = new SolidColorBrush(Colors.Transparent),
		};

		MainLayout.SetLayoutBounds(controlsBorder, new Rect(175, WorldRect.Y + (WorldRect.Height * HeightScale) + 10, 400, 100));

		var r = 55;

		var stepBtn = new Button() {
			Text = "▶️",
			FontSize = 20,

			WidthRequest = r,
			HeightRequest = r,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Start,

			BackgroundColor = Colors.Wheat,

			Background = SolidColorBrush.Transparent,

			CornerRadius = 12,
			BorderWidth = 2
		};
		stepBtn.Clicked += (s, e) => {
			if (paused) {
				MainPause.Set();

				Task.Run(() => {
					MainPause.Reset();
				});
			}
		};

		var playBtn = new Button() {
			Text = "▶️▶️",
			FontSize = 18,

			WidthRequest = r,
			HeightRequest = r,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Start,

			BackgroundColor = Colors.Wheat,

			Background = SolidColorBrush.Transparent,

			CornerRadius = 12,
			BorderWidth = 2,

		};
		playBtn.Clicked += (s, e) => {
			if (paused) {
				paused = false;

				MainPause.Set();
			}
		};

		var pauseBtn = new Button() {
			Text = "▏▏ ▏▏",
			FontSize = 20,

			WidthRequest = r,
			HeightRequest = r,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Start,

			BackgroundColor = Colors.Wheat,

			Background = SolidColorBrush.Transparent,

			ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Top, 40),

			CornerRadius = 12,
			BorderWidth = 2
		};
		pauseBtn.Clicked += (s, e) => {
			if (!paused) {
				paused = true;

				MainPause.Reset();
			}
		};

		var refreshBtn = new Button() {
			Text = "↺",
			FontSize = 30,

			WidthRequest = r,
			HeightRequest = r,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Start,

			BackgroundColor = Colors.Wheat,

			Background = SolidColorBrush.Transparent,

			ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Top, 40),

			CornerRadius = 12,
			BorderWidth = 2
		};
		refreshBtn.Clicked += (s, e) => {
			drawAll(true);
		};

		controlsBorder.Children.Add(stepBtn);
		controlsBorder.Children.Add(playBtn);
		controlsBorder.Children.Add(pauseBtn);
		controlsBorder.Children.Add(refreshBtn);
		#endregion

		#region Main Thread

		void drawAll(bool show = true) {
			if (show) {
				//throw new Exception($"what");
				try {
					Planet.Show(MainLayout, WorldRect, (WidthScale, HeightScale), vT, factorSToggles);
					Planet.Step(1, MainLayout, WorldRect, (WidthScale, HeightScale), vT, factorSToggles, true);
				}
				catch (Exception) { }
			}

			/* Text changes */

			var memusage = Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024;

			memlbl.Text = $"{memusage}MB";

			/* Adding */

			factorSettingsLbl.FormattedText.Spans.Clear();
			
			foreach (var pair in factorSToggles) {
				factorSettingsLbl.FormattedText.Spans.Add(new Span { Text = pair.Key + "\n", TextColor = pair.Value ? FullColor : NullColor });
			}

			MainLayout.Children.Add(bgImg);
			MainLayout.Children.Add(memlbl);
			MainLayout.Children.Add(years);
			MainLayout.Children.Add(timeRateEntry);
			MainLayout.Children.Add(timeRateLbl);
			MainLayout.Children.Add(views);
			MainLayout.Children.Add(factorSettingsEntry);
			MainLayout.Children.Add(factorSettingsLbl);
			MainLayout.Children.Add(visibleFactorsLbl);
			MainLayout.Children.Add(controlsBorder);
		}

		void mainThread(object obj) {
			var cancelTkn = (CancellationToken)obj;

			while (true) {
				if (cancelTkn.IsCancellationRequested)
					break;

				MainThread.BeginInvokeOnMainThread(() => {
					try {
						Planet.Step(1, MainLayout, WorldRect, (WidthScale, HeightScale), vT, factorSToggles);
						years.Text = $"Year: {Planet.Year.ToString("N0")}";
					} catch (Exception) {}

					drawAll(false);
				});


				Thread.Sleep(paused ? 100 : 1500);

				MainPause.WaitOne();
			}
		}

		var mTS = new Thread(new ParameterizedThreadStart(mainThread));

		mTS.Start(MainCancelSrc.Token);

		MainThreadStart = mTS;
		#endregion
	}

	protected override void OnDisappearing() {
		base.OnDisappearing();

		MainPause.Set(); 
		MainCancelSrc.Cancel();

		Task.Run(() => {
			if (MainThreadStart.IsAlive) {
				MainThreadStart.Join();
			}
		});
	}
}