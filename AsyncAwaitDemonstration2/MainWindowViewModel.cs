using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AsyncAwaitDemonstration2
{
	public sealed class MainWindowViewModel
	{
		public MainWindowViewModel()
		{
			this.Images = new ObservableCollection<ImageSource>();
			this.FireStart = new AsyncCommand(this.OnFireStartAsync);
		}

		public ObservableCollection<ImageSource> Images
		{
			get;
			set;
		}

		public IAsyncCommand FireStart
		{
			get;
			set;
		}

		private async Task OnFireStartAsync(object parameter)
		{
			var image = await this.FetchImageAsync();
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
