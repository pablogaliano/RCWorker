namespace JMFamily.Automation.RCWorker
{
	using System.ServiceProcess;

	partial class RCService : ServiceBase
	{
		public RCService()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			Program.Start(args);
		}

		protected override void OnStop()
		{
			Program.Stop();
		}
	}
}