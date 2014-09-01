namespace GoToWindow.Extensibility.ViewModel
{
	public interface IBasicCommandResult : ISearchResult
	{
		string BeforeText { get; }
		string Text { get; }
		string AfterText { get; }
	}
}