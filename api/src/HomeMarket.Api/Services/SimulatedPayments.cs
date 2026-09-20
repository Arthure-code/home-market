using System.Security.Cryptography;
using HomeMarket.Api.Interfaces;

namespace HomeMarket.Api.Services
{
    // The gateway the shop runs with until a provider is plugged in. It
    // answers as one would: a number that fails the Luhn check, a past
    // expiry or a bad security code is refused with the reason, the
    // provider's usual test number for a decline is declined, anything
    // else is accepted with a brand, the last four digits and a reference.
    // Nothing leaves the process.
    public class SimulatedPayments : IPaymentGateway
    {
        public const string DeclinedNumber = "4000000000000002";

        public Task<PaymentResult> ChargeAsync(PaymentRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Charge(request));
        }

        private static PaymentResult Charge(PaymentRequest request)
        {
            var digits = new string(request.CardNumber.Where(char.IsDigit).ToArray());
            if (digits.Length < 12 || digits.Length > 19 || request.CardNumber.Any(c => !char.IsDigit(c) && c != ' ' && c != '-') || !PassesLuhn(digits))
            {
                return PaymentResult.Refused("That card number is not valid.");
            }
            if (request.CardHolderName.Trim().Length < 2)
            {
                return PaymentResult.Refused("The name on the card is missing.");
            }
            if (!IsStillValid(request.Expiry))
            {
                return PaymentResult.Refused("That card has expired.");
            }
            if (request.SecurityCode.Length < 3 || request.SecurityCode.Length > 4 || !request.SecurityCode.All(char.IsDigit))
            {
                return PaymentResult.Refused("The security code is three or four digits.");
            }
            if (digits == DeclinedNumber)
            {
                return PaymentResult.Refused("Your card was declined.");
            }
            var reference = "sim_" + Convert.ToHexString(RandomNumberGenerator.GetBytes(12)).ToLowerInvariant();
            return new PaymentResult(true, string.Empty, BrandOf(digits), digits[^4..], reference);
        }

        private static bool PassesLuhn(string digits)
        {
            var sum = 0;
            var doubleIt = false;
            for (var i = digits.Length - 1; i >= 0; i--)
            {
                var d = digits[i] - '0';
                if (doubleIt)
                {
                    d *= 2;
                    if (d > 9) d -= 9;
                }
                sum += d;
                doubleIt = !doubleIt;
            }
            return sum % 10 == 0;
        }

        // MM/YY, valid through the last day of that month.
        private static bool IsStillValid(string expiry)
        {
            var parts = expiry.Split('/');
            if (parts.Length != 2 || !int.TryParse(parts[0], out var month) || !int.TryParse(parts[1], out var year) || month < 1 || month > 12)
            {
                return false;
            }
            var endOfMonth = new DateTime(2000 + year, month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);
            return DateTime.UtcNow < endOfMonth;
        }

        private static string BrandOf(string digits)
        {
            return digits[0] switch
            {
                '4' => "Visa",
                '5' => "Mastercard",
                '3' => "American Express",
                _ => "Card",
            };
        }
    }
}
