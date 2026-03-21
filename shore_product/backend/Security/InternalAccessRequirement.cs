using Microsoft.AspNetCore.Authorization;

namespace ProductApi.Security;

public sealed class InternalAccessRequirement : IAuthorizationRequirement
{
}