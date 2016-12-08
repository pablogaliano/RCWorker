namespace JMFamily.Automation.RCWorker
{
	using StructureMap;

	public class RCWorkerRegistry : Registry
	{
		public RCWorkerRegistry()
		{
			Scan(x =>
			{
				x.TheCallingAssembly();
				x.WithDefaultConventions();
				x.AddAllTypesOf<IMessageReceiver>();
			});

			For<IConfigurationSettings>().Use("Setup", ctx => new ConfigurationSettings());
		}
	}
}