using System.Xml.Serialization;

namespace DevelopmentInProgress.DipState
{
    public enum DipStateType
    {
        [XmlEnum("1")]
        Standard = 1,

        [XmlEnum("2")]
        Aggregate = 2,

        [XmlEnum("3")]
        Auto = 3
    }
}