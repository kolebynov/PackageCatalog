using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace PackageCatalog.WebApp;

public class CustomValidation : ComponentBase
{
	private ValidationMessageStore messageStore = null!;

	[CascadingParameter]
	private EditContext CurrentEditContext { get; set; } = null!;

	protected override void OnInitialized()
	{
		if (CurrentEditContext == null)
		{
			throw new InvalidOperationException(
				$"{nameof(CustomValidation)} requires a cascading " +
				$"parameter of type {nameof(EditContext)}. " +
				$"For example, you can use {nameof(CustomValidation)} " +
				$"inside an {nameof(EditForm)}.");
		}

		messageStore = new(CurrentEditContext);
		CurrentEditContext.OnValidationRequested += (s, e) => messageStore.Clear();
		CurrentEditContext.OnFieldChanged += (s, e) => messageStore.Clear(e.FieldIdentifier);
	}

	public void AddErrors(IReadOnlyDictionary<string, IReadOnlyCollection<string>> errors)
	{
		ClearErrors();

		foreach (var err in errors)
		{
			messageStore.Add(CurrentEditContext.Field(err.Key), err.Value);
		}

		CurrentEditContext.NotifyValidationStateChanged();
	}

	private void ClearErrors()
	{
		messageStore.Clear();
		CurrentEditContext.NotifyValidationStateChanged();
	}
}