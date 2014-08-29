namespace GoToWindow.Plugins.ExpandBrowsersTabs.Contracts
{
	public abstract class TabBase
	{
		public string Title { get; set; }

	    protected TabBase(string title)
		{
			Title = title;
		}
	}
}
