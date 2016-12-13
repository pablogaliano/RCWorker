namespace JMFamily.Automation.RCWorker
{
	using Amazon.Runtime;
	using Amazon.SimpleSystemsManagement;
	using Amazon.SimpleSystemsManagement.Model;
	using log4net;
	using Messaging;
	using Newtonsoft.Json;
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

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

			log.InfoFormat($"Sending command for execution: {Environment.NewLine}{runCommand.ToString()}");

			var commandRequest = new SendCommandRequest(runCommand.SSMDocument, new List<string> { runCommand.InstanceId });

			foreach (var @param in runCommand.SSMDocumentParameters)
			{
				commandRequest.Parameters.Add(@param.Key, @param.Value);
			}

			var response = _client.Value.SendCommand(commandRequest);

			log.InfoFormat($"Command sent for execution: {Environment.NewLine}CommandId: {response.Command.CommandId}");

			if (_configurationSettings.WaitForCommandExecution)
			{
				WaitForCommandExecution(runCommand.InstanceId, response.Command.CommandId);
			}
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

		private void WaitForCommandExecution(string instanceId, string commandId)
		{
			while (true)
			{
				var response = _client.Value.GetCommandInvocation(
					new GetCommandInvocationRequest { InstanceId = instanceId, CommandId = commandId });

				if (response.Status == CommandInvocationStatus.Pending ||
					response.Status == CommandInvocationStatus.InProgress)
				{
					Task.Delay(1000).Wait();
				}
				else
				{
					log.InfoFormat($"Command response code: {response.ResponseCode}");
					log.InfoFormat($"Command status: {response.StatusDetails}");
					log.InfoFormat($"Command output: {response.StandardOutputContent}");

					return;
				}
			}
		}
	}
}