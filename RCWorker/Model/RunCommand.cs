namespace JMFamily.Automation.RCWorker
{
	using System;
	using System.Collections.Generic;

	[Serializable]
	public class RunCommand
	{
		public string InstanceId { get; set; }

		public string SSMDocument { get; set; }

		public Dictionary<string, List<string>> SSMDocumentParameters { get; set; }

		public RunCommand()
		{
			SSMDocumentParameters = new Dictionary<string, List<string>>();
		}
	}
}