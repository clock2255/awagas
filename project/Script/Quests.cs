using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Atavism
{

    public class QuestLogEntry
    {
        public OID QuestId = null;
        public OID NpcId;
        public string Title = "";
        public string Description = "";
        public string Objective = "";
        public int gradeCount = 0;
        public string ProgressText = "";
        public List<QuestGradeEntry> gradeInfo;
        public bool Complete = false;
        public int GradeReached = 0;
        public int itemChosen = -1;
        public int reqLeval = 1;
        public string CompleteText = "";

    }

    public class QuestGradeEntry
    {
        public string completionText;
        public List<string> objectives;
        public List<QuestRewardEntry> rewardItems;
        public List<QuestRewardEntry> RewardItemsToChoose;
        public List<QuestRepRewardEntry> rewardRep;
        public int expReward;
        public List<QuestRewardEntry> currencies;
    }

    public class QuestRewardEntry
    {
        public string name = "";
        public int id = -1;
        public int count = 1;
        public AtavismInventoryItem item;
    }

    public class QuestRepRewardEntry
    {
        public string name = "";
        public int count = 1;
    }
    public class Quests : MonoBehaviour
    {

        static Quests instance;

        // info about the last quests we were offered
        //	int questsOfferedSelectedIndex = 0;
        List<QuestLogEntry> questsOffered = new List<QuestLogEntry>();

        // quest log info
        int questLogSelectedIndex = 0;
        List<QuestLogEntry> questLogEntries = new List<QuestLogEntry>();

        // quest progress info
        //	int questProgressSelectedIndex = 0;
        List<QuestLogEntry> questsInProgress = new List<QuestLogEntry>();
        // quest history log info
        int questHistoryLogSelectedIndex = 0;
        List<QuestLogEntry> questHistoryLogEntries = new List<QuestLogEntry>();
        OID npcID;
        public List<long> questListSelected = new List<long>();
        [SerializeField] int maxQuestsSelected = 4;
        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;

            NetworkAPI.RegisterExtensionMessageHandler("ao.QUEST_OFFER", _HandleQuestOfferResponse);
            NetworkAPI.RegisterExtensionMessageHandler("ao.QUEST_LOG_INFO", _HandleQuestLogInfo);
            NetworkAPI.RegisterExtensionMessageHandler("ao.QUEST_HISTORY_LOG_INFO", _HandleQuestHistoryLogInfo);
            NetworkAPI.RegisterExtensionMessageHandler("ao.QUEST_STATE_INFO", _HandleQuestStateInfo);
            NetworkAPI.RegisterExtensionMessageHandler("ao.QUEST_PROGRESS", _HandleQuestProgressInfo);
            NetworkAPI.RegisterExtensionMessageHandler("ao.REMOVE_QUEST_RESP", _HandleRemoveQuestResponse);
            NetworkAPI.RegisterExtensionMessageHandler("quest_event", _HandleQuestEvent);
        }

        public QuestLogEntry GetQuestOfferedInfo(int pos)
        {
            return questsOffered[pos];
        }

        public void AcceptQuest(int questPos)
        {
            NetworkAPI.SendQuestResponseMessage(npcID.ToLong(), questsOffered[questPos].QuestId.ToLong(), true);
        }

        public void DeclineQuest(int questPos)
        {
            NetworkAPI.SendQuestResponseMessage(npcID.ToLong(), questsOffered[questPos].QuestId.ToLong(), false);
        }

        public void QuestLogEntrySelected(int pos)
        {
            questLogSelectedIndex = pos;
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("QUEST_LOG_UPDATE", args);

        }
        public void QuestHistoryLogEntrySelected(int pos)
        {
            questHistoryLogSelectedIndex = pos;
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("QUEST_LOG_UPDATE", args);
        }
        public List<QuestLogEntry> GetSelectedListQuestLog()
        {
            List<QuestLogEntry> questsList = new List<QuestLogEntry>();
           
            if (!AtavismSettings.Instance.GetQuestListSelected().ContainsKey(ClientAPI.GetPlayerOid()))
                AtavismSettings.Instance.GetQuestListSelected().Add(ClientAPI.GetPlayerOid(), new List<long>());
            foreach (QuestLogEntry q in questLogEntries)
            {
                if (AtavismSettings.Instance != null  && AtavismSettings.Instance.GetQuestListSelected()[ClientAPI.GetPlayerOid()].Contains(q.QuestId.ToLong()))
                    questsList.Add(q);
            }
            return questsList;
        }

        public QuestLogEntry GetSelectedQuestLogEntry()
        {
            if (questLogEntries.Count - 1 < questLogSelectedIndex)
                return null;
            if (questLogSelectedIndex == -1)
                return null;
            return questLogEntries[questLogSelectedIndex];
        }
        /// <summary>
        ///  Function return selected historical quest
        /// </summary>
        /// <returns></returns>
        public QuestLogEntry GetSelectedQuestHistoryLogEntry()
        {
            if (questHistoryLogEntries.Count - 1 < questHistoryLogSelectedIndex)
                return null;
            if (questHistoryLogSelectedIndex == -1)
                return null;
            return questHistoryLogEntries[questHistoryLogSelectedIndex];
        }

        public void AbandonQuest()
        {
            if (questLogSelectedIndex == -1 || questLogSelectedIndex > questLogEntries.Count)
                return;
            NetworkAPI.SendTargetedCommand(ClientAPI.GetPlayerOid(), "/abandonQuest " + questLogEntries[questLogSelectedIndex].QuestId);
            UpdateQuestListSelected();
        }

        public QuestLogEntry GetQuestProgressInfo(int pos)
        {
            return questsInProgress[pos];
        }

        public bool CompleteQuest()
        {
            QuestLogEntry quest = questsInProgress[0];
            /*Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("senderOid", ClientAPI.GetPlayerOid());
            props.Add("questNPC", quest.NpcId);
            props.Add("questOID", quest.QuestId);
            props.Add("reward", rewardID);
            NetworkAPI.SendExtensionMessage(quest.NpcId.ToLong(), false, "ao.COMPLETE_QUEST", props);*/

            if (quest.gradeInfo[0].RewardItemsToChoose.Count > 0 && quest.itemChosen == -1)
                return false;

            NetworkAPI.SendTargetedCommand(quest.NpcId.ToLong(), "/completeQuest " + quest.QuestId + " " + quest.itemChosen);
            UpdateQuestListSelected();
            return true;
        }

        void _HandleQuestOfferResponse(Dictionary<string, object> props)
        {
            // update the information about the quests on offer from this npc
            questsOffered.Clear();
            int numQuests = (int)props["numQuests"];
            npcID = (OID)props["npcID"];
            for (int i = 0; i < numQuests; i++)
            {
                QuestLogEntry logEntry = new QuestLogEntry();
                questsOffered.Add(logEntry);
                logEntry.Title = (string)props["title" + i];
                logEntry.QuestId = (OID)props["questID" + i];
                logEntry.NpcId = npcID;
                logEntry.Description = (string)props["description" + i];
                logEntry.Objective = (string)props["objective" + i];
                //logEntry.Objectives.Clear ();
                //LinkedList<string> objectives = (LinkedList<string>)props ["objectives"];
                //foreach (string objective in objectives)
                //	logEntry.Objectives.Add (objective);
                logEntry.gradeCount = (int)props["grades" + i];
                logEntry.gradeInfo = new List<QuestGradeEntry>();
                //ClientAPI.Write("Quest grades: %s" % logEntry.grades)
                for (int j = 0; j < (logEntry.gradeCount + 1); j++)
                {
                    QuestGradeEntry gradeEntry = new QuestGradeEntry();
                    List<QuestRewardEntry> gradeRewards = new List<QuestRewardEntry>();

                    int numRewards = (int)props["rewards" + i + " " + j];
                    //               Debug.LogError("Quest " + logEntry.Title + " rewards count:" + numRewards);
                    for (int k = 0; k < numRewards; k++)
                    {
                        //id, name, icon, count = item;
                        QuestRewardEntry entry = new QuestRewardEntry();
                        entry.id = (int)props["rewards" + i + "_" + j + "_" + k];
                        AtavismInventoryItem item = gameObject.GetComponent<Inventory>().GetItemByTemplateID(entry.id);
                        entry.item = item;
                        entry.item.Count = (int)props["rewards" + i + "_" + j + "_" + k + "Count"];
                        gradeRewards.Add(entry);
                        //ClientAPI.Write("Reward: %s" % entry)
                    }
                    gradeEntry.rewardItems = gradeRewards;
                    // Items to choose from
                    List<QuestRewardEntry> gradeRewardsToChoose = new List<QuestRewardEntry>();
                    numRewards = (int)props["rewardsToChoose" + i + " " + j];
                    //              Debug.LogError("Quest " + logEntry.Title + " rewards Choose count:" + numRewards);
                    for (int k = 0; k < numRewards; k++)
                    {
                        //id, name, icon, count = item;
                        QuestRewardEntry entry = new QuestRewardEntry();
                        entry.id = (int)props["rewardsToChoose" + i + "_" + j + "_" + k];
                        AtavismInventoryItem item = gameObject.GetComponent<Inventory>().GetItemByTemplateID(entry.id);
                        entry.item = item;
                        entry.item.Count = (int)props["rewardsToChoose" + i + "_" + j + "_" + k + "Count"];
                        gradeRewardsToChoose.Add(entry);
                        //ClientAPI.Write("Reward to choose: %s" % entry)
                    }
                    gradeEntry.RewardItemsToChoose = gradeRewardsToChoose;

                    List<QuestRepRewardEntry> gradeRepReward = new List<QuestRepRewardEntry>();
                    numRewards = (int)props["rewardsRep" + i + " " + j];
                    //              Debug.LogError("Quest " + logEntry.Title + " rewards Choose count:" + numRewards);
                    for (int k = 0; k < numRewards; k++)
                    {
                        //id, name, icon, count = item;
                        QuestRepRewardEntry entry = new QuestRepRewardEntry();
                        entry.name = (string)props["rewardsRep" + i + "_" + j + "_" + k];
                        entry.count= (int)props["rewardsRep" + i + "_" + j + "_" + k + "Count"];
                        gradeRepReward.Add(entry);
                        //ClientAPI.Write("Reward to choose: %s" % entry)
                    }
                    gradeEntry.rewardRep = gradeRepReward;




                    // Quest Exp
                    gradeEntry.expReward = (int)props["xpReward" + i + " " + j];
                    // Currencies
                    List<QuestRewardEntry> currencies = new List<QuestRewardEntry>();
                    numRewards = (int)props["currencies" + i + " " + j];
                    for (int k = 0; k < numRewards; k++)
                    {
                        QuestRewardEntry entry = new QuestRewardEntry();
                        entry.id = (int)props["currency" + i + "_" + j + "_" + k];
                        entry.count = (int)props["currency" + i + "_" + j + "_" + k + "Count"];
                        currencies.Add(entry);
                        //ClientAPI.Write("Reward to choose: %s" % entry)
                    }
                    gradeEntry.currencies = currencies;
                    logEntry.gradeInfo.Add(gradeEntry);
                }
            }
            // dispatch a ui event to tell the rest of the system
            if (gameObject.GetComponent<NpcInteraction>().NpcId != npcID)
                gameObject.GetComponent<NpcInteraction>().InteractionOptions.Clear();
            gameObject.GetComponent<NpcInteraction>().NpcId = npcID;
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("QUEST_OFFERED_UPDATE", args);
        }

        /// <summary>
        /// Handles the Quest Log Update message, which has information about the current status 
        /// of Quests that the player is on.
        /// </summary>
        /// <param name="props">Properties.</param>
        void _HandleQuestLogInfo(Dictionary<string, object> props)
        {
            // update our idea of the state
            QuestLogEntry logEntry = null;
            long quest_id = (long)props["ext_msg_subject_oid"];
            OID questID = OID.fromLong(quest_id);
            foreach (QuestLogEntry entry in questLogEntries)
            {
                if (entry.QuestId.Equals(questID))
                {
                    logEntry = entry;
                    break;
                }
            }
            if (logEntry == null)
            {
                logEntry = new QuestLogEntry();
                questLogEntries.Add(logEntry);
            }
            logEntry.QuestId = questID;
            logEntry.Title = (string)props["title"];
            logEntry.Description = (string)props["description"];
            logEntry.Objective = (string)props["objective"];
            logEntry.Complete = (bool)props["complete"];
            logEntry.gradeCount = (int)props["grades"];
            logEntry.gradeInfo = new List<QuestGradeEntry>();
            for (int j = 0; j < (logEntry.gradeCount + 1); j++)
            {
                QuestGradeEntry gradeEntry = new QuestGradeEntry();
                // Objectives
                List<string> objectives = new List<string>();
                int numObjectives = (int)props["numObjectives" + j];
                for (int k = 0; k < numObjectives; k++)
                {
                    string objective = (string)props["objective" + j + "_" + k];
                    objectives.Add(objective);
                }
                gradeEntry.objectives = objectives;

                // Rewards
                List<QuestRewardEntry> gradeRewards = new List<QuestRewardEntry>();
                int numRewards = (int)props["rewards" + j];
                //           Debug.LogError("QuestLog " + logEntry.Title + " rewards count:" + numRewards);
                for (int k = 0; k < numRewards; k++)
                {
                    //id, name, icon, count = item;
                    QuestRewardEntry entry = new QuestRewardEntry();
                    entry.id = (int)props["rewards" + j + "_" + k];
                    AtavismInventoryItem item = gameObject.GetComponent<Inventory>().GetItemByTemplateID(entry.id);
                    entry.item = item;
                    entry.item.Count = (int)props["rewards" + j + "_" + k + "Count"];
                    gradeRewards.Add(entry);
                    //ClientAPI.Write("Reward: %s" % entry)
                }
                gradeEntry.rewardItems = gradeRewards;
                // Items to choose from
                List<QuestRewardEntry> gradeRewardsToChoose = new List<QuestRewardEntry>();
                numRewards = (int)props["rewardsToChoose" + j];
                //          Debug.LogError("QuestLog " + logEntry.Title + " rewards Choose count:" + numRewards);
                for (int k = 0; k < numRewards; k++)
                {
                    //id, name, icon, count = item;
                    QuestRewardEntry entry = new QuestRewardEntry();
                    entry.id = (int)props["rewardsToChoose" + j + "_" + k];
                    AtavismInventoryItem item = gameObject.GetComponent<Inventory>().GetItemByTemplateID(entry.id);
                    entry.item = item;
                    entry.item.Count = (int)props["rewardsToChoose" + j + "_" + k + "Count"];
                    gradeRewardsToChoose.Add(entry);
                    //ClientAPI.Write("Reward: %s" % entry)
                }
                gradeEntry.RewardItemsToChoose = gradeRewardsToChoose;
                List<QuestRepRewardEntry> gradeRepReward = new List<QuestRepRewardEntry>();
                numRewards = (int)props["rewardsRep" + j];
                //              Debug.LogError("Quest " + logEntry.Title + " rewards Choose count:" + numRewards);
                for (int k = 0; k < numRewards; k++)
                {
                    //id, name, icon, count = item;
                    QuestRepRewardEntry entry = new QuestRepRewardEntry();
                    entry.name = (string)props["rewardsRep" + j + "_" + k];
                    entry.count = (int)props["rewardsRep" + j + "_" + k + "Count"];
                    gradeRepReward.Add(entry);
                    //ClientAPI.Write("Reward to choose: %s" % entry)
                }
                gradeEntry.rewardRep = gradeRepReward;


                gradeEntry.expReward = (int)props["xpReward" + j];
                // Currencies
                List<QuestRewardEntry> currencies = new List<QuestRewardEntry>();
                numRewards = (int)props["currencies" + j];
                for (int k = 0; k < numRewards; k++)
                {
                    //id, name, icon, count = item;
                    QuestRewardEntry entry = new QuestRewardEntry();
                    entry.id = (int)props["currency" + j + "_" + k];
                    entry.count = (int)props["currency" + j + "_" + k + "Count"];
                    currencies.Add(entry);
                    //ClientAPI.Write("Reward: %s" % entry)
                }
                gradeEntry.currencies = currencies;
                logEntry.gradeInfo.Add(gradeEntry);
            }
            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("QUEST_LOG_UPDATE", args);
            UpdateQuestListSelected();

        }

        /// <summary>
        /// Handles the Quest History Log Update message, which has information about the Historicaly Quests.
        /// </summary>
        /// <param name="props">Properties.</param>
        void _HandleQuestHistoryLogInfo(Dictionary<string, object> props)
        {
            questHistoryLogEntries.Clear();
            for (int ii = 0; ii < (int)props["numQuests"]; ii++)
            {
                QuestLogEntry logEntry = new QuestLogEntry();
                long qId = (long)props["questId" + ii];
                OID questId = OID.fromLong(qId);
                logEntry.QuestId = questId;
                logEntry.Title = (string)props["title" + ii];
                logEntry.Description = (string)props["description" + ii];
                logEntry.Objective = (string)props["objective" + ii];
                logEntry.CompleteText = (string)props["complete" + ii];
                logEntry.reqLeval = (int)props["level" + ii];
                questHistoryLogEntries.Add(logEntry);
            }

            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("QUEST_HISTORY_LOG_UPDATE", args);
        }



        /// <summary>
        /// Handles the updates of the Quest State and updates the objectives in the players Quest Log
        /// to match.
        /// </summary>
        /// <param name="props">Properties.</param>
        void _HandleQuestStateInfo(Dictionary<string, object> props)
        {
            long quest_id = (long)props["ext_msg_subject_oid"];
            OID questID = OID.fromLong(quest_id);
            // update our idea of the state
            foreach (QuestLogEntry entry in questLogEntries)
            {
                if (!entry.QuestId.Equals(questID))
                    continue;
                for (int j = 0; j < (entry.gradeCount + 1); j++)
                {
                    // Objectives
                    List<string> objectives = new List<string>();
                    int numObjectives = (int)props["numObjectives" + j];
                    for (int k = 0; k < numObjectives; k++)
                    {
                        string objective = (string)props["objective" + j + "_" + k];
                        objectives.Add(objective);
                    }
                    entry.gradeInfo[j].objectives = objectives;
                }
            }

            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("QUEST_LOG_UPDATE", args);
        }

        void _HandleQuestProgressInfo(Dictionary<string, object> props)
        {
            /// update the information about the quests in progress from this npc
            questsInProgress.Clear();
            int numQuests = (int)props["numQuests"];
            npcID = (OID)props["npcID"];
            for (int i = 0; i < numQuests; i++)
            {
                QuestLogEntry logEntry = new QuestLogEntry();
                questsInProgress.Add(logEntry);
                logEntry.Title = (string)props["title" + i];
                logEntry.QuestId = (OID)props["questID" + i];
                logEntry.NpcId = npcID;
                //logEntry.Description = (string)props ["description" + i];
                logEntry.ProgressText = (string)props["progress" + i];
                logEntry.Complete = (bool)props["complete" + i];
                logEntry.Objective = (string)props["objective" + i];
                logEntry.gradeCount = (int)props["grades" + i];
                logEntry.gradeInfo = new List<QuestGradeEntry>();
                //ClientAPI.Write("Quest grades: %s" % logEntry.grades)
                for (int j = 0; j < (logEntry.gradeCount + 1); j++)
                {
                    QuestGradeEntry gradeEntry = new QuestGradeEntry();
                    List<QuestRewardEntry> gradeRewards = new List<QuestRewardEntry>();
                    int numRewards = (int)props["rewards" + i + " " + j];
                    for (int k = 0; k < numRewards; k++)
                    {
                        //id, name, icon, count = item;
                        QuestRewardEntry entry = new QuestRewardEntry();
                        entry.id = (int)props["rewards" + i + "_" + j + "_" + k];
                        AtavismInventoryItem item = gameObject.GetComponent<Inventory>().GetItemByTemplateID(entry.id);
                        entry.item = item;
                        entry.item.Count = (int)props["rewards" + i + "_" + j + "_" + k + "Count"];
                        gradeRewards.Add(entry);
                        //ClientAPI.Write("Reward: %s" % entry)
                    }
                    gradeEntry.rewardItems = gradeRewards;
                    // Items to choose from
                    List<QuestRewardEntry> gradeRewardsToChoose = new List<QuestRewardEntry>();
                    numRewards = (int)props["rewardsToChoose" + i + " " + j];
                    for (int k = 0; k < numRewards; k++)
                    {
                        //id, name, icon, count = item;
                        QuestRewardEntry entry = new QuestRewardEntry();
                        entry.id = (int)props["rewardsToChoose" + i + "_" + j + "_" + k];
                        AtavismInventoryItem item = gameObject.GetComponent<Inventory>().GetItemByTemplateID(entry.id);
                        entry.item = item;
                        entry.item.Count = (int)props["rewardsToChoose" + i + "_" + j + "_" + k + "Count"];
                        gradeRewardsToChoose.Add(entry);
                        //ClientAPI.Write("Reward to choose: %s" % entry)
                    }
                    gradeEntry.RewardItemsToChoose = gradeRewardsToChoose;
                    List<QuestRepRewardEntry> gradeRepReward = new List<QuestRepRewardEntry>();
                    numRewards = (int)props["rewardsRep" + i + " " + j];
                    //              Debug.LogError("Quest " + logEntry.Title + " rewards Choose count:" + numRewards);
                    for (int k = 0; k < numRewards; k++)
                    {
                        //id, name, icon, count = item;
                        QuestRepRewardEntry entry = new QuestRepRewardEntry();
                        entry.name = (string)props["rewardsRep" + i + "_" + j + "_" + k];
                        entry.count = (int)props["rewardsRep" + i + "_" + j + "_" + k + "Count"];
                        gradeRepReward.Add(entry);
                        //ClientAPI.Write("Reward to choose: %s" % entry)
                    }
                    gradeEntry.rewardRep = gradeRepReward;

                    if (props.ContainsKey("xpReward" + i + " " + j))
                    {
                        //                  Debug.LogError("Quest Progress xpReward" + i + " " + j + " ->" + props["xpReward" + i + " " + j]);
                        gradeEntry.expReward = (int)props["xpReward" + i + " " + j];
                    }
                    else
                        Debug.LogWarning("Quest Progress no xpReward");

                    // Currencies
                    List<QuestRewardEntry> currencies = new List<QuestRewardEntry>();
                    numRewards = (int)props["currencies" + i + " " + j];
                    for (int k = 0; k < numRewards; k++)
                    {
                        QuestRewardEntry entry = new QuestRewardEntry();
                        entry.id = (int)props["currency" + i + "_" + j + "_" + k];
                        entry.count = (int)props["currency" + i + "_" + j + "_" + k + "Count"];
                        currencies.Add(entry);
                        //ClientAPI.Write("Reward to choose: %s" % entry)
                    }
                    gradeEntry.currencies = currencies;
                    gradeEntry.completionText = (string)props["completion" + i + "_" + j];
                    logEntry.gradeInfo.Add(gradeEntry);
                }
            }
            //
            // dispatch a ui event to tell the rest of the system
            //
            if (gameObject.GetComponent<NpcInteraction>().NpcId != npcID)
                gameObject.GetComponent<NpcInteraction>().InteractionOptions.Clear();

            gameObject.GetComponent<NpcInteraction>().NpcId = npcID;
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("QUEST_PROGRESS_UPDATE", args);
            UpdateQuestListSelected();
        }

        void _HandleRemoveQuestResponse(Dictionary<string, object> props)
        {
            int index = 1; // questLogSelectedIndex is 1 based.
            long quest_id = (long)props["ext_msg_subject_oid"];
            OID questID = OID.fromLong(quest_id);
            foreach (QuestLogEntry entry in questLogEntries)
            {
                if (entry.QuestId.Equals(questID))
                {
                    questLogEntries.Remove(entry);
                    break;
                }
                index++;
            }
            if (index == questLogSelectedIndex)
            {
                // we removed the selected entry. reset selection
                questLogSelectedIndex = 0;
            }
            else if (index < questLogSelectedIndex)
            {
                // removed an entry before our selection - decrement our selection
                questLogSelectedIndex--;
            }

            // dispatch a ui event to tell the rest of the system
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("QUEST_LOG_UPDATE", args);
            UpdateQuestListSelected();
        }

        public void _HandleQuestEvent(Dictionary<string, object> props)
        {
            string eventType = (string)props["event"];
            int val1 = (int)props["val1"];
            int val2 = (int)props["val2"];
            int val3 = (int)props["val3"];
            string data = (string)props["data"];

#if AT_I2LOC_PRESET
        if (data.IndexOf(" killed:") != -1) {
            string objectivesNames = I2.Loc.LocalizationManager.GetTranslation("Quests/" + data.Remove(data.IndexOf(" killed:")));
            //    string objectivesValues = data.Remove(0, data.LastIndexOf(':') < 0 ? 0 : data.LastIndexOf(':')+1);
            data = objectivesNames + " " + I2.Loc.LocalizationManager.GetTranslation("killed") + ": " + val2 + "/" + val3 + (val2 == val3 ? " (" + I2.Loc.LocalizationManager.GetTranslation("Complete") + ")" : "");
        }
        if (data.IndexOf(" collected:") != -1) {
            string objectivesNames = I2.Loc.LocalizationManager.GetTranslation("Quests/" + data.Remove(data.IndexOf(" collected:")));
            //    string objectivesValues = data.Remove(0, data.LastIndexOf(':') < 0 ? 0 : data.LastIndexOf(':')+1);
            data = objectivesNames + " " + I2.Loc.LocalizationManager.GetTranslation("collected") + ": " + val2 + "/" + val3 + (val2 == val3 ? " (" + I2.Loc.LocalizationManager.GetTranslation("Complete") + ")" : "");
        }
#endif
            //string errorMessage = eventType;
            if (eventType == "QuestProgress")
            {
                // dispatch a ui event to tell the rest of the system
                string[] args = new string[1];
                args[0] = data;
                AtavismEventSystem.DispatchEvent("ANNOUNCEMENT", args);
            }
        }

        #region Properties
        public static Quests Instance
        {
            get
            {
                return instance;
            }
        }

        public List<QuestLogEntry> QuestLogEntries
        {
            get
            {
                return questLogEntries;
            }
        }
        public List<QuestLogEntry> QuestHistoryLogEntries
        {
            get
            {
                return questHistoryLogEntries;
            }
        }
        #endregion Properties


        //  List<GameObject> qObjects = new List<GameObject>();
        OID questOnMinimap;

        public void ClickedQuest(QuestLogEntry quest)
        {
            /*   Scene aScene = SceneManager.GetActiveScene();
               List<string> teleports = new List<string>();
               string[] cords;
               GameObject obj;
               string qCordsObjects = "";
               foreach (GameObject qo in qObjects) {
                   Destroy(qo);
               }
               questOnMinimap = quest.QuestId;
               qObjects.Clear();
               for (int i = 0; i < quest.gradeInfo[0].objectives.Count; i++) {
                   qCordsObjects = I2.Loc.LocalizationManager.GetTranslation("QuestCoords/" + quest.Title + i);
                   //   Debug.LogError(qCordsObjects);
                   if (!string.IsNullOrEmpty(qCordsObjects)) {
                       string[] cLoc = qCordsObjects.Split('&');
                       for (int ii = 0; ii < cLoc.Length; ii++) {
                           cords = cLoc[ii].Split('|');
                           if (aScene.name.Equals(cords[0])) {
                               obj = new GameObject(quest.QuestId + "Obj" + i + "_" + ii);
                               obj.transform.localPosition = new Vector3(float.Parse(cords[1]), float.Parse(cords[2]), float.Parse(cords[3]));
                               bl_MiniMapItem mmi = obj.AddComponent<bl_MiniMapItem>();
                               mmi.Target = obj.transform;
                               mmi.Icon = GameSettings.Instance.MinimapSettings.minimapQuestMobArea;
                               mmi.IconColor = Color.blue;
                               mmi.Size = cords.Length > 4 ? float.Parse(cords[4]) : 35;
                               mmi.InfoItem = "";
                               qObjects.Add(obj);
                           } else if (teleports.IndexOf(aScene.name + "_" + cords[0]) == -1) {
                               qCordsObjects = I2.Loc.LocalizationManager.GetTranslation("QuestCoords/" + aScene.name + "_" + cords[0]);
                               if (!string.IsNullOrEmpty(qCordsObjects)) {
                                   string[] cordsTelep = qCordsObjects.Split('|');
                                   obj = new GameObject(quest.QuestId + "Obj" + i + "_" + ii);
                                   obj.transform.localPosition = new Vector3(float.Parse(cordsTelep[0]), float.Parse(cordsTelep[1]), float.Parse(cordsTelep[2]));
                                   bl_MiniMapItem mmi = obj.AddComponent<bl_MiniMapItem>();
                                   mmi.Target = obj.transform;
                                   mmi.Icon = GameSettings.Instance.MinimapSettings.minimapQuestTarget;
                                   mmi.IconColor = Color.blue;
                                   mmi.Size = 20;
                                   mmi.InfoItem = "";
                                   qObjects.Add(obj);
                                   teleports.Add(aScene.name + "_" + cords[0]);
                               } else {
                                   Debug.LogError("No Cords for " + aScene.name + "_" + cords[0]);
                               }
                           }
                       }
                   } else {
                       Debug.LogError("No Cords for " + quest.Title + i);
                   }
               }
               qCordsObjects = I2.Loc.LocalizationManager.GetTranslation("QuestCoords/" + quest.Title);
               if (!string.IsNullOrEmpty(qCordsObjects)) {
                   cords = qCordsObjects.Split('|');
                   obj = new GameObject(quest.QuestId + "Target");
                   if (aScene.name.Equals(cords[0])) {
                       obj.transform.localPosition = new Vector3(float.Parse(cords[1]), float.Parse(cords[2]), float.Parse(cords[3]));
                       bl_MiniMapItem mmi = obj.AddComponent<bl_MiniMapItem>();
                       mmi.Target = obj.transform;
                       if (cords.Length > 4 && !string.IsNullOrEmpty(cords[4])) {
                           mmi.Icon = GameSettings.Instance.MinimapSettings.minimapQuestMobArea;
                           mmi.Size = float.Parse(cords[4]);

                       } else {
                           mmi.Icon = GameSettings.Instance.MinimapSettings.minimapQuestTarget;
                           mmi.Size = 20;
                       }
                       mmi.IconColor = Color.blue;
                       mmi.InfoItem = "";
                       qObjects.Add(obj);
                   } else if (teleports.IndexOf(aScene.name + "_" + cords[0]) == -1) {
                       qCordsObjects = I2.Loc.LocalizationManager.GetTranslation("QuestCoords/" + aScene.name + "_" + cords[0]);
                       if (!string.IsNullOrEmpty(qCordsObjects)) {
                           string[] cordsTelep = qCordsObjects.Split('|');
                           obj.transform.localPosition = new Vector3(float.Parse(cordsTelep[0]), float.Parse(cordsTelep[1]), float.Parse(cordsTelep[2]));
                           bl_MiniMapItem mmi = obj.AddComponent<bl_MiniMapItem>();
                           mmi.Target = obj.transform;
                           mmi.Icon = GameSettings.Instance.MinimapSettings.minimapQuestTarget;
                           mmi.IconColor = Color.blue;
                           mmi.Size = 20;
                           mmi.InfoItem = "";
                           qObjects.Add(obj);
                           teleports.Add(aScene.name + "_" + cords[0]);
                       } else {
                           Debug.LogError("No Cords for " + aScene.name + "_" + cords[0]);
                       }
                   }
               } else {
                   Debug.LogError("No Cords for " + quest.Title);
               }
               */
        }


        void UpdateQuestListSelected()
        {
            bool jestQ;
            //     bool isQuestOnMinimap = false;
        
            List<long> toRemove = new List<long>();
            if (AtavismSettings.Instance != null && AtavismSettings.Instance.GetQuestListSelected() != null )
            {
                if (!AtavismSettings.Instance.GetQuestListSelected().ContainsKey(ClientAPI.GetPlayerOid()))
                    AtavismSettings.Instance.GetQuestListSelected().Add(ClientAPI.GetPlayerOid(), new List<long>());

                foreach (long id in AtavismSettings.Instance.GetQuestListSelected()[ClientAPI.GetPlayerOid()])
                {
                    jestQ = false;
                    foreach (QuestLogEntry q in questLogEntries)
                    {
                        if (q.QuestId.ToLong() == id)
                        {
                            jestQ = true;
                            break;
                        }
                    }
                    if (!jestQ)
                        toRemove.Add(id);
                }
                foreach (long id in toRemove)
                {
                    AtavismSettings.Instance.GetQuestListSelected()[ClientAPI.GetPlayerOid()].Remove(id);
                }
            }
            /*
                    foreach (QuestLogEntry q in questLogEntries) {
                        if (q.QuestId.Equals(questOnMinimap))
                            isQuestOnMinimap = true;
                    }
                    Debug.LogError("isQuestOnMinimap->" + isQuestOnMinimap);
                    if (!isQuestOnMinimap) {
                        foreach (GameObject qo in qObjects) {
                            Destroy(qo);
                        }
                        qObjects.Clear();
                    }
                    */
        }
        public int GetMaxQuestsSelected
        {
            get
            {
                return maxQuestsSelected;
            }
        }




    }
}