using LuckyMaze.Domain.Enums;

namespace LuckyMaze.Application.Dtos;

public record AdjustCreditsDto(long Amount, CreditAdjustmentMode Mode);
