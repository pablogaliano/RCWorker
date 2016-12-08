namespace RCWorkerTestTool
{
	using JMFamily.Messaging;
	using Newtonsoft.Json;
	using System;
	using System.Configuration;
	using System.Threading.Tasks;

	public class Program
	{
		static void Main(string[] args)
		{
			var hostName = ConfigurationManager.AppSettings["MessagingHost"];

			Console.WriteLine("Sending messages. To exit press CTRL+C");

			while (true)
			{
				using (var client = QueueFactory.GetQueueClient(hostName))
				{
					var message = JsonConvert.SerializeObject(new { Name = "Foo", Args = "Bar,Baz" });

					client.Send("runcommand_exchange", message, "runcommand.jmfamily.com");
					client.Send("runcommand_exchange", message, "foo.jmfamily.com");

					Console.WriteLine($"Message sent: {message}");
				}

				Task.Delay(5000).Wait();
			}
		}
	}
}