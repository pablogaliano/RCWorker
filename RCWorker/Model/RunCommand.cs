namespace JMFamily.Automation.RCWorker
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Linq;

	[Serializable]
	public class RunCommand
	{
		public List<string> InstanceIds { get; set; }

		public string SSMDocument { get; set; }

		public Dictionary<string, List<string>> SSMDocumentParameters { get; set; }

		public string TargetPlatform { get; set; }

		public RunCommand()
		{
			InstanceIds = new List<string>();
			SSMDocumentParameters = new Dictionary<string, List<string>>();
		}

		public override string ToString()
		{
			var builder = new StringBuilder();

			builder.AppendLine($"InstanceIds: {string.Join("|", InstanceIds)}");
			builder.AppendLine($"SSMDocument: {SSMDocument}");

			var @params = string.Join("|", SSMDocumentParameters.Select(k => $"{k.Key} - {string.Join("|", k.Value)}"));
			builder.AppendLine($"SSMDocumentParameters: {@params}");

			builder.Append($"TargetPlatform: {TargetPlatform}");

			return builder.ToString();
		}
	}
}