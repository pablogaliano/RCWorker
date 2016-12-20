namespace JMFamily.Automation.RCWorker
{
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
		private IDisposable _receiver;

		private readonly IQueueServer _queueServer;
		private readonly IConfigurationSettings _configurationSettings;
		private readonly IRunCommandValidator _runCommandValidator;
		private readonly IAmazonSimpleSystemsManagement _awsManagement;

		public ExecuteCommandReceiver(IQueueServer queueServer, IConfigurationSettings configurationSettings, IAmazonSimpleSystemsManagement awsManagement, IRunCommandValidator runCommandValidator)
		{
			_queueServer = queueServer;
			_configurationSettings = configurationSettings;
			_awsManagement = awsManagement;
			_runCommandValidator = runCommandValidator;
		}

		public void Receive()
		{
			_receiver = _queueServer.ReceiveAsync<string>("runcommand_queue", "runcommand_exchange", new string[] { "runcommand.jmfamily.com" }, (e) =>
			   {
				   return Task.Run(() => ProcessMessage(e.Message));
			   });
		}

		internal void ProcessMessage(string message)
		{
			try
			{
				Exceptions.ThrowIfNullOrEmpty(message, nameof(message));

				var runCommand = JsonConvert.DeserializeObject<RunCommand>(message);

				if (!_runCommandValidator.Validate(runCommand))
				{
					return;
				}

				log.Info($"Sending command for execution: {Environment.NewLine}{runCommand.ToString()}");

				var commandRequest = new SendCommandRequest(runCommand.SSMDocument, runCommand.InstanceIds);

				foreach (var @param in runCommand.SSMDocumentParameters)
				{
					commandRequest.Parameters.Add(@param.Key, @param.Value);
				}

				var response = _awsManagement.SendCommand(commandRequest);

				log.Info($"Command sent for execution: {Environment.NewLine}CommandId: {response.Command.CommandId}");

				if (_configurationSettings.WaitForCommandExecution)
				{
					//Spin 500 msec to wait for AWS 
					Task.Delay(500).Wait();

					WaitForCommandExecution(runCommand.InstanceIds, response.Command.CommandId);
				}
			}
			catch (Exception ex)
			{
				log.Error($"Exception occurred: {ex.ToString()}");

				throw;
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
				if (_receiver != null)
				{
					_receiver.Dispose();
				}

				disposed = true;
			}
		}

		private void WaitForCommandExecution(List<string> instanceIds, string commandId)
		{
			foreach (var instanceId in instanceIds)
			{
				while (true)
				{
					try
					{
						var response = _awsManagement.GetCommandInvocation(
						new GetCommandInvocationRequest { InstanceId = instanceId, CommandId = commandId });

						if (response.Status == CommandInvocationStatus.Pending ||
							response.Status == CommandInvocationStatus.InProgress)
						{
							log.Info($"Waiting for command '{commandId}' execution on instance '{instanceId}'");

							Task.Delay(1000).Wait();
						}
						else
						{
							log.Info($"Target instance id: {instanceId}");
							log.Info($"Command response code: {response.ResponseCode}");
							log.Info($"Command start time: {response.ExecutionStartDateTime}");
							log.Info($"Command finish time: {response.ExecutionEndDateTime}");
							log.Info($"Command status: {response.StatusDetails}");
							log.Info($"Command output: {response.StandardOutputContent}");

							break;
						}
					}
					catch(Exception ex)
					{
						log.Error($"Exception occurred while waiting for execution: {ex.ToString()}");
					}
				}
			}
		}
	}
}