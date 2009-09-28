using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PockeTwit.SpecialTimelines
{
    [Serializable]
    public class UserGroupTimeLine : ISpecialTimeLine
    {
        [Serializable]
        public class GroupTerm
        {
            public string Term;
            public string Name;
            public bool Exclusive;
        }
        
        public string ListName
        {
            get
            {
                return "Grouped_TimeLine_" + name;
            }
        }

        public string name { get; set; }
        [XmlArray]
        public GroupTerm[] Terms { get; set; }

        
        public void AddItem(string term, string screenName, bool exclusive)
        {
            var newTerm = new GroupTerm { Term = term, Name = screenName, Exclusive = exclusive };
            if (Terms != null && Terms.Length > 0)
            {
                var items = new List<GroupTerm>(Terms);
                if (!items.Contains(newTerm))
                {
                    items.Add(newTerm);
                }
                Terms = items.ToArray();
            }
            else
            {
                Terms = new[] { newTerm };
            }
        }
        public void RemoveItem(string term)
        {
            var items = new List<GroupTerm>(Terms);
            var toRemove = new GroupTerm();
            foreach (var t in items)
            {
                if (t.Term == term)
                {
                    toRemove = t;
                }
            }
            if (items.Contains(toRemove))
            {
                items.Remove(toRemove);
            }
            Terms = items.ToArray();
            if(Terms.Length==0)
            {
                SpecialTimeLinesRepository.Remove(this);
            }
            SpecialTimeLinesRepository.Save();
        }
        

        public string GetConstraints()
        {
            if (Terms == null) 
            {
                SpecialTimeLinesRepository.Load();
            }
            if (Terms == null) 
            {
                return "";
            }
            var ret = "";
            var userList = new List<string>();
            foreach (var t in Terms)
            {
                userList.Add("'"+t.Term+"'");
                
            }
            if (userList.Count > 0)
            {
                ret = " AND statuses.userid IN(" + string.Join(",", userList.ToArray()) + ") ";
            }

            return ret;
        }

        public override string ToString()
        {
            return name;
        }

        public SpecialTimeLinesRepository.TimeLineType Timelinetype
        {
            get { return SpecialTimeLinesRepository.TimeLineType.UserGroup; }
        }
    }
}