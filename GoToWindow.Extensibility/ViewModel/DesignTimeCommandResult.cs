using System.Windows.Controls;

namespace GoToWindow.Extensibility.ViewModel
{
	public class DesignTimeCommandResult : IBasicCommandResult
	{
	    public UserControl View => null;

		public string BeforeText { get; }
		public string Text { get; }
		public string AfterText { get; }

		public DesignTimeCommandResult()
		{
			BeforeText = "Execute command '";
			Text = "Input Text";
			AfterText = "' with something";
		}

		public void Select()
		{
		}

		public bool IsShown(string searchQuery)
		{
			return true;
		}
	}
}