﻿namespace EPR.RegistrationValidation.Data.Constants;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public static class ErrorCodes
{
    public const string FileFormatInvalid = "80";
    public const string CsvFileEmptyErrorCode = "82";
    public const string CsvFileInvalidHeaderErrorCode = "83";
    public const string UncaughtExceptionErrorCode = "89";
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
    public const string MissingAuditAddressLine1 = "814";
    public const string MissingAuditPostcode = "815";
    public const string MissingPrincipalAddressLine1 = "816";
    public const string MissingPrincipalAddressPostcode = "817";
    public const string MissingPrincipalAddressPhoneNumber = "818";
    public const string DuplicateOrganisationIdSubsidiaryId = "819";
    public const string MainActivitySicNotFiveDigitsInteger = "820";
    public const string MissingPrimaryActivity = "821";
    public const string MultiplePrimaryActivity = "822";
    public const string InvalidPackagingActivity = "823";
    public const string MissingPackagingActivitySo = "824";
    public const string MissingPackagingActivityPf = "825";
    public const string MissingPackagingActivityIm = "826";
    public const string MissingPackagingActivitySe = "827";
    public const string MissingPackagingActivityHl = "828";
    public const string MissingPackagingActivityOm = "829";
    public const string MissingPackagingActivitySl = "830";
    public const string InvalidProduceBlankPackagingFlag = "831";
    public const string TradingNameSameAsOrganisationName = "840";
    public const string MissingServiceOfNoticeAddressLine1 = "845";
    public const string MissingServiceOfNoticePostcode = "846";
    public const string MissingServiceOfNoticePhoneNumber = "847";
    public const string HeadOrganisationMissingSubOrganisation = "848";
    public const string TurnoverHasComma = "855";
    public const string InvalidTurnoverDecimalValues = "856";
    public const string TurnoverHasZeroOrNegativeValue = "857";
    public const string InvalidTurnoverDigits = "858";
    public const string InvalidCompanyHouseNumber = "859";
    public const string CompanyHouseNumberMustBeEmpty = "860";
    public const string CompaniesHouseNumberNotMatchOrganisationId = "861";
    public const string TotalTonnageMustBeGreaterThanZero = "865";
    public const string TotalTonnageIncludesComma = "867";
    public const string TotalTonnageIsNotNumber = "868";
    public const string MissingOrganisationTypeCode = "880";
    public const string InvalidOrganisationTypeCode = "881";
    public const string CheckOrganisationId = "882";
    public const string SubsidiaryIdDoesNotExist = "883";
    public const string SubsidiaryIdBelongsToDifferentOrganisation = "884";
    public const string SubsidiaryDoesNotBelongToAnyOrganisation = "885";
    public const string SubsidiaryIdDoesNotMatchCompaniesHouseNumber = "886";
    public const string BrandDetailsNotMatchingOrganisation = "890";
    public const string PartnerDetailsNotMatchingOrganisation = "891";
    public const string BrandDetailsNotMatchingSubsidiary = "892";
    public const string PartnerDetailsNotMatchingSubsidiary = "893";
    public const string MissingOrganisationSizeValue = "894";
    public const string InvalidOrganisationSizeValue = "895";
    public const string SmallProducerTurnoverInvalid = "896";
    public const string LargeProducerTurnoverInvalid = "897";
    public const string InvalidJoinerDateFormat = "898";
    public const string LeaverCodeMustBePresentWhenLeaverDatePresent = "899";
    public const string LeaverDateMustBePresentWhenLeaverCodePresent = "900";
    public const string InvalidLeaverDateFormat = "901";
    public const string LeaverDateMustBeAfterJoinerDate = "902";
    public const string OrganisationChangeReasonMustBePresentWhenLeaverCodePresent = "903";
    public const string OrganisationChangeReasonCannotBeLongerThan200Characters = "904";
    public const string JoinerDateDoesNotMatchJoinerDateInDatabase = "905";
    public const string OrganisationChangeReasonCannotIncludeSpecialCharacters = "906";
    public const string InvalidLeaverCode = "907";
    public const string LeaverDateCannotBeInTheFuture = "908";
    public const string JoinerDateCannotBeInTheFuture = "909";
    public const string RegistrationTypeCodeIsMandatory = "910";
    public const string RegistrationTypeCodeInvalidValue = "911";
    public const string RegistrationTypeCodeIsMandatoryCS = "912";
    public const string LeaverCodeMustBePresentWhenLeaverDatePresentCS = "913";
    public const string LeaverDateMustBePresentWhenLeaverCodePresentCS = "914";
    public const string LeaverDateMustBeAfterJoinerDateCS = "915";
}