using System;
using System.Collections.Generic;

namespace NorthernTown2026
{
    [Serializable]
    public class StatCheck
    {
        public StatId Stat;
        public int Threshold;
        public string SuccessNodeId;
        public string FailNodeId;
    }

    [Serializable]
    public class ChoiceOption
    {
        public string Text;
        public string NextNodeId;
        public StatCheck Check;
        public int GrantXp;
        public string GrantItemId;
        public string RequiresItemId;
        public int RequiresInsightSum;
    }

    [Serializable]
    public class ChoiceView
    {
        public ChoiceOption Option;
        public bool IsAvailable;
        public string DisabledReason;
    }

    [Serializable]
    public class StoryNode
    {
        public string Id;
        public string Text;
        public List<ChoiceOption> Choices = new List<ChoiceOption>();
    }
}
