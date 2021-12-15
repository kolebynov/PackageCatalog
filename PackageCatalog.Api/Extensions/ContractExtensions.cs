using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using PackageCatalog.Api.Interfaces;
using PackageCatalog.Api.Objects;
using PackageCatalog.Contracts.V1;
using PackageCatalog.Core.Models;
using PackageCatalog.Core.Objects;

namespace PackageCatalog.Api.Extensions;

public static class ContractExtensions
{
	public static CollectionResponseV1<T> ToCollectionResponseV1<T>(
		this IReadOnlyCollection<T> data, ControllerBase controller, ISkipTokenGenerator skipTokenGenerator,
		PaginationV1 paginationV1)
	{
		if (data.Count != paginationV1.Top)
		{
			return new() { Data = data };
		}

		var newValues = controller.ModelState
			.Where(x => x.Value?.AttemptedValue != null && !x.Key.Equals("skipToken", StringComparison.OrdinalIgnoreCase))
			.ToDictionary(x => x.Key, x => x.Value!.AttemptedValue);
		var skipToken = skipTokenGenerator.GenerateSkipToken(
			new SkipToken { Skip = GetSkip(paginationV1, skipTokenGenerator) + paginationV1.Top });
		newValues["skipToken"] = skipToken;
		newValues[Constants.ApiVersionParameterName] = controller.HttpContext.Features.Get<IApiVersioningFeature>()
			!.RequestedApiVersion?.ToString();

		var nextUri = new Uri(controller.Url.ActionLink(values: newValues)!);
		return new() { Data = data, NextUri = nextUri, SkipToken = skipToken };
	}

	public static Pagination ToPaginationObject(this PaginationV1 paginationV1, ISkipTokenGenerator skipTokenGenerator) =>
		new(paginationV1.Top, GetSkip(paginationV1, skipTokenGenerator));

	public static CategoryV1 ToContractV1(this Category package) => new()
	{
		Id = package.Id.Value,
		DisplayName = package.DisplayName,
	};

	public static PackageV1 ToContractV1(this Package package) => new()
	{
		Id = package.Id.Value,
		DisplayName = package.DisplayName,
		CategoryId = package.CategoryId.Value,
	};

	public static PackageVersionV1 ToContractV1(this PackageVersion packageVersion) => new()
	{
		PackageId = packageVersion.PackageId.Value,
		Version = packageVersion.Version,
	};

	private static int GetSkip(PaginationV1 paginationV1, ISkipTokenGenerator skipTokenGenerator) =>
		string.IsNullOrEmpty(paginationV1.SkipToken)
			? paginationV1.Skip ?? 0
			: skipTokenGenerator.ParseSkipToken<SkipToken>(paginationV1.SkipToken).Skip;
}