using SharpCompress.Reader.Zip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace AsyncAwaitDemonstration
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly Nito.AsyncEx.AsyncLock lock_ = new Nito.AsyncEx.AsyncLock();

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

#if WPFBUG
		private static Task DependencyObjectTaskRun(Action action)
		{
			// BUG: Resource leaked by worker thread using DependencyObject.
			//  http://grabacr.net/archives/1851

			var tcs = new TaskCompletionSource<object>();

			var thread = new Thread(() =>
			{
				try
				{
					action();

					tcs.SetResult(null);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
				}

				System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.SystemIdle);
				System.Windows.Threading.Dispatcher.Run();
			});

			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();

			return tcs.Task;
		}
#endif

		private Task ExtractAndShowImagesAsync(Stream stream)
		{
#if REACTIVE
			return this.ExtractImages(stream).
				ToObservable(ThreadPoolScheduler.Instance).
				ObserveOnDispatcher().
				ForEachAsync(image => this.ClaudiaImages.Add(image));
#elif WPFBUG
			return DependencyObjectTaskRun(() =>
				{
					foreach (var image in this.ExtractImages(stream))
					{
						this.Dispatcher.BeginInvoke(new Action(() => this.ClaudiaImages.Add(image)));
					}
				});
#else
			// BUG: Resource leaked by worker thread using DependencyObject.
			//  http://grabacr.net/archives/1851
			return Task.Run(() =>
				{
					foreach (var image in this.ExtractImages(stream))
					{
						this.Dispatcher.BeginInvoke(new Action(() => this.ClaudiaImages.Add(image)));
					}
				});
#endif
		}

#if NET45
		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			using (var l = await lock_.LockAsync())
			{
				using (var httpClient = new HttpClient())
				{
					using (var stream = await httpClient.GetStreamAsync(
						"http://download.microsoft.com/download/B/B/1/BB1F3160-9806-4021-97CC-CCBAC25EB5D4/Claudia_data1.zip"))
					{
						await this.ExtractAndShowImagesAsync(stream);
					}
				}
			}
		}
#else
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			lock_.LockAsync().
				ContinueWith(task0 =>
					{
						var httpClient = new HttpClient();
						httpClient.GetStreamAsync(
							"http://download.microsoft.com/download/B/B/1/BB1F3160-9806-4021-97CC-CCBAC25EB5D4/Claudia_data1.zip").
							ContinueWith(task1 =>
							{
								this.Dispatcher.BeginInvoke(new Action(() =>
								{
									this.ExtractAndShowImagesAsync(task1.Result).
										ContinueWith(task2 =>
										{
											task1.Result.Dispose();
											httpClient.Dispose();
											task0.Result.Dispose();
										});
								}));
							});
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
