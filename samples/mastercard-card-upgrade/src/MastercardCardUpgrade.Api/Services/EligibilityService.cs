using MastercardCardUpgrade.Api;
using MastercardCardUpgrade.Api.Models.Cards;

namespace MastercardCardUpgrade.Api.Services;

public interface IEligibilityService
{
    void ValidateCreate(string productCode, string pan);
    void ValidateUpgrade(CardAccount card, string targetProductCode);
}

public sealed class EligibilityService : IEligibilityService
{
    private readonly IProductCatalog _catalog;

    public EligibilityService(IProductCatalog catalog)
    {
        _catalog = catalog;
    }

    public void ValidateCreate(string productCode, string pan)
    {
        _catalog.GetRequired(productCode);
        PanRules.Validate(pan);
        if (!_catalog.IsAllowedAccountRange(pan))
            throw new EligibilityException($"PAN BIN/account range is not in the allowed prefixes.");
    }

    public void ValidateUpgrade(CardAccount card, string targetProductCode)
    {
        if (card.Status == CardStatus.Closed)
            throw new EligibilityException($"Card '{card.CardId}' is closed.");

        var target = _catalog.GetRequired(targetProductCode);

        if (string.Equals(card.ProductCode, target.Code, StringComparison.OrdinalIgnoreCase))
            throw new EligibilityException("Card is already on the target product.");

        if (!_catalog.IsAllowedTransition(card.ProductCode, target.Code))
            throw new EligibilityException(
                $"Transition {card.ProductCode} → {target.Code} is not in the allowed product catalog.");
    }
}

public static class PanRules
{
    public static string Normalize(string pan) =>
        new(pan.Where(char.IsDigit).ToArray());

    public static void Validate(string pan)
    {
        if (pan.Length is < 13 or > 19)
            throw new EligibilityException("PAN must contain 13 to 19 digits.");
    }

    public static string Mask(string pan) =>
        pan.Length <= 10
            ? new string('*', pan.Length)
            : $"{pan[..6]}{new string('*', pan.Length - 10)}{pan[^4..]}";

    public static string Bin(string pan) =>
        pan.Length >= 8 ? pan[..8] : pan[..Math.Min(6, pan.Length)];

    public static string GenerateMastercardTestPan(string? binPrefix = null)
    {
        var prefix = new string((binPrefix ?? MastercardTestData.Pan).Where(char.IsDigit).ToArray());
        if (prefix.Length < 6)
            prefix = MastercardTestData.Pan[..6];
        if (prefix.Length > 8)
            prefix = prefix[..6];

        var body = prefix.PadRight(15, '0').ToCharArray();
        var seq = Random.Shared.Next(0, 1_000_000_000).ToString("D9");
        for (var i = 0; i < 9 && i + prefix.Length < 15; i++)
            body[prefix.Length + i] = seq[i];

        var withoutCheck = new string(body[..15]);
        return withoutCheck + LuhnCheckDigit(withoutCheck);
    }

    public static int LuhnCheckDigit(string digits)
    {
        var sum = 0;
        var alt = true;
        for (var i = digits.Length - 1; i >= 0; i--)
        {
            var n = digits[i] - '0';
            if (alt)
            {
                n *= 2;
                if (n > 9) n -= 9;
            }
            sum += n;
            alt = !alt;
        }

        return (10 - (sum % 10)) % 10;
    }
}
