using System.Net.Mail;
using Milese.Common.Shared;

namespace Milese.Common.Types.ValueTypes.Identity;

public readonly record struct Email(string Value) : IStringValueType<Email>
{
    public static int MaxLength => 254;

    public static Result<Email, InvalidData> Parse(string? value) =>
        ValueTypeParser.StringNotEmptyMaxLengthAndFormat<Email>(value, nameof(Email), IsValidFormat);

    private static bool IsValidFormat(string value) => MailAddress.TryCreate(value, out _);
}
