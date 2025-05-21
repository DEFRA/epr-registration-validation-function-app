namespace EPR.RegistrationValidation.Application.Constants;

public static class StatusCode
{
    public const string BusinessIsNowSmallProducer = "A";
    public const string Liquidation = "B";
    public const string ConfirmedMemberAwaitingPayment = "C";
    public const string ConfirmedMemberAwaitingPackagingData = "D";
    public const string ConfirmedMemberAwaitingRegFile = "E";
    public const string ProducerNoLongerObligatedBelowTurnover = "F";
    public const string ProducerNoLongerObligatedBelowPackagingThreshold = "G";
    public const string ProducerNoLongerObligatedNoLongerProducer = "H";
    public const string SmallProducerJoinedGroupButStillReportsForItself = "I";
    public const string SmallProducerJoinedGroupAndParentReportsOnItsBehalf = "J";
    public const string SmallProducerLeavesGroupReportedForItself = "K";
    public const string SmallProducerLeavesGroupParentReportedForIt = "L";
    public const string LargeProducerLeftGroupButStartsToReportForItself = "M";
    public const string LargeProducerLeftGroupThatReportedForIt = "N";
    public const string LargeProducerJoinedGroupAndParentReportsOnItsBehalf = "O";
}