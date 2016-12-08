namespace JMFamily.Automation.RCWorker
{
	using log4net;
	using Messaging;
	using System;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Amazon.SimpleSystemsManagement;
	using Amazon.SimpleSystemsManagement.Model;
	using System.Collections.Generic;
	using Amazon.Runtime;
	using Amazon;

	public class ExecuteCommandReceiver : IMessageReceiver
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private bool disposed = false;
		private IQueueServer _server;
		private IDisposable _receiver;

		private readonly IConfigurationSettings _configurationSettings;

		private readonly Lazy<AmazonSimpleSystemsManagementClient> _client;

		public ExecuteCommandReceiver(IConfigurationSettings configurationSettings)
		{
			_configurationSettings = configurationSettings;

			_client = new Lazy<AmazonSimpleSystemsManagementClient>(() => new AmazonSimpleSystemsManagementClient(
				new StoredProfileAWSCredentials(_configurationSettings.AWSProfileName), _configurationSettings.AWSRegion));
		}

		public void Receive()
		{
			_server = QueueFactory.GetQueueServer(_configurationSettings.HostName);

			_receiver = _server.ReceiveAsync<string>("runcommand_queue", "runcommand_exchange", new string[] { "runcommand.jmfamily.com" }, (e) =>
			   {
				   return Task.Run(() => ProcessMessage(e.Message));
			   });
		}

		private void ProcessMessage(string message)
		{
			var runCommand = JsonConvert.DeserializeObject<RunCommand>(message);

			log.InfoFormat($"Sending command: {runCommand.SSMDocument}");

			var commandRequest = new SendCommandRequest(runCommand.SSMDocument, new List<string> { runCommand.InstanceId });

			foreach (var @param in runCommand.SSMDocumentParameters)
			{
				commandRequest.Parameters.Add(@param.Key, @param.Value);
			}

			var response = _client.Value.SendCommand(commandRequest);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			if (disposing)
			{
				if (_server != null)
				{
					_server.Dispose();
				}

				if (_receiver != null)
				{
					_receiver.Dispose();
				}

				disposed = true;
			}
		}
	}
}