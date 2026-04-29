using LuckyMaze.Domain.Enums;

namespace LuckyMaze.Application.Dtos;

public record AdminSettingsDto(
    decimal? MaxPricePerMillionTokens,
    ModelListMode ActiveModelListMode,
    long StartingBalance,
    decimal CostMultiplier,
    decimal CreditsPerUsd);
