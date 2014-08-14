namespace GoToWindow.Plugins.ExpandBrowsersTabs.Contracts
{
	public interface ITab
	{
		string Title { get; set; }
		void Select();
	}
}
