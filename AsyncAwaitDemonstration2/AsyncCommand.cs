using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AsyncAwaitDemonstration2
{
	public interface IAsyncCommand : ICommand
	{
		Task ExecuteAsync(object parameter);
	}

	public sealed class AsyncCommand : IAsyncCommand
	{
		private readonly Func<object, Task> asyncAction_;

		public AsyncCommand(Func<object, Task> asyncAction)
		{
			asyncAction_ = asyncAction;
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			var task = asyncAction_(parameter);
		}

		public Task ExecuteAsync(object parameter)
		{
			return asyncAction_(parameter);
		}
	}
}
