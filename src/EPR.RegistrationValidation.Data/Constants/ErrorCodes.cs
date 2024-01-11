namespace EPR.RegistrationValidation.Data.Constants;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public static class ErrorCodes
{
    public const string FileFormatInvalid = "80";
    public const string CsvFileEmptyErrorCode = "82";
    public const string CsvFileInvalidHeaderErrorCode = "83";
    public const string MissingOrganisationId = "801";
    public const string MissingOrganisationName = "802";
    public const string MissingHomeNationCode = "803";
    public const string InvalidHomeNationCode = "804";
    public const string MissingPrimaryContactFirstName = "805";
    public const string MissingPrimaryContactLastName = "806";
    public const string MissingPrimaryContactEmail = "807";
    public const string MissingPrimaryContactPhoneNumber = "808";
    public const string MissingRegisteredAddressLine1 = "809";
    public const string MissingRegisteredAddressPostcode = "810";
    public const string MissingRegisteredAddressPhoneNumber = "811";
    public const string InvalidAuditAddressCountry = "812";
    public const string CharacterLengthExceeded = "813";
    public const string MissingPrincipalAddressLine1 = "816";
    public const string MissingPrincipalAddressPostcode = "817";
    public const string MissingPrincipalAddressPhoneNumber = "818";
    public const string DuplicateOrganisationIdSubsidiaryId = "815";
    public const string MainActivitySicNotFiveDigitsInteger = "819";
    public const string MissingPrimaryActivity = "820";
    public const string MultiplePrimaryActivity = "821";
    public const string InvalidPackagingActivity = "822";
    public const string MissingPackagingActivity = "823";
    public const string TradingNameSameAsOrganisationName = "840";
}