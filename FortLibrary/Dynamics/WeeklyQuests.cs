﻿using FortLibrary.EpicResponses.Profile.Query.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FortLibrary.Dynamics
{
    public class WeeklyQuestsJson
    {
        public string BundleId { get; set; } = string.Empty;
        public string BundleSchedule { get; set; } = string.Empty;

        public WeeklyQuestJsonBundleRequired BundleRequired { get; set; } = new WeeklyQuestJsonBundleRequired();

        public List<WeeklyObjects> BundlesObject { get; set; } = new List<WeeklyObjects>();
       // public List<WeeklyObjects> PaidBundleObject { get; set; } = new List<WeeklyObjects>();
        //public List<WeeklyObjects> OtherQuests { get; set; } = new List<WeeklyObjects>();
    }

    public class WeeklyQuestJsonBundleRequired
    {
        public int RequiredLevel { get; set; } = 1;
        public List<WeeklyRewards> CompleteItems { get; set; } = new List<WeeklyRewards>();
}

    public class WeeklyObjects
    {
        public string templateId { get; set; } = string.Empty;
        public Quest_data quest_data { get; set; } = new Quest_data();
        public WeeklyRewards Rewards { get; set; } = new WeeklyRewards();
        public List<WeeklyObjectsObjectives> Objectives { get; set; } = new List<WeeklyObjectsObjectives>();
    }

    public class Quest_data
    {
        public bool RequireBP { get; set; } = false;
        public bool ExtraQuests { get; set; } = true;
        public bool Steps { get; set; } = false;

        public bool IsWeekly { get; set; } = false;
    }

    public class WeeklyObjectsObjectives
    {
        public string BackendName { get; set; } = string.Empty;
        public int Count { get; set; } = 0;
        public int Stage { get; set; } = -1;
    }

    public class WeeklyRewards
    {
        public int BattleStars { get; set; } = 0;
        public List<WeeklyRewardsQuest> Quest { get; set; } = new List<WeeklyRewardsQuest>();
        public List<WeeklyRewardsQuest> GrantedItems { get; set; } = new List<WeeklyRewardsQuest>();
    }

    public class WeeklyRewardsQuest
    {
        public string TemplateId { get; set; } = string.Empty;
        public List<AthenaItemVariants> variants { get; set; } = new List<AthenaItemVariants>();
        public List<NewAddedItemVariants> new_variants { get; set; } = new List<NewAddedItemVariants>();
        public string connectedTemplate { get; set; } = string.Empty;
        public int Quantity { get; set; } = 0;
    }
}
