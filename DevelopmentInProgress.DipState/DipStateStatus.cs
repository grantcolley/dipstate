using System.Xml.Serialization;

namespace DevelopmentInProgress.DipState
{
    public enum DipStateStatus
    {
        [XmlEnum("1")]
        NotStarted = 1,

        [XmlEnum("2")]
        InProgress = 2,

        [XmlEnum("3")]
        Completed = 3,
    }
}
