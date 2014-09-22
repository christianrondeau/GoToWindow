using System.Windows.Controls;

namespace GoToWindow.Extensibility.ViewModel
{
	public class DesignTimeCommandResult : IBasicCommandResult
	{
	    public UserControl View { get { return null; } }

	    public string BeforeText { get; private set; }
		public string Text { get; private set; }
		public string AfterText { get; private set; }

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