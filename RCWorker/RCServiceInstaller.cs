namespace JMFamily.Automation.RCWorker
{
	using System.ComponentModel;

	[RunInstaller(true)]
	public partial class RCServiceInstaller : System.Configuration.Install.Installer
	{
		public RCServiceInstaller()
		{
			InitializeComponent();
		}
	}
}
