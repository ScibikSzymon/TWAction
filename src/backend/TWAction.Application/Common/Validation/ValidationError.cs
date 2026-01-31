namespace TWAction.Application.Common.Validation;

public sealed record ValidationError(string PropertyName, string ErrorMessage);
