using System;

namespace PockeTwit.SpecialTimelines
{
    [Serializable]
    public class SavedSearchTimeLine : ISpecialTimeLine
    {
        public bool autoUpdate { get; set; }

        public string ListName
        {
            get
            {
                return "SavedSearch_TimeLine_" + name;
            }
        }

        public string name { get; set; }

        public string SearchPhrase { get; set; }

        public string GetConstraints()
        {
            return " AND statuses.SearchTerm='" + SearchPhrase + "' ";
        }

        public override string ToString()
        {
            return name;
        }

        public SpecialTimeLinesRepository.TimeLineType Timelinetype
        {
            get { return SpecialTimeLinesRepository.TimeLineType.SavedSearch; }
        }
    }
}