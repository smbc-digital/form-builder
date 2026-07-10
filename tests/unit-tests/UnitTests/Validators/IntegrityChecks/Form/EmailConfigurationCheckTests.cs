namespace form_builder_tests.UnitTests.Validators.IntegrityChecks.Form;

public class EmailConfigurationCheckTests
{
    private readonly Mock<IEmailConfigurationTransformDataProvider> _mockEmailConfigProvider = new();

    [Fact]
    public async Task EmailConfigurationCheck_IsNotValid_WhenNoConfigFound_ForForm()
    {
        // Arrange
        _mockEmailConfigProvider
            .Setup(_ => _.Get<List<EmailConfiguration>>())
            .ReturnsAsync(new List<EmailConfiguration>());

        var behaviour = new BehaviourBuilder()
            .WithBehaviourType(EBehaviourType.SubmitAndEmail)
            .Build();

        var page = new PageBuilder()
            .WithBehaviour(behaviour)
            .Build();

        var schema = new FormSchemaBuilder()
            .WithPage(page)
            .WithName("test-name")
            .Build();

        var check = new EmailConfigurationCheck(_mockEmailConfigProvider.Object);

        // Act
        var result = await check.ValidateAsync(schema);

        // Assert
        Assert.False(result.IsValid);
        Assert.Collection<string>(result.Messages, message => Assert.StartsWith(IntegrityChecksConstants.FAILURE, message));
    }

    [Fact]
    public async Task EmailConfigurationCheck_IsNotValid_WhenConfigFound_ForForm_ButRecipientIsNotSet()
    {
        // Arrange
        _mockEmailConfigProvider
            .Setup(_ => _.Get<List<EmailConfiguration>>())
            .ReturnsAsync(new List<EmailConfiguration>
            {
                new EmailConfiguration { FormName = new() {"test-form"}, Subject="subject" }
            });

        var behaviour = new BehaviourBuilder()
            .WithBehaviourType(EBehaviourType.SubmitAndEmail)
            .Build();

        var page = new PageBuilder()
            .WithBehaviour(behaviour)
            .Build();

        var schema = new FormSchemaBuilder()
            .WithPage(page)
            .WithName("test-form")
            .WithBaseUrl("test-name")
            .Build();

        var check = new EmailConfigurationCheck(_mockEmailConfigProvider.Object);

        // Act
        var result = await check.ValidateAsync(schema);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task EmailConfigurationCheck_IsNotValid_WhenConfigFound_ForForm_ButSubjectIsNotSet()
    {
        // Arrange
        _mockEmailConfigProvider
            .Setup(_ => _.Get<List<EmailConfiguration>>())
            .ReturnsAsync(new List<EmailConfiguration>
            {
                new EmailConfiguration { FormName = new() {"test-form"}, Recipient=new() { "recipient@email.com" } }
            });

        var behaviour = new BehaviourBuilder()
            .WithBehaviourType(EBehaviourType.SubmitAndEmail)
            .Build();

        var page = new PageBuilder()
            .WithBehaviour(behaviour)
            .Build();

        var schema = new FormSchemaBuilder()
            .WithPage(page)
            .WithName("test-form")
            .WithBaseUrl("test-name")
            .Build();

        var check = new EmailConfigurationCheck(_mockEmailConfigProvider.Object);

        // Act
        var result = await check.ValidateAsync(schema);

        // Assert
        Assert.False(result.IsValid);
    }
}