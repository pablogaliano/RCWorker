namespace JMFamily.Automation.RCWorker
{
	using System;

	[Serializable]
	public class Command
	{
		public string Name { get; set; }

		public string Args { get; set; }

		public override string ToString()
		{
			return $"{Name}:{Args}";
		}
	}
}