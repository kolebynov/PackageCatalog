using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PackageCatalog.Api.Internal;

namespace PackageCatalog.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public sealed class DevController : ControllerBase
{
	[HttpPost("generateJwtToken")]
	[MapToApiVersion("1.0")]
	public string GenerateJwtToken([FromBody] GenerateJwtTokenParams generateJwtTokenParams)
	{
		var token = new JwtSecurityToken(
			claims: generateJwtTokenParams.Roles.Select(x => new Claim("role", x)).Append(new Claim("scope", generateJwtTokenParams.Scope)).Append(new Claim("test.test", "123")),
			expires: DateTime.UtcNow.AddYears(1),
			issuer: JwtOptions.Issuer,
			audience: JwtOptions.Audience,
			signingCredentials: new SigningCredentials(JwtOptions.SymmetricSecurityKey, SecurityAlgorithms.HmacSha256));

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	[HttpPost("getClaims")]
	[MapToApiVersion("1.0")]
	[Authorize]
#pragma warning disable CA1024
	public IEnumerable<Tuple<string, string>> GetClaims()
#pragma warning restore CA1024
	{
		return User.Claims.Select(x => new Tuple<string, string>(x.Type, x.Value));
	}
}