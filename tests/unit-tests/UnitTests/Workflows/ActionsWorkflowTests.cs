namespace form_builder_tests.UnitTests.Workflows;

public class ActionsWorkflowTests
{
    private readonly ActionsWorkflow _actionsWorkflow;
    private readonly Mock<IRetrieveExternalDataService> _mockRetrieveExternalDataService = new();
    private readonly Mock<IEmailService> _mockEmailService = new();
    private readonly Mock<ISchemaFactory> _mockSchemaFactory = new();
    private readonly Mock<IValidateService> _mockValidateService = new();
    private readonly Mock<ITemplatedEmailService> _mockTemplatedEmailService = new();

    public ActionsWorkflowTests()
    {
        _actionsWorkflow = new ActionsWorkflow(
            _mockRetrieveExternalDataService.Object,
            _mockEmailService.Object,
            _mockSchemaFactory.Object,
            _mockValidateService.Object,
            _mockTemplatedEmailService.Object);
    }

    [Fact]
    public async Task Process_ShouldCallService_IfRetrieveExternalDataActionExists()
    {
        // Arrange
        var page = new Page
        {
            PageActions = new List<IAction>
            {
                new RetrieveExternalData
                {
                    Type = EActionType.RetrieveExternalData
                }
            }
        };

        // Act
        await _actionsWorkflow.Process(page.PageActions, new FormSchema(), "form");

        // Assert
        _mockRetrieveExternalDataService.Verify(_ => _.Process(page.PageActions, It.IsAny<FormSchema>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Process_ShouldCallService_IfValidateActionExists()
    {
        // Arrange
        var page = new Page
        {
            PageActions = new List<IAction>
            {
                new Validate
                {
                    Type = EActionType.Validate
                }
            }
        };

        // Act
        await _actionsWorkflow.Process(page.PageActions, new FormSchema(), "form");

        // Assert
        _mockValidateService.Verify(_ => _.Process(page.PageActions, It.IsAny<FormSchema>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Process_ShouldCallTemplatedEmailService_IfTemplatedEmailActionExists()
    {
        // Arrange
        var page = new Page
        {
            PageActions = new List<IAction>
            {
                new RetrieveExternalData
                {
                    Type = EActionType.TemplatedEmail
                }
            }
        };

        // Act
        await _actionsWorkflow.Process(page.PageActions, new FormSchema(), "form");

        // Assert
        _mockTemplatedEmailService.Verify(_ => _.ProcessTemplatedEmail(page.PageActions, "form"), Times.Once);
    }
}