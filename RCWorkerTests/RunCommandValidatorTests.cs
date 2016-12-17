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
		public void ValidateTargetPlatform_OnException_ReturnsFalse()
		{
			var awsManagement = new Mock<IAmazonSimpleSystemsManagement>();

			awsManagement.Setup(a => a.DescribeDocument(It.IsAny<string>())).Throws(new Exception());

			var validator = new RunCommandValidator(awsManagement.Object);

			var command = new RunCommand { TargetPlatform = "Bar" };

			Assert.False(validator.ValidateTargetPlatform(command));
		}

		[Fact]
		public void ValidateTargetPlatform_InvalidTargetPlatform_ReturnsFalse()
		{
			var awsManagement = new Mock<IAmazonSimpleSystemsManagement>();

			awsManagement.Setup(a => a.DescribeDocument(It.IsAny<string>())).Returns(
				new DescribeDocumentResponse
				{
					Document = new DocumentDescription { PlatformTypes = new List<string> { "Foo" } }
				});

			var validator = new RunCommandValidator(awsManagement.Object);

			var command = new RunCommand { TargetPlatform = "Bar" };

			Assert.False(validator.ValidateTargetPlatform(command));
		}

		[Fact]
		public void ValidateTargetPlatform_ValidTargetPlatform_ReturnsTrue()
		{
			var awsManagement = new Mock<IAmazonSimpleSystemsManagement>();

			awsManagement.Setup(a => a.DescribeDocument(It.IsAny<string>())).Returns(
				new DescribeDocumentResponse
				{
					Document = new DocumentDescription { PlatformTypes = new List<string> { "foo", "bar" } }
				});

			var validator = new RunCommandValidator(awsManagement.Object);

			var command = new RunCommand { TargetPlatform = "Foo" };

			Assert.True(validator.ValidateTargetPlatform(command));
		}

		[Fact]
		public void ValidateInstances_EmptyInstanceIds_ReturnsFalse()
		{
			var awsManagement = new Mock<IAmazonSimpleSystemsManagement>();

			var validator = new RunCommandValidator(awsManagement.Object);

			var command = new RunCommand { InstanceIds = { } };

			Assert.False(validator.ValidateInstances(command));
		}

		[Fact]
		public void ValidateInstances_OnException_ReturnsFalse()
		{
			var awsManagement = new Mock<IAmazonSimpleSystemsManagement>();

			awsManagement.Setup(a => a.DescribeInstanceInformation(It.IsAny<DescribeInstanceInformationRequest>()))
				.Throws(new Exception());

			var validator = new RunCommandValidator(awsManagement.Object);

			var command = new RunCommand { InstanceIds = { "i-foo00001" } };

			Assert.False(validator.ValidateInstances(command));
		}

		[Fact]
		public void ValidateInstances_MissingInstance_ReturnsFalse()
		{
			var awsManagement = new Mock<IAmazonSimpleSystemsManagement>();

			awsManagement.Setup(a => a.DescribeInstanceInformation(It.IsAny<DescribeInstanceInformationRequest>()))
				.Returns(new DescribeInstanceInformationResponse
				{
					InstanceInformationList =
					new List<InstanceInformation> { new InstanceInformation { InstanceId = "i-foo12345" } }
				});

			var validator = new RunCommandValidator(awsManagement.Object);

			var command = new RunCommand { InstanceIds = { "i-foo00001" } };

			Assert.False(validator.ValidateInstances(command));
		}

		[Fact]
		public void ValidateInstances_DifferentPlatformInstance_ReturnsFalse()
		{
			var awsManagement = new Mock<IAmazonSimpleSystemsManagement>();

			awsManagement.Setup(a => a.DescribeInstanceInformation(It.IsAny<DescribeInstanceInformationRequest>()))
				.Returns(new DescribeInstanceInformationResponse
				{
					InstanceInformationList =
						new List<InstanceInformation> {
							new InstanceInformation { InstanceId = "i-foo12345", PlatformType = PlatformType.Windows } }
				});

			var validator = new RunCommandValidator(awsManagement.Object);

			var command = new RunCommand { InstanceIds = { "i-foo12345" }, TargetPlatform = "Foo" };

			Assert.False(validator.ValidateInstances(command));
		}

		[Fact]
		public void ValidateInstances_EqualPlatformInstance_ReturnsTrue()
		{
			var awsManagement = new Mock<IAmazonSimpleSystemsManagement>();

			awsManagement.Setup(a => a.DescribeInstanceInformation(It.IsAny<DescribeInstanceInformationRequest>()))
				.Returns(new DescribeInstanceInformationResponse
				{
					InstanceInformationList =
						new List<InstanceInformation> {
							new InstanceInformation { InstanceId = "i-foo12345", PlatformType = PlatformType.Windows } }
				});

			var validator = new RunCommandValidator(awsManagement.Object);

			var command = new RunCommand { InstanceIds = { "i-foo12345" }, TargetPlatform = "Windows" };

			Assert.True(validator.ValidateInstances(command));
		}
	}
}