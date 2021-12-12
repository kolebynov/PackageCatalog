using Microsoft.AspNetCore.Mvc;
using PackageCatalog.Api.Infrastructure;
using PackageCatalog.Api.Interfaces;
using PackageCatalog.Contracts.V1;

namespace PackageCatalog.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
	private readonly ITempContentStorage tempContentStorage;

	public UploadController(ITempContentStorage tempContentStorage)
	{
		this.tempContentStorage = tempContentStorage ?? throw new ArgumentNullException(nameof(tempContentStorage));
	}

	[HttpPost]
	[MapToApiVersion("1.0")]
	[ProducesResponseType(typeof(UploadContentResponseV1), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
	[RequireScope(ScopeConstants.Upload)]
	public async Task<IActionResult> UploadContent(IFormFile content, CancellationToken cancellationToken)
	{
		var ticket = await tempContentStorage.StoreTempContent(content.OpenReadStream(), cancellationToken);
		return Ok(new UploadContentResponseV1 { Ticket = ticket });
	}
}