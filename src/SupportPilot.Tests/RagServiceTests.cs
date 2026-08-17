using Microsoft.Extensions.Logging;
using Moq;
using SupportPilot.Application.DTOs;
using SupportPilot.Application.Interfaces;
using SupportPilot.Application.Services;
using SupportPilot.Domain.Common;
using Xunit;

namespace SupportPilot.Tests;

public class RagServiceTests
{
    private readonly Mock<IEmbeddingService> _embeddingServiceMock = new();
    private readonly Mock<IVectorStore> _vectorStoreMock = new();
    private readonly Mock<IChatProvider> _chatProviderMock = new();
    private readonly Mock<IRagOptions> _optionsMock = new();
    private readonly Mock<ILogger<RagService>> _loggerMock = new();

    private RagService CreateSut()
    {
        return new RagService(
            _embeddingServiceMock.Object,
            _vectorStoreMock.Object,
            _chatProviderMock.Object,
            _optionsMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task AskAsync_NoRelevantResults_ReturnsDeclineWithoutCallingChatProvider()
    {
        // Arrange: vector store returns a result, but its score is below threshold.
        _optionsMock.Setup(o => o.TopK).Returns(3);
        _optionsMock.Setup(o => o.RelevanceThreshold).Returns(0.7f);

        _embeddingServiceMock
            .Setup(e => e.GetEmbeddingAsync(It.IsAny<string>()))
            .ReturnsAsync(new float[] { 0.1f, 0.2f });

        _vectorStoreMock
            .Setup(v => v.SearchAsync(It.IsAny<float[]>(), It.IsAny<int>()))
            .ReturnsAsync(new List<SearchResult>
            {
                new SearchResult("Unrelated text", 0.4f)
            });

        var sut = CreateSut();

        // Act
        ChatAnswer result = await sut.AskAsync(new ChatRequest("What is your refund policy?"));

        // Assert
        Assert.False(result.AnsweredFromKnowledgeBase);
        Assert.Equal(0, result.RetrievedChunks);
        Assert.Contains("don't have information", result.Content);

        // The chat provider must never be called when there's no relevant context —
        // this is the core guarantee of the threshold guard.
        _chatProviderMock.Verify(
            c => c.SendMessageAsync(It.IsAny<List<ChatTurn>>()),
            Times.Never);
    }

    [Fact]
    public async Task AskAsync_RelevantResultFound_ReturnsGroundedAnswerFromChatProvider()
    {
        // Arrange: vector store returns a result above threshold.
        _optionsMock.Setup(o => o.TopK).Returns(3);
        _optionsMock.Setup(o => o.RelevanceThreshold).Returns(0.7f);
        _optionsMock.Setup(o => o.SystemPromptTemplate).Returns("Context:\n{0}");

        _embeddingServiceMock
            .Setup(e => e.GetEmbeddingAsync(It.IsAny<string>()))
            .ReturnsAsync(new float[] { 0.1f, 0.2f });

        _vectorStoreMock
            .Setup(v => v.SearchAsync(It.IsAny<float[]>(), It.IsAny<int>()))
            .ReturnsAsync(new List<SearchResult>
            {
                new SearchResult("Refunds are processed within 30 days.", 0.85f)
            });

        _chatProviderMock
            .Setup(c => c.SendMessageAsync(It.IsAny<List<ChatTurn>>()))
            .ReturnsAsync(new ChatResponse("You can request a refund within 30 days.", 50, 20));

        var sut = CreateSut();

        // Act
        ChatAnswer result = await sut.AskAsync(new ChatRequest("What is your refund policy?"));

        // Assert
        Assert.True(result.AnsweredFromKnowledgeBase);
        Assert.Equal(1, result.RetrievedChunks);
        Assert.Equal("You can request a refund within 30 days.", result.Content);

        _chatProviderMock.Verify(
            c => c.SendMessageAsync(It.IsAny<List<ChatTurn>>()),
            Times.Once);
    }
}