namespace JMFamily.Automation.RCWorker
{
	using Amazon;
	using System.Configuration;

	public interface IConfigurationSettings
	{
		string HostName { get; }
		string AWSProfileName { get; }
		RegionEndpoint AWSRegion { get; }
		bool WaitForCommandExecution { get; }
	}

	public class ConfigurationSettings : IConfigurationSettings
	{
		public string HostName => ConfigurationManager.AppSettings["MessagingHost"];
		public string AWSProfileName => ConfigurationManager.AppSettings["AWSProfileName"];
		public RegionEndpoint AWSRegion => RegionEndpoint.GetBySystemName(ConfigurationManager.AppSettings["AWSRegion"]);
		public bool WaitForCommandExecution => bool.Parse(ConfigurationManager.AppSettings["WaitForCommandExecution"]);
	}
}