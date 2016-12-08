namespace JMFamily.Automation.RCWorker
{
	using log4net;
	using System;
	using System.Collections.Generic;

	public class Program
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static IEnumerable<IMessageReceiver> receivers;

		public static void Main(string[] args)
		{
			DependencyResolver.Current.ThrowAllResolveErrors = true;
			DependencyResolver.Current.AddRegistry<RCWorkerRegistry>();

			InitializeReceivers();

			if (!Environment.UserInteractive)
			{
				// Running as service
				using (var service = new RCService())
				{
					RCService.Run(service);
				}
			}
			else
			{
				Start(args);

				Console.WriteLine("Press any key to stop...");
				Console.ReadKey(true);

				Stop();
			}
		}

		public static void Start(string[] args)
		{
			log.Info("Starting Run Command Worker");

			foreach (var receiver in receivers)
			{
				receiver.Receive();
			}
		}

		public static void Stop()
		{
			log.Info("Stopping Run Command Worker");

			foreach (var receiver in receivers)
			{
				receiver.Dispose();
			}
		}

		private static void InitializeReceivers()
		{
			receivers = DependencyResolver.Current.GetServices<IMessageReceiver>();
		}
	}
}