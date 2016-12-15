namespace JMFamily.Automation.RCWorker
{
	using Amazon.SimpleSystemsManagement;
	using Amazon.SimpleSystemsManagement.Model;
	using log4net;
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.Net;
	using System.Text;

	public class RunCommandValidator : IRunCommandValidator
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IAmazonSimpleSystemsManagement _awsManagement;

		public RunCommandValidator(IAmazonSimpleSystemsManagement awsManagement)
		{
			_awsManagement = awsManagement;
		}

		public bool Validate(RunCommand runCommand)
		{
			return 
				ValidateSSMDocument(runCommand) &&
				ValidateTargetPlatform(runCommand) &&
				ValidateInstances(runCommand);
		}

		private bool ValidateSSMDocument(RunCommand command)
		{
			var isValid = false;

			try
			{
				log.Info($"Validating SSM document");

				var response = _awsManagement.GetDocument(command.SSMDocument);

				isValid = response.HttpStatusCode == HttpStatusCode.OK;
			}
			catch (Exception)
			{
				isValid = false;
			}

			if (!isValid)
			{
				log.Error("The SSMDocument is invalid");
			}

			return isValid;
		}

		private bool ValidateTargetPlatform(RunCommand command)
		{
			var isValid = false;
			var documentPlatform = string.Empty;

			try
			{
				log.Info($"Validating command target platform");

				var response = _awsManagement.DescribeDocument(new DescribeDocumentRequest(command.SSMDocument));

				isValid = response.Document.PlatformTypes.Contains(command.TargetPlatform, StringComparer.OrdinalIgnoreCase);

				documentPlatform = string.Join("|", response.Document.PlatformTypes);
			}
			catch (Exception)
			{
				isValid = false;
			}

			if (!isValid)
			{
				var builder = new StringBuilder();

				builder.AppendLine("Command target platform and SSM document platform don't match");
				builder.AppendLine($"Command target platform: {command.TargetPlatform}");
				builder.AppendLine($"SSM Document target platform: {documentPlatform}");

				log.Error(builder.ToString());
			}

			return isValid;
		}

		private bool ValidateInstances(RunCommand command)
		{
			var isValid = true;
			var invalidInstances = new List<Tuple<string, string>>();

			try
			{
				log.Info($"Validating target instances");

				var response = _awsManagement.DescribeInstanceInformation(
					new DescribeInstanceInformationRequest
					{
						Filters = new List<InstanceInformationStringFilter> {
								new InstanceInformationStringFilter { Key="InstanceIds", Values = command.InstanceIds } }
					});

				foreach (var instanceInformation in response.InstanceInformationList)
				{
					if (!instanceInformation.PlatformType.Value.Equals(command.TargetPlatform, StringComparison.OrdinalIgnoreCase))
					{
						invalidInstances.Add(new Tuple<string, string>(instanceInformation.InstanceId, instanceInformation.PlatformType.Value));
						isValid = false;
					}
				}
			}
			catch (Exception)
			{
				isValid = false;
			}

			if (!isValid)
			{
				var builder = new StringBuilder();

				builder.AppendLine("Command target platform and instances's platform don't match");
				builder.AppendLine($"Command target platform: {command.TargetPlatform}");
				builder.Append($"Instances target platform: {string.Join("|", invalidInstances.Select(t => $"InstanceId: {t.Item1} - Platform: {t.Item2}"))}");

				log.Error(builder.ToString());
			}

			return isValid;
		}
	}
}