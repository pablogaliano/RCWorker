namespace RCWorkerTests
{
	using Amazon.SimpleSystemsManagement;
	using Amazon.SimpleSystemsManagement.Model;
	using JMFamily.Automation.RCWorker;
	using Moq;
	using System;
	using System.Collections.Generic;
	using System.Net;
	using Xunit;

	public class RunCommandValidatorTests
	{
		[Fact]
		public void ValidateSSMDocument_NullSSMDocument_ReturnsFalse()
		{
			var awsManagement = new Mock<IAmazonSimpleSystemsManagement>();

			var validator = new RunCommandValidator(awsManagement.Object);

			var command = new RunCommand { SSMDocument = null };

			Assert.False(validator.ValidateSSMDocument(command));
		}

		[Fact]
		public void ValidateSSMDocument_EmptySSMDocument_ReturnsFalse()
		{
			var awsManagement = new Mock<IAmazonSimpleSystemsManagement>();

			var validator = new RunCommandValidator(awsManagement.Object);

			var command = new RunCommand { SSMDocument = "" };

			Assert.False(validator.ValidateSSMDocument(command));
		}

		[Fact]
		public void ValidateSSMDocument_OnException_ReturnsFalse()
		{
			var awsManagement = new Mock<IAmazonSimpleSystemsManagement>();

			awsManagement.Setup(a => a.GetDocument(It.IsAny<string>())).Throws(new Exception());

			var validator = new RunCommandValidator(awsManagement.Object);

			var command = new RunCommand { SSMDocument = "Bar" };

			Assert.False(validator.ValidateSSMDocument(command));
		}

		[Fact]
		public void ValidateSSMDocument_InvalidSSMDocument_ReturnsTrue()
		{
			var awsManagement = new Mock<IAmazonSimpleSystemsManagement>();

			awsManagement.Setup(a => a.GetDocument(It.IsAny<string>())).Returns(new GetDocumentResponse { HttpStatusCode = HttpStatusCode.NotFound });

			var validator = new RunCommandValidator(awsManagement.Object);

			var command = new RunCommand { SSMDocument = "Bar" };

			Assert.False(validator.ValidateSSMDocument(command));
		}

		[Fact]
		public void ValidateSSMDocument_ValidSSMDocument_ReturnsTrue()
		{
			var awsManagement = new Mock<IAmazonSimpleSystemsManagement>();

			awsManagement.Setup(a => a.GetDocument(It.IsAny<string>())).Returns(new GetDocumentResponse { HttpStatusCode = HttpStatusCode.OK });

			var validator = new RunCommandValidator(awsManagement.Object);

			var command = new RunCommand { SSMDocument = "Bar" };

			Assert.True(validator.ValidateSSMDocument(command));
		}

		[Fact]
		public void ValidateTargetPlatform_NullTargetPlatform_ReturnsFalse()
		{
			var awsManagement = new Mock<IAmazonSimpleSystemsManagement>();

			var validator = new RunCommandValidator(awsManagement.Object);

			var command = new RunCommand { TargetPlatform = null };

			Assert.False(validator.ValidateTargetPlatform(command));
		}

		[Fact]
		public void ValidateTargetPlatform_EmptyTargetPlatform_ReturnsFalse()
		{
			var awsManagement = new Mock<IAmazonSimpleSystemsManagement>();

			var validator = new RunCommandValidator(awsManagement.Object);

			var command = new RunCommand { TargetPlatform = "" };

			Assert.False(validator.ValidateTargetPlatform(command));
		}

		[Fact]
		public void ValidateInstances_EmptyInstanceIds_ReturnsFalse()
		{
			var awsManagement = new Mock<IAmazonSimpleSystemsManagement>();

			var validator = new RunCommandValidator(awsManagement.Object);

			var command = new RunCommand { InstanceIds = { } };

			Assert.False(validator.ValidateInstances(command));
		}
	}
}