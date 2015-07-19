using System.Xml.Serialization;

namespace DevelopmentInProgress.DipState
{
    public enum DipStateActionType
    {
        [XmlEnum("1")]
        Status,

        [XmlEnum("2")]
        Entry,

        [XmlEnum("3")]
        Exit
    }
}
