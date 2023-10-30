using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Asp.Versioning;
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
			claims: new[] { new Claim("scope", generateJwtTokenParams.Scope), new Claim("role", generateJwtTokenParams.Role) },
			expires: DateTime.UtcNow.AddYears(1),
			issuer: JwtOptions.Issuer,
			audience: JwtOptions.Audience,
			signingCredentials: new SigningCredentials(JwtOptions.SymmetricSecurityKey, SecurityAlgorithms.HmacSha256));

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}