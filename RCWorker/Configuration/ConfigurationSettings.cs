namespace JMFamily.Automation.RCWorker
{
	using System.Configuration;

	public interface IConfigurationSettings
	{
		string HostName { get; }
	}

	public class ConfigurationSettings : IConfigurationSettings
	{
		public string HostName => ConfigurationManager.AppSettings["MessagingHost"];
	}
}