using System.Xml.Serialization;

namespace DevelopmentInProgress.DipState
{
    public enum DipStateActionType
    {
        [XmlEnum("1")]
        Entry,

        [XmlEnum("2")]
        Exit
    }
}
