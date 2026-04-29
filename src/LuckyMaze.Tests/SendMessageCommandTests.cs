using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using NSubstitute;
using LuckyMaze.Application.Command;
using LuckyMaze.Domain;
using LuckyMaze.Domain.Enums;
using LuckyMaze.Infrastructure;
using LuckyMaze.Infrastructure.Services;

namespace LuckyMaze.Tests;

public class SendMessageCommandTests
{
    [Test]
    public async Task Handle_NotEnoughCredits_ReturnsFailure()
    {
        // Arrange: user has 5 credits, but sending costs ~100
        var db = CreateDb();
        var user = await SeedUserWithCredits(db, credits: 5);
        await SeedModel(db);

        var creditsService = Substitute.For<ICreditsService>();
        creditsService
            .EstimateChatCreditsAsync(Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(100L);

        var handler = CreateHandler(db, user.Id, creditsService);

        // Act
        var result = await handler.Handle(new SendMessageCommand(null, "Hello", "gpt-4"), CancellationToken.None);

        // Assert
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error!).Contains("Insufficient credits");
    }

    [Test]
    public async Task Handle_NotEnoughCredits_ErrorShowsAmounts()
    {
        var db = CreateDb();
        var user = await SeedUserWithCredits(db, credits: 50);
        await SeedModel(db);

        var creditsService = Substitute.For<ICreditsService>();
        creditsService
            .EstimateChatCreditsAsync(Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(200L);

        var handler = CreateHandler(db, user.Id, creditsService);

        var result = await handler.Handle(new SendMessageCommand(null, "Hello", "gpt-4"), CancellationToken.None);

        await Assert.That(result.Error!).Contains("200");
        await Assert.That(result.Error!).Contains("50");
    }

    [Test]
    public async Task Handle_NotEnoughCredits_NeverCallsAI()
    {
        var db = CreateDb();
        var user = await SeedUserWithCredits(db, credits: 0);
        await SeedModel(db);

        var creditsService = Substitute.For<ICreditsService>();
        creditsService
            .EstimateChatCreditsAsync(Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(1L);

        var chatService = Substitute.For<IChatCompletionService>();
        var handler = CreateHandler(db, user.Id, creditsService, chatService);

        await handler.Handle(new SendMessageCommand(null, "Hello", "gpt-4"), CancellationToken.None);

        await chatService.DidNotReceive()
            .GetChatMessageContentsAsync(Arg.Any<ChatHistory>(), Arg.Any<PromptExecutionSettings>(), Arg.Any<Kernel>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ExactlyEnoughCredits_Succeeds()
    {
        var db = CreateDb();
        var user = await SeedUserWithCredits(db, credits: 100);
        await SeedModel(db);

        var creditsService = Substitute.For<ICreditsService>();
        creditsService
            .EstimateChatCreditsAsync(Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(100L);
        creditsService
            .CalculateChatCreditsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(80L);

        var handler = CreateHandler(db, user.Id, creditsService, FakeChatService("Hi!"));

        var result = await handler.Handle(new SendMessageCommand(null, "Hello", "gpt-4"), CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task Handle_Success_DeductsActualCostNotEstimate()
    {
        // Arrange: estimate is 500, but actual cost is 120
        var dbName = Guid.NewGuid().ToString();
        var db = CreateDb(dbName);
        var user = await SeedUserWithCredits(db, credits: 1000);
        await SeedModel(db);

        var creditsService = Substitute.For<ICreditsService>();
        creditsService
            .EstimateChatCreditsAsync(Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(500L);
        creditsService
            .CalculateChatCreditsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(120L);

        var handler = CreateHandler(db, user.Id, creditsService, FakeChatService("Response"));

        // Act
        await handler.Handle(new SendMessageCommand(null, "Hello", "gpt-4"), CancellationToken.None);

        // Assert: 1000 - 120 = 880 (not 1000 - 500)
        var checkDb = CreateDb(dbName);
        var updatedUser = await checkDb.Users.SingleAsync(u => u.Id == user.Id);
        await Assert.That(updatedUser.CreditBalance).IsEqualTo(880);
    }

    [Test]
    public async Task Handle_Success_UsesActualTokensFromAIResponse()
    {
        var db = CreateDb();
        var user = await SeedUserWithCredits(db, credits: 10_000);
        await SeedModel(db);

        var creditsService = Substitute.For<ICreditsService>();
        creditsService
            .EstimateChatCreditsAsync(Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(500L);
        creditsService
            .CalculateChatCreditsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(100L);

        // AI response reports 250 prompt tokens, 75 completion tokens
        var handler = CreateHandler(db, user.Id, creditsService, FakeChatService("Hi", promptTokens: 250, completionTokens: 75));

        await handler.Handle(new SendMessageCommand(null, "Hello", "gpt-4"), CancellationToken.None);

        // Should calculate cost with actual tokens (250, 75) and model prices (3, 15)
        await creditsService.Received(1).CalculateChatCreditsAsync(
            250, 75, 3m, 15m, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_Success_UsesModelPricesForEstimate()
    {
        var db = CreateDb();
        var user = await SeedUserWithCredits(db, credits: 10_000);
        await SeedModel(db);

        var creditsService = Substitute.For<ICreditsService>();
        creditsService
            .EstimateChatCreditsAsync(Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(100L);
        creditsService
            .CalculateChatCreditsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(50L);

        var handler = CreateHandler(db, user.Id, creditsService, FakeChatService("Hi"));

        await handler.Handle(new SendMessageCommand(null, "Hello", "gpt-4"), CancellationToken.None);

        // Model has PromptPrice=3, CompletionPrice=15
        await creditsService.Received(1).EstimateChatCreditsAsync(
            Arg.Any<int>(), 3m, 15m, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_FreeModel_ZeroCreditsSucceeds()
    {
        var db = CreateDb();
        var user = await SeedUserWithCredits(db, credits: 0);
        await SeedModel(db);

        var creditsService = Substitute.For<ICreditsService>();
        creditsService
            .EstimateChatCreditsAsync(Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(0L);
        creditsService
            .CalculateChatCreditsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(0L);

        var handler = CreateHandler(db, user.Id, creditsService, FakeChatService("Free response"));

        var result = await handler.Handle(new SendMessageCommand(null, "Hello", "gpt-4"), CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task Handle_LockedUser_Fails()
    {
        var db = CreateDb();
        var user = await SeedUserWithCredits(db, credits: 10_000);
        user.IsLocked = true;
        await db.SaveChangesAsync();
        await SeedModel(db);
        var handler = CreateHandler(db, user.Id);

        var result = await handler.Handle(new SendMessageCommand(null, "Hello", "gpt-4"), CancellationToken.None);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error!).Contains("locked");
    }

    [Test]
    public async Task Handle_NullModel_Fails()
    {
        var db = CreateDb();
        var user = await SeedUserWithCredits(db, credits: 10_000);
        var handler = CreateHandler(db, user.Id);

        var result = await handler.Handle(new SendMessageCommand(null, "Hello", null), CancellationToken.None);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error!).Contains("Model is required");
    }

    [Test]
    public async Task Handle_UnknownModel_Fails()
    {
        var db = CreateDb();
        var user = await SeedUserWithCredits(db, credits: 10_000);
        await SeedModel(db);
        var handler = CreateHandler(db, user.Id);

        var result = await handler.Handle(new SendMessageCommand(null, "Hello", "nonexistent"), CancellationToken.None);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error!).Contains("not found");
    }

    [Test]
    public async Task Handle_NonexistentChat_Fails()
    {
        var db = CreateDb();
        var user = await SeedUserWithCredits(db, credits: 10_000);
        await SeedModel(db);
        var handler = CreateHandler(db, user.Id);

        var result = await handler.Handle(new SendMessageCommand(Guid.NewGuid(), "Hello", "gpt-4"), CancellationToken.None);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error!).Contains("Chat not found");
    }

    // --- Simple factory methods (no logic, just reduce repeated boilerplate) ---

    private static LuckyMazeDbContext CreateDb(string? name = null)
    {
        var options = new DbContextOptionsBuilder<LuckyMazeDbContext>()
            .UseInMemoryDatabase(name ?? Guid.NewGuid().ToString())
            .Options;
        return new LuckyMazeDbContext(options);
    }

    private static async Task<User> SeedUserWithCredits(LuckyMazeDbContext db, long credits)
    {
        db.AdminSettings.Add(new AdminSettings());
        var user = new User
        {
            ExternalId = "ext-1",
            Email = "test@test.com",
            CreditBalance = credits,
            Preferences = new UserPreferences()
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private static async Task SeedModel(LuckyMazeDbContext db)
    {
        db.Models.Add(new Model
        {
            ModelId = "gpt-4",
            Name = "GPT-4",
            PromptPricePerMillion = 3m,
            CompletionPricePerMillion = 15m
        });
        await db.SaveChangesAsync();
    }

    private static IChatCompletionService FakeChatService(string reply, int promptTokens = 100, int completionTokens = 50)
    {
        var metadata = new Dictionary<string, object?>
        {
            ["Usage"] = new { InputTokenCount = promptTokens, OutputTokenCount = completionTokens }
        };
        var response = new ChatMessageContent(AuthorRole.Assistant, reply, metadata: metadata);

        var service = Substitute.For<IChatCompletionService>();
        service
            .GetChatMessageContentsAsync(Arg.Any<ChatHistory>(), Arg.Any<PromptExecutionSettings>(), Arg.Any<Kernel>(), Arg.Any<CancellationToken>())
            .Returns(new List<ChatMessageContent> { response });
        return service;
    }

    private static SendMessageCommandHandler CreateHandler(
        LuckyMazeDbContext db,
        Guid userId,
        ICreditsService? creditsService = null,
        IChatCompletionService? chatService = null)
    {
        var userService = Substitute.For<IUserService>();
        userService.GetCurrentUserIdAsync(Arg.Any<CancellationToken>()).Returns(userId);

        return new SendMessageCommandHandler(
            userService,
            db,
            chatService ?? Substitute.For<IChatCompletionService>(),
            creditsService ?? Substitute.For<ICreditsService>());
    }
}
