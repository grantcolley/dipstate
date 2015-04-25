using System.Xml.Serialization;

namespace DevelopmentInProgress.DipState
{
    public enum DipStateStatus
    {
        [XmlEnum("1")]
        Uninitialised = 1,

        [XmlEnum("2")]
        Initialised = 2,

        [XmlEnum("3")]
        InProgress = 3,

        [XmlEnum("4")]
        Completed = 4,

        [XmlEnum("5")]
        Failed = 5
    }
}