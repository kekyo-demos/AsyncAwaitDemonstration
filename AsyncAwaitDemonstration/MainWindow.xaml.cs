using SharpCompress.Reader.Zip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AsyncAwaitDemonstration
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			this.ClaudiaImages = new ObservableCollection<ClaudiaImage>();

			InitializeComponent();

			this.DataContext = this;
		}

		public ObservableCollection<ClaudiaImage> ClaudiaImages
		{
			get;
			private set;
		}

		private IEnumerable<ClaudiaImage> ExtractImages(Stream stream)
		{
			using (var zipReader = ZipReader.Open(stream))
			{
				while (zipReader.MoveToNextEntry() == true)
				{
					using (var imageStream = zipReader.OpenEntryStream())
					{
						var fileName = zipReader.Entry.Key;
						if (fileName.EndsWith(".jpg") == true)
						{
							var decoder = new JpegBitmapDecoder(imageStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
							var imageSource = decoder.Frames[0];
							imageSource.Freeze();

							yield return new ClaudiaImage
							{
								Title = fileName,
								Image = imageSource
							};
						}
						else
						{
							imageStream.SkipEntry();
						}
					}
				}
			}
		}

		private void ExtractAndShowImages(Stream stream)
		{
			foreach (var image in this.ExtractImages(stream))
			{
				this.ClaudiaImages.Add(image);
			}
		}

		private Task ExtractAndShowImagesAsync(Stream stream)
		{
			// BUG: Resource leaked by worker thread using DependencyObject.
			//  http://grabacr.net/archives/1851
			return Task.Run(() =>
				{
					foreach (var image in this.ExtractImages(stream))
					{
						this.Dispatcher.BeginInvoke(new Action(() => this.ClaudiaImages.Add(image)));
					}
				});
		}

#if !NET40
		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			using (var httpClient = new HttpClient())
			{
				using (var stream = await httpClient.GetStreamAsync(
					"http://download.microsoft.com/download/B/B/1/BB1F3160-9806-4021-97CC-CCBAC25EB5D4/Claudia_data1.zip"))
				{
					this.ExtractAndShowImages(stream);
				}
			}
		}
#else
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var httpClient = new HttpClient();

			httpClient.GetStreamAsync(
				"http://download.microsoft.com/download/B/B/1/BB1F3160-9806-4021-97CC-CCBAC25EB5D4/Claudia_data1.zip").
				ContinueWith(task1 =>
				{
					this.Dispatcher.BeginInvoke(new Action(() =>
						{
							try
							{
								using (var stream = task1.Result)
								{
									this.ExtractAndShowImages(stream);
								}
							}
							finally
							{
								httpClient.Dispose();
							}
						}));
				});
		}
#endif

		public sealed class ClaudiaImage
		{
			public string Title
			{
				get;
				set;
			}

			public ImageSource Image
			{
				get;
				set;
			}
		}
	}
}
