using System;
using System.Collections.Generic;

namespace EnglishLearning.Models
{
    public class AdminStatsViewModel
    {
        public IDictionary<string, int> SubmissionsBySkill { get; set; } = new Dictionary<string, int>();
        public IList<string> Last7DaysLabels { get; set; } = new List<string>();
        public IList<int> Last7DaysCounts { get; set; } = new List<int>();
    }
}

