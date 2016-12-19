namespace RCWorkerTests
{
	using Amazon.SimpleSystemsManagement;
	using Amazon.SimpleSystemsManagement.Model;
	using JMFamily.Automation.RCWorker;
	using JMFamily.Messaging;
	using Moq;
	using Newtonsoft.Json;
	using System;
	using System.Collections.Generic;
	using Xunit;

	public class ExecuteCommandReceiverTests
	{
		[Fact]
		public void ProcessMessage_NullMessage_Throws()
		{
			var queueServer = new Mock<IQueueServer>();
			var configurationSettings = new Mock<IConfigurationSettings>();
			var awsManagement = new Mock<IAmazonSimpleSystemsManagement>();
			var runCommandValidator = new Mock<IRunCommandValidator>();

			var receiver = new ExecuteCommandReceiver(queueServer.Object, configurationSettings.Object, awsManagement.Object, runCommandValidator.Object);

			Assert.Throws<ArgumentNullException>(() => receiver.ProcessMessage(null));
		}

		[Fact]
		public void ProcessMessage_EmptyMessage_Throws()
		{
			var queueServer = new Mock<IQueueServer>();
			var configurationSettings = new Mock<IConfigurationSettings>();
			var awsManagement = new Mock<IAmazonSimpleSystemsManagement>();
			var runCommandValidator = new Mock<IRunCommandValidator>();

			var receiver = new ExecuteCommandReceiver(queueServer.Object, configurationSettings.Object, awsManagement.Object, runCommandValidator.Object);

			Assert.Throws<ArgumentException>(() => receiver.ProcessMessage(""));
		}

		[Fact]
		public void ProcessMessage_WrongMessage_Throws()
		{
			var queueServer = new Mock<IQueueServer>();
			var configurationSettings = new Mock<IConfigurationSettings>();
			var awsManagement = new Mock<IAmazonSimpleSystemsManagement>();
			var runCommandValidator = new Mock<IRunCommandValidator>();

			var receiver = new ExecuteCommandReceiver(queueServer.Object, configurationSettings.Object, awsManagement.Object, runCommandValidator.Object);

			Assert.Throws<JsonReaderException>(() => receiver.ProcessMessage("foo"));
		}

		[Fact]
		public void ProcessMessage_InvalidMessage_DontSendMessage()
		{
			var queueServer = new Mock<IQueueServer>();
			var configurationSettings = new Mock<IConfigurationSettings>();
			var awsManagement = new Mock<IAmazonSimpleSystemsManagement>();
			var runCommandValidator = new Mock<IRunCommandValidator>();

			runCommandValidator.Setup(r => r.Validate(It.IsAny<RunCommand>())).Returns(false);

			var receiver = new ExecuteCommandReceiver(queueServer.Object, configurationSettings.Object, awsManagement.Object, runCommandValidator.Object);

			var command = new RunCommand();

			receiver.ProcessMessage(JsonConvert.SerializeObject(command));

			awsManagement.Verify(a => a.SendCommand(It.IsAny<SendCommandRequest>()), Times.Never);
		}

		[Fact]
		public void ProcessMessage_InvalidMessage_SendsMessage()
		{
			var queueServer = new Mock<IQueueServer>();
			var configurationSettings = new Mock<IConfigurationSettings>();

			configurationSettings.SetupGet(c => c.WaitForCommandExecution).Returns(false);

			var awsManagement = new Mock<IAmazonSimpleSystemsManagement>();

			awsManagement.Setup(a => a.SendCommand(It.IsAny<SendCommandRequest>()))
				.Returns(new SendCommandResponse { Command = new Command { CommandId = Guid.NewGuid().ToString() } });

			var runCommandValidator = new Mock<IRunCommandValidator>();

			runCommandValidator.Setup(r => r.Validate(It.IsAny<RunCommand>())).Returns(true);

			var receiver = new ExecuteCommandReceiver(queueServer.Object, configurationSettings.Object, awsManagement.Object, runCommandValidator.Object);

			var command = new RunCommand { InstanceIds = new List<string> { "foo" }, SSMDocument = "bar", TargetPlatform = "baz" };

			receiver.ProcessMessage(JsonConvert.SerializeObject(command));

			awsManagement.Verify(a => a.SendCommand(It.IsAny<SendCommandRequest>()), Times.Once);
		}
	}
}