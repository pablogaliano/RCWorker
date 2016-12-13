namespace JMFamily.Automation.RCWorker
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Linq;

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

		public override string ToString()
		{
			var builder = new StringBuilder();

			builder.AppendLine($"InstanceId: {InstanceId}");
			builder.AppendLine($"SSMDocument: {SSMDocument}");

			var @params = string.Join("|", SSMDocumentParameters.Select(k => $"{k.Key} - {string.Join("|", k.Value)}"));
			builder.Append($"SSMDocumentParameters: {@params}");

			return builder.ToString();
		}
	}
}