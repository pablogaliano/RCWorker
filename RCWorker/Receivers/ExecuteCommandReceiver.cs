namespace JMFamily.Automation.RCWorker
{
	using log4net;
	using Messaging;
	using System;
	using System.Threading.Tasks;
	using Newtonsoft.Json;

	public class ExecuteCommandReceiver : IMessageReceiver
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private bool disposed = false;
		private IQueueServer _server;
		private IDisposable _receiver;

		private readonly IConfigurationSettings _configurationSettings;

		public ExecuteCommandReceiver(IConfigurationSettings configurationSettings)
		{
			_configurationSettings = configurationSettings;
		}

		public void Receive()
		{
			_server = QueueFactory.GetQueueServer(_configurationSettings.HostName);

			_receiver = _server.ReceiveAsync<string>("runcommand_queue", "runcommand_exchange", new string[] { "runcommand.jmfamily.com" }, (e) =>
			   {
				   return Task.Run(() => ProcessMessage(e.Message));
			   });
		}

		private static void ProcessMessage(string message)
		{
			var command = JsonConvert.DeserializeObject<Command>(message);

			log.InfoFormat($"ExecuteCommandReceiver: Received Message: {command.ToString()}");
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