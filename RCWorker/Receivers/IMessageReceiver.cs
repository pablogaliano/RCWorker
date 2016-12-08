namespace JMFamily.Automation.RCWorker
{
	using System;

	public interface IMessageReceiver : IDisposable
	{
		void Receive();
	}
}