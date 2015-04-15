using System.Xml.Serialization;

namespace DevelopmentInProgress.DipState
{
    public enum DipStateAction
    {
        [XmlEnum("1")]
        Entry,

        [XmlEnum("2")]
        Exit
    }
}
