using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AsyncAwaitDemonstration2
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			this.Images = new ObservableCollection<ImageSource>();
			this.DataContext = this;
		}

		public ObservableCollection<ImageSource> Images
		{
			get;
			set;
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			var image = await FetchImageAsync();
			this.Images.Add(image);
		}

		private async Task<ImageSource> FetchImageAsync()
		{
			using (var httpClient = new HttpClient())
			{
				using (var stream = await httpClient.GetStreamAsync(
					"http://msdn.microsoft.com/ja-jp/hh561749.claudia_wp_01(l=ja-jp).jpg").
					ConfigureAwait(false))
				{
					var decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
					var imageSource = decoder.Frames[0];
					imageSource.Freeze();

					return imageSource;
				}
			}
		}
	}
}
