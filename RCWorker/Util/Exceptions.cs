namespace JMFamily.Automation.RCWorker
{
	using System;

	public static class Exceptions
	{
		public static void ThrowIfNull(object parameter, string name)
		{
			if (parameter == null)
			{
				throw new ArgumentNullException(name);
			}
		}

		public static void ThrowIfGuidEmpty(Guid parameter, string name)
		{
			if (parameter == Guid.Empty)
			{
				throw new ArgumentException("Expected non-empty Guid.", name);
			}
		}

		public static void ThrowIfEmpty(string value, string parameterName)
		{
			ThrowIfNull(value, parameterName);

			if (value.Length == 0)
			{
				throw new ArgumentException(
					"Expected non-empty string.",
					parameterName);
			}
		}

		public static void ThrowIfNullOrEmpty(string parameter, string name)
		{
			ThrowIfNull(parameter, name);
			ThrowIfEmpty(parameter, name);
		}
	}
}