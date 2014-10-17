using System;
using System.Windows.Input;

namespace AsyncAwaitDemonstration2
{
	public sealed class Command : ICommand
	{
		private readonly Action<object> action_;

		public Command(Action<object> action)
		{
			action_ = action;
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			action_(parameter);
		}
	}
}
