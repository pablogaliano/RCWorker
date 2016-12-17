namespace JMFamily.Automation.RCWorker
{
	using Amazon.Runtime;
	using Amazon.SimpleSystemsManagement;
	using Messaging;
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

			For<IAmazonSimpleSystemsManagement>().Use("Setup", ctx =>
				{
					var configurationSettings = ctx.GetInstance<IConfigurationSettings>();
					var awsManagement = new AmazonSimpleSystemsManagementClient(
						 new StoredProfileAWSCredentials(configurationSettings.AWSProfileName), configurationSettings.AWSRegion) as IAmazonSimpleSystemsManagement;

					return awsManagement;
				});

			For<IRunCommandValidator>().Use("Setup", ctx => new RunCommandValidator(ctx.GetInstance<IAmazonSimpleSystemsManagement>()));

			For<IQueueServer>().Use("Setup", ctx => QueueFactory.GetQueueServer(ctx.GetInstance<IConfigurationSettings>().HostName));

		}
	}
}