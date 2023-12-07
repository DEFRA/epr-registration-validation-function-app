namespace EPR.RegistrationValidation.Data.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ColumnAttribute : Attribute
{
    public ColumnAttribute(int index)
    {
        Index = index;
    }

    public int Index { get; set; }
}