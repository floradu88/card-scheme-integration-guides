namespace Interchange.Configuration;

public sealed class PackageValidator
{
    private readonly IEnumerable<IPackageValidationRule> _rules;

    public PackageValidator(IEnumerable<IPackageValidationRule> rules) =>
        _rules = rules;

    public ValidationReport Validate(ConfigurationPackage package)
    {
        var findings = new List<ValidationFinding>();

        foreach (var rule in _rules)
        {
            try
            {
                findings.AddRange(rule.Validate(package));
            }
            catch (Exception ex)
            {
                findings.Add(new ValidationFinding(
                    "VALIDATOR_FAILURE",
                    ValidationSeverity.Error,
                    rule.GetType().Name,
                    ex.Message));
            }
        }

        return new ValidationReport(findings);
    }
}
