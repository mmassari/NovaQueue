namespace NovaQueue.Abstractions
{
	public class SettingsOptions
	{
		public string SectionName { get; set; }
		public string SettingsFile { get; set; }
		public bool ReloadAfterWrite { get; set; }
	}
}
