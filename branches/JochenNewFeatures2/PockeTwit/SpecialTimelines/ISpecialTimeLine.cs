using System.Xml.Serialization;
using System;
using System.Collections.Generic;
namespace PockeTwit
{
    public interface ISpecialTimeLine
    {
        string ListName { get; }
        string name { get; set; }
        string GetConstraints();
        string ToString();
        SpecialTimelines.SpecialTimeLinesRepository.TimeLineType Timelinetype { get; }
    }

    [Serializable]
    public class SpecialTimeLineSerializationHelper
    {
        public SpecialTimeLineSerializationHelper(List<ISpecialTimeLine> items)
        {
            Items = items.ToArray();
        }

        public SpecialTimeLineSerializationHelper()
        {
        }

        [XmlArrayItem(ElementName = "SavedSearchItem",
        Type = typeof(PockeTwit.SpecialTimelines.SavedSearchTimeLine)),
        XmlArrayItem(ElementName = "UserGroupItem",
        Type = typeof(PockeTwit.SpecialTimelines.UserGroupTimeLine))]
        [XmlArray]
        public ISpecialTimeLine[] Items { get; set; }
    }
}