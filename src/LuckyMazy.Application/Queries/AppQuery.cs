using LuckyMazy.Application.Dtos;
using LuckyMazy.Application.Models;
using Mediator;
using System;
using System.Collections.Generic;
using System.Text;

namespace LuckyMazy.Application.Queries
{
    public record AppQuery() : IQuery<Result<AppDto>>;

    public record AppQueryHandler : IQueryHandler<AppQuery, Result<AppDto>>
    {
        public async ValueTask<Result<AppDto>> Handle(AppQuery query, CancellationToken cancellationToken)
        {
            return Result<AppDto>.Success(new AppDto("1.1.1"));
        }
    }
}
