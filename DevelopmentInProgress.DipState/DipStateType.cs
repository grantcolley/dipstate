using System.Xml.Serialization;

namespace DevelopmentInProgress.DipState
{
    public enum DipStateType
    {
        [XmlEnum("1")]
        Standard = 1,

        [XmlEnum("2")]
        Auto = 3
    }
}