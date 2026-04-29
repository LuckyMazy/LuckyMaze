using LuckyMaze.Domain;
using LuckyMaze.Infrastructure.Services;
using LuckyMaze.Tests.Helpers;

namespace LuckyMaze.Tests;

public class CreditsServiceTests
{
    [Test]
    public async Task FromUsdAsync_DefaultRate_Returns10000CreditsPerDollar()
    {
        await using var context = await DbHelper.CreateSeededContextAsync();
        var service = new CreditsService(context);

        var credits = await service.FromUsdAsync(1m);

        await Assert.That(credits).IsEqualTo(10_000);
    }

    [Test]
    public async Task FromUsdAsync_CustomRate_UsesConfiguredRate()
    {
        var settings = new AdminSettings { CreditsPerUsd = 5_000m };
        await using var context = await DbHelper.CreateSeededContextAsync(settings);
        var service = new CreditsService(context);

        var credits = await service.FromUsdAsync(2m);

        await Assert.That(credits).IsEqualTo(10_000);
    }

    [Test]
    public async Task FromUsdAsync_FractionalResult_RoundsUp()
    {
        await using var context = await DbHelper.CreateSeededContextAsync();
        var service = new CreditsService(context);

        var credits = await service.FromUsdAsync(0.00011m);

        await Assert.That(credits).IsEqualTo(2);
    }

    [Test]
    public async Task ToUsdAsync_DefaultRate_ConvertsBack()
    {
        await using var context = await DbHelper.CreateSeededContextAsync();
        var service = new CreditsService(context);

        var usd = await service.ToUsdAsync(10_000);

        await Assert.That(usd).IsEqualTo(1m);
    }

    [Test]
    public async Task ToUsdAsync_SmallCredits_ReturnsFractionalUsd()
    {
        await using var context = await DbHelper.CreateSeededContextAsync();
        var service = new CreditsService(context);

        var usd = await service.ToUsdAsync(1);

        await Assert.That(usd).IsEqualTo(0.0001m);
    }

    [Test]
    public async Task CalculateChatCreditsAsync_DefaultMultiplier_CalculatesCorrectly()
    {
        await using var context = await DbHelper.CreateSeededContextAsync();
        var service = new CreditsService(context);

        var credits = await service.CalculateChatCreditsAsync(
            promptTokens: 1_000_000,
            completionTokens: 1_000_000,
            promptPricePerMillion: 3m,
            completionPricePerMillion: 15m);

        var expectedUsd = 3m + 15m;
        var expectedCredits = (long)Math.Ceiling(expectedUsd * 10_000m);
        await Assert.That(credits).IsEqualTo(expectedCredits);
    }

    [Test]
    public async Task CalculateChatCreditsAsync_WithCostMultiplier_AppliesMultiplier()
    {
        var settings = new AdminSettings { CostMultiplier = 2m };
        await using var context = await DbHelper.CreateSeededContextAsync(settings);
        var service = new CreditsService(context);

        var credits = await service.CalculateChatCreditsAsync(
            promptTokens: 1_000_000,
            completionTokens: 0,
            promptPricePerMillion: 1m,
            completionPricePerMillion: 0m);

        var expectedCredits = (long)Math.Ceiling(1m * 2m * 10_000m);
        await Assert.That(credits).IsEqualTo(expectedCredits);
    }

    [Test]
    public async Task CalculateChatCreditsAsync_ZeroMultiplier_FallsBackToOne()
    {
        var settings = new AdminSettings { CostMultiplier = 0m };
        await using var context = await DbHelper.CreateSeededContextAsync(settings);
        var service = new CreditsService(context);

        var credits = await service.CalculateChatCreditsAsync(
            promptTokens: 1_000_000,
            completionTokens: 0,
            promptPricePerMillion: 1m,
            completionPricePerMillion: 0m);

        var expectedCredits = (long)Math.Ceiling(1m * 1m * 10_000m);
        await Assert.That(credits).IsEqualTo(expectedCredits);
    }

    [Test]
    public async Task CalculateChatCreditsAsync_ZeroTokens_ReturnsZero()
    {
        await using var context = await DbHelper.CreateSeededContextAsync();
        var service = new CreditsService(context);

        var credits = await service.CalculateChatCreditsAsync(
            promptTokens: 0,
            completionTokens: 0,
            promptPricePerMillion: 3m,
            completionPricePerMillion: 15m);

        await Assert.That(credits).IsEqualTo(0);
    }

    [Test]
    public async Task CalculateChatCreditsAsync_SmallTokenCount_RoundsUp()
    {
        await using var context = await DbHelper.CreateSeededContextAsync();
        var service = new CreditsService(context);

        var credits = await service.CalculateChatCreditsAsync(
            promptTokens: 1,
            completionTokens: 0,
            promptPricePerMillion: 1m,
            completionPricePerMillion: 0m);

        await Assert.That(credits).IsGreaterThanOrEqualTo(1);
    }

    [Test]
    public async Task EstimateChatCreditsAsync_UsesCharToTokenRatioAndMaxOutput()
    {
        await using var context = await DbHelper.CreateSeededContextAsync();
        var service = new CreditsService(context);

        var credits = await service.EstimateChatCreditsAsync(
            inputCharCount: 4000,
            promptPricePerMillion: 1m,
            completionPricePerMillion: 1m);

        var expectedInputTokens = 4000 / 4;
        var expectedOutputTokens = 4000;
        var expectedUsd = (expectedInputTokens * 1m / 1_000_000m + expectedOutputTokens * 1m / 1_000_000m) * 1m;
        var expectedCredits = (long)Math.Ceiling(expectedUsd * 10_000m);
        await Assert.That(credits).IsEqualTo(expectedCredits);
    }
}
