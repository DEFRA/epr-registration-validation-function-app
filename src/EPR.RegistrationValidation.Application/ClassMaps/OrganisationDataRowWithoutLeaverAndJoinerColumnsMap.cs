﻿namespace EPR.RegistrationValidation.Application.ClassMaps;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CsvHelper.Configuration;
using Data.Models;

[ExcludeFromCodeCoverage]
public sealed class OrganisationDataRowWithoutLeaverAndJoinerColumnsMap : ClassMap<OrganisationDataRow>
{
    public OrganisationDataRowWithoutLeaverAndJoinerColumnsMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(x => x.StatusCode).Ignore();
        Map(x => x.OrganisationChangeReason).Ignore();
        Map(x => x.LeaverDate).Ignore();
        Map(x => x.JoinerDate).Ignore();
    }
}
