using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;
using Shared.Application.Contracts.AuthJWT;

namespace AuthService.Application.MediatR.RefreshToken
{
    public record RefreshTokenCommand(string RefreshToken)
        : IRequest<ApiResponse<AuthGoResponse>>;
}
