namespace GoToWindow.Plugins.ExpandBrowsersTabs.Contracts
{
	public abstract class TabBase
	{
		public string Title { get; set; }

		public TabBase(string title)
		{
			Title = title;
		}
	}
}
