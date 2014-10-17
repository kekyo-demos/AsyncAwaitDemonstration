using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncAwaitDemonstration2.Tests
{
	[TestClass()]
	public class MainWindowViewModelTests
	{
		//[TestMethod()]
		//public void FireStartTest()
		//{
		//	var viewModel = new MainWindowViewModel();

		//	viewModel.FireStart.Execute(null);

		//	// Downloaded 1 image.
		//	Assert.AreEqual(1, viewModel.Images.Count);

		//	var image = viewModel.Images[0];

		//	// Downloaded image sizes are:
		//	Assert.AreEqual(1844, image.Width);
		//	Assert.AreEqual(1037, image.Height);
		//}

		[TestMethod()]
		public async Task FireStartTestAsync()
		{
			var viewModel = new MainWindowViewModel();

			await viewModel.FireStart.ExecuteAsync(null);

			// Downloaded 1 image.
			Assert.AreEqual(1, viewModel.Images.Count);

			var image = (BitmapSource)viewModel.Images[0];

			// Downloaded image sizes are:
			Assert.AreEqual(2134, image.PixelWidth);
			Assert.AreEqual(1200, image.PixelHeight);
		}
	}
}
