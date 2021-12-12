using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PackageCatalog.Api.Internal;

internal class JwtOptions
{
	public const string Issuer = "PackageCatalog";
	public const string Audience = "PackageCatalog";

	private const string Key = "0123456789ABCDEF";

	public static SymmetricSecurityKey SymmetricSecurityKey => new(Encoding.ASCII.GetBytes(Key));
}