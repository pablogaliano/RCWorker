﻿namespace JMFamily.Automation.RCWorker
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
		private IQueueServer _server;
		private IDisposable _receiver;

		private readonly IConfigurationSettings _configurationSettings;
		private readonly IRunCommandValidator _runCommandValidator;
		private readonly IAmazonSimpleSystemsManagement _awsManagement;

		public ExecuteCommandReceiver(IConfigurationSettings configurationSettings, IAmazonSimpleSystemsManagement awsManagement, IRunCommandValidator runCommandValidator)
		{
			_configurationSettings = configurationSettings;
			_awsManagement = awsManagement;
			_runCommandValidator = runCommandValidator;
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
			try
			{
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

		private void WaitForCommandExecution(List<string> instanceIds, string commandId)
		{
			foreach (var instanceId in instanceIds)
			{
				while (true)
				{
					var response = _awsManagement.GetCommandInvocation(
						new GetCommandInvocationRequest { InstanceId = instanceId, CommandId = commandId });

					if (response.Status == CommandInvocationStatus.Pending ||
						response.Status == CommandInvocationStatus.InProgress)
					{
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
			}
		}
	}
}