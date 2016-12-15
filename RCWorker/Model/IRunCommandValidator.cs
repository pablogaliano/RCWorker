namespace JMFamily.Automation.RCWorker
{
	public interface IRunCommandValidator
	{
		bool Validate(RunCommand runCommand);
	}
}