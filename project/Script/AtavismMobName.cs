using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Atavism
{

    public class AtavismMobName : MonoBehaviour
    {
        [SerializeField] float height = 2f;
        GameObject mobName;
        GameObject mobQuest;
        GameObject mob;
        [SerializeField] string textfield;
        [SerializeField] float textheight;
        [SerializeField] float renderDistance = 50f;
        [SerializeField] float minFontSize = 1f;
        [SerializeField] float maxFontSize = 5f;
        //  [SerializeField] Color friendlyNameColour = Color.green;
        //   [SerializeField] Color neutralNameColour = Color.yellow;
        //   [SerializeField] Color enemyNameColour = Color.red;
        //   [SerializeField] Color myDamageColour = Color.red;
        //   [SerializeField] Color targetDamageColour = Color.white;
        //   [SerializeField] Color myMessageColour = Color.yellow;
        //   [SerializeField] Color targetMessageColour = Color.yellow;
        [SerializeField] bool nameOnSelf = false;
        AtavismNode node;
        //  bool deadstate = false;
        //   bool upadteIsRunning = false;
        string mName = "";
        //   Coroutine chIcon;
        //  Coroutine chTimer;
        bool death = false;
        bool showTitle = false;
        // Use this for initialization
        void Start()
        {
            node = GetComponent<AtavismNode>();
            mob = new GameObject("mob");
            mob.layer = LayerMask.NameToLayer(AtavismCursor.Instance.layerForTexts);
            mob.transform.SetParent(transform, false);
            mobName = new GameObject("mobName");
            mobName.layer = LayerMask.NameToLayer(AtavismCursor.Instance.layerForTexts);
            mobName.transform.SetParent(mob.transform, false);
            if (transform.gameObject.GetComponent<CharacterController>() != null)
                height = transform.gameObject.GetComponent<CharacterController>().height > 2 * transform.gameObject.GetComponent<CharacterController>().radius ? transform.gameObject.GetComponent<CharacterController>().height : transform.gameObject.GetComponent<CharacterController>().radius * 2;
            else
                height = textheight;
            mob.transform.localPosition = new Vector3(0f, height, 0f);
            mobName.transform.localPosition = AtavismSettings.Instance.MobNamePosition;
            TextMeshPro textMeshPro = mobName.AddComponent<TextMeshPro>();
            textMeshPro.alignment = AtavismSettings.Instance.MobNameAlignment;// TextAlignmentOptions.Midline;
            textMeshPro.margin = AtavismSettings.Instance.MobNameMargin;
            textMeshPro.fontSize = AtavismSettings.Instance.MobNameFontSize;

            textMeshPro.color = AtavismSettings.Instance.MobNameDefaultColor;
            if (AtavismSettings.Instance.MobNameFont == null)
            {
                TMP_FontAsset font1 = Resources.Load("Lato-BoldSDFNames", typeof(TMP_FontAsset)) as TMP_FontAsset;
                mobName.GetComponent<TextMeshPro>().font = font1;
            }
            else
            {
                mobName.GetComponent<TextMeshPro>().font = AtavismSettings.Instance.MobNameFont;
            }
            mobName.GetComponent<TextMeshPro>().outlineWidth = AtavismSettings.Instance.MobNameOutlineWidth;
            mobQuest = new GameObject("mobQuest");
            mobQuest.layer = LayerMask.NameToLayer(AtavismCursor.Instance.layerForTexts);
            mobQuest.transform.SetParent(mobName.transform, false);
            mobQuest.transform.localPosition = AtavismSettings.Instance.GetNpcInfoTextPosition;
            TextMeshPro textMeshProQuest = mobQuest.AddComponent<TextMeshPro>();
            textMeshProQuest.alignment = TextAlignmentOptions.Midline;
            textMeshProQuest.fontSize = AtavismSettings.Instance.GetNpcInfoTextSize;
            textMeshProQuest.color = AtavismSettings.Instance.GetNpcInfoTextColor;
            textMeshProQuest.text = "";
            textMeshProQuest.spriteAsset = AtavismSettings.Instance.GetSpriteAsset;


            node = GetComponent<AtavismNode>();
            if (node != null)
            {
                node.RegisterObjectPropertyChangeHandler("title", LevelHandler);
                node.RegisterObjectPropertyChangeHandler("subTitle", LevelHandler);
                node.RegisterObjectPropertyChangeHandler("level", LevelHandler);
                node.RegisterObjectPropertyChangeHandler("reaction", TargetTypeHandler);
                node.RegisterObjectPropertyChangeHandler("aggressive", TargetTypeHandler);
                node.RegisterObjectPropertyChangeHandler("adminLevel", AdminLevelHandler);
                node.RegisterObjectPropertyChangeHandler("questavailable", QuestAvailableHandler);
                node.RegisterObjectPropertyChangeHandler("questinprogress", QuestInProgressHandler);
                node.RegisterObjectPropertyChangeHandler("questconcludable", QuestConcludableHandler);
                node.RegisterObjectPropertyChangeHandler("dialogue_available", DialogueAvailableHandler);
                node.RegisterObjectPropertyChangeHandler("itemstosell", ItemsToSellHandler);
                node.RegisterObjectPropertyChangeHandler("nameDisplay", NameDisplayHandler);
                node.RegisterObjectPropertyChangeHandler("guildName", GuildNameDisplayHandler);
                node.RegisterObjectPropertyChangeHandler("deadstate", HandleDeadState);
                node.RegisterObjectPropertyChangeHandler("bankteller", BankHandler);
#if AT_I2LOC_PRESET
                if (GetComponent<AtavismNode>().PropertyExists("displayName"))
                {
                    if (!string.IsNullOrEmpty(I2.Loc.LocalizationManager.GetTranslation("Mobs/" + GetComponent<AtavismNode>().GetProperty("displayName"))))
                        mName = I2.Loc.LocalizationManager.GetTranslation("Mobs/" + GetComponent<AtavismNode>().GetProperty("displayName"));
                    else
                        mName = (string)GetComponent<AtavismNode>().GetProperty("displayName");
                }
                else
                {
                    if (!string.IsNullOrEmpty(I2.Loc.LocalizationManager.GetTranslation("Mobs/" + GetComponent<AtavismNode>().name)))
                        mName = I2.Loc.LocalizationManager.GetTranslation("Mobs/" + GetComponent<AtavismNode>().name);
                    else
                        mName = GetComponent<AtavismNode>().name;
                }
              
#else
                if (GetComponent<AtavismNode>().PropertyExists("displayName")&& !string.IsNullOrEmpty((string)GetComponent<AtavismNode>().GetProperty("displayName")))
                {
                    mName = (string)GetComponent<AtavismNode>().GetProperty("displayName");
                }
                else
                {
                    mName = GetComponent<AtavismNode>().name;
                }
#endif
            }
            AtavismEventSystem.RegisterEvent("UPDATE_LANGUAGE", this);
            UpdateNameDisplay(true);
            if (!AtavismSettings.Instance.NameVisable)
            {
                textMeshPro.enabled = false;
                textMeshProQuest.enabled = false;
            }
            StartCoroutine(UpdateTimer());
            StartCoroutine(UpdateIcon());
        }

        public void HandleDeadState(object sender, PropertyChangeEventArgs args)
        {
            death = (bool)node.GetProperty("deadstate");
            UpdateNameDisplay(true);
        }

        public void LevelHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateNameDisplay(true);
        }
        public void BankHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateNameDisplay(true);
        }

        public void TargetTypeHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateNameDisplay(true);
        }
        public void AdminLevelHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateNameDisplay(true);
        }
        public void NameDisplayHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateNameDisplay(true);
        }
        public void GuildNameDisplayHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateNameDisplay(true);
        }
        public void QuestAvailableHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateNameDisplay(true);
        }
        public void QuestInProgressHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateNameDisplay(true);
        }
        public void QuestConcludableHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateNameDisplay(true);
        }
        public void DialogueAvailableHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateNameDisplay(true);
        }
        public void ItemsToSellHandler(object sender, PropertyChangeEventArgs args)
        {
            UpdateNameDisplay(true);
        }
        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "UPDATE_LANGUAGE")
            {
                UpdateNameDisplay(true);
#if AT_I2LOC_PRESET
             if (GetComponent<AtavismNode>().PropertyExists("displayName"))
                {
                    if (!string.IsNullOrEmpty(I2.Loc.LocalizationManager.GetTranslation("Mobs/" + GetComponent<AtavismNode>().GetProperty("displayName"))))
                        mName = I2.Loc.LocalizationManager.GetTranslation("Mobs/" + GetComponent<AtavismNode>().GetProperty("displayName"));
                    else
                        mName = (string)GetComponent<AtavismNode>().GetProperty("displayName");
                }
                else
                {
                    if (!string.IsNullOrEmpty(I2.Loc.LocalizationManager.GetTranslation("Mobs/" + GetComponent<AtavismNode>().name)))
                        mName = I2.Loc.LocalizationManager.GetTranslation("Mobs/" + GetComponent<AtavismNode>().name);
                    else
                        mName = GetComponent<AtavismNode>().name;
                }
#endif
            }
        }

        IEnumerator UpdateIcon()
        {
            WaitForSeconds delay = new WaitForSeconds(1f);
            while (true)
            {
                if (!SceneManager.GetActiveScene().name.Equals("Login") && !SceneManager.GetActiveScene().name.Equals(ClientAPI.Instance.characterSceneName))
                    if (mobQuest != null && node != null)
                    {
                        if (node.PropertyExists("mobType") && (int)node.GetProperty("mobType") == 2)
                        {
                            if (GetComponent<bl_MiniMapItem>() != null)
                            {
                                bl_MiniMapItem item = GetComponent<bl_MiniMapItem>();
                                item.SetIcon(AtavismSettings.Instance.MinimapSettings.minimapBossIcon);
                                item.Size = AtavismSettings.Instance.MinimapSettings.minimapBossIconSize;
                                item.IconColor = AtavismSettings.Instance.MinimapSettings.minimapBossIconColor;
                                item.InfoItem = "";
                                item.DestroyWithObject = true;
                            }
                            else
                            {
                                bl_MiniMapItem item = gameObject.AddComponent<bl_MiniMapItem>();
                                item.Icon = AtavismSettings.Instance.MinimapSettings.minimapBossIcon;
                                item.Size = AtavismSettings.Instance.MinimapSettings.minimapBossIconSize;
                                item.IconColor = AtavismSettings.Instance.MinimapSettings.minimapBossIconColor;
                                item.InfoItem = "";
                                item.DestroyWithObject = true;
                            }
                        }
                        else if (node.CheckBooleanProperty("questconcludable"))
                        {
                            if (mobQuest.GetComponent<TextMeshPro>() != null)
                                mobQuest.GetComponent<TextMeshPro>().text = AtavismSettings.Instance.QuestConcludableText;
                            if (GetComponent<bl_MiniMapItem>() != null)
                            {
                                bl_MiniMapItem item = GetComponent<bl_MiniMapItem>();
                                item.SetIcon(AtavismSettings.Instance.MinimapSettings.minimapQuestConcludableIcon);
                                item.Size = AtavismSettings.Instance.MinimapSettings.minimapQuestConcludableIconSize;
                                item.IconColor = AtavismSettings.Instance.MinimapSettings.minimapQuestConcludableIconColor;
                                item.InfoItem = "";
                                item.DestroyWithObject = true;
                            }
                            else
                            {
                                bl_MiniMapItem item = gameObject.AddComponent<bl_MiniMapItem>();
                                item.Icon = AtavismSettings.Instance.MinimapSettings.minimapQuestConcludableIcon;
                                item.Size = AtavismSettings.Instance.MinimapSettings.minimapQuestConcludableIconSize;
                                item.IconColor = AtavismSettings.Instance.MinimapSettings.minimapQuestConcludableIconColor;
                                item.InfoItem = "";
                                item.DestroyWithObject = true;
                            }
                        }
                        else if (node.CheckBooleanProperty("questinprogress"))
                        {
                            if (mobQuest.GetComponent<TextMeshPro>() != null)
                                mobQuest.GetComponent<TextMeshPro>().text = AtavismSettings.Instance.QuestProgressText;
                            if (GetComponent<bl_MiniMapItem>() != null)
                            {
                                bl_MiniMapItem item = GetComponent<bl_MiniMapItem>();
                                item.SetIcon(AtavismSettings.Instance.MinimapSettings.minimapQuestProgressIcon);
                                item.Size = AtavismSettings.Instance.MinimapSettings.minimapQuestProgressIconSize;
                                item.IconColor = AtavismSettings.Instance.MinimapSettings.minimapQuestProgressIconColor;
                                item.InfoItem = ""; 
                                item.DestroyWithObject = true;
                            }
                            else
                            {
                                bl_MiniMapItem item = gameObject.AddComponent<bl_MiniMapItem>();
                                item.Icon = AtavismSettings.Instance.MinimapSettings.minimapQuestProgressIcon;
                                item.Size = AtavismSettings.Instance.MinimapSettings.minimapQuestProgressIconSize;
                                item.IconColor = AtavismSettings.Instance.MinimapSettings.minimapQuestProgressIconColor;
                                item.InfoItem = "";
                                item.DestroyWithObject = true; 
                            }
                        }
                        else if (node.CheckBooleanProperty("questavailable"))
                        {
                            if (mobQuest.GetComponent<TextMeshPro>() != null)
                                mobQuest.GetComponent<TextMeshPro>().text = AtavismSettings.Instance.QuestNewText;
                            if (GetComponent<bl_MiniMapItem>() != null)
                            {
                                bl_MiniMapItem item = GetComponent<bl_MiniMapItem>();
                                item.Icon = AtavismSettings.Instance.MinimapSettings.minimapQuestAvailableIcon;
                                item.Size = AtavismSettings.Instance.MinimapSettings.minimapQuestAvailableIconSize;
                                item.IconColor = AtavismSettings.Instance.MinimapSettings.minimapQuestAvailableIconColor;
                                item.InfoItem = "";
                                item.DestroyWithObject = true;
                            }
                            else
                            {
                                bl_MiniMapItem item = gameObject.AddComponent<bl_MiniMapItem>();
                                item.Icon = AtavismSettings.Instance.MinimapSettings.minimapQuestAvailableIcon;
                                item.Size = AtavismSettings.Instance.MinimapSettings.minimapQuestAvailableIconSize;
                                item.IconColor = AtavismSettings.Instance.MinimapSettings.minimapQuestAvailableIconColor;
                                item.InfoItem = "";
                                item.DestroyWithObject = true;
                            }
                        }
                        else if (node.CheckBooleanProperty("itemstosell"))
                        {
                            if (mobQuest.GetComponent<TextMeshPro>() != null)
                                mobQuest.GetComponent<TextMeshPro>().text = AtavismSettings.Instance.ShopText;
                            if (GetComponent<bl_MiniMapItem>() != null)
                            {
                                bl_MiniMapItem item = GetComponent<bl_MiniMapItem>();
                                item.SetIcon(AtavismSettings.Instance.MinimapSettings.minimapShopIcon);
                                item.Size = AtavismSettings.Instance.MinimapSettings.minimapShopIconSize;
                                item.IconColor = AtavismSettings.Instance.MinimapSettings.minimapShopIconColor;
                                item.InfoItem = "";
                                item.DestroyWithObject = true;
                            }
                            else
                            {
                                bl_MiniMapItem item = gameObject.AddComponent<bl_MiniMapItem>();
                                item.Icon = AtavismSettings.Instance.MinimapSettings.minimapShopIcon;
                                item.Size = AtavismSettings.Instance.MinimapSettings.minimapShopIconSize;
                                item.IconColor = AtavismSettings.Instance.MinimapSettings.minimapShopIconColor;
                                item.InfoItem = "";
                                item.DestroyWithObject = true;
                            }
                        }
                        else if (node.CheckBooleanProperty("bankteller"))
                        {
                            if (mobQuest.GetComponent<TextMeshPro>() != null)
                                mobQuest.GetComponent<TextMeshPro>().text = AtavismSettings.Instance.BankText;
                            if (GetComponent<bl_MiniMapItem>() != null)
                            {
                                bl_MiniMapItem item = GetComponent<bl_MiniMapItem>();
                                item.SetIcon(AtavismSettings.Instance.MinimapSettings.minimapBankIcon);
                                item.Size = AtavismSettings.Instance.MinimapSettings.minimapBankIconSize;
                                item.SetColor(AtavismSettings.Instance.MinimapSettings.minimapBankIconColor);
                                item.InfoItem = "";
                                item.DestroyWithObject = true;
                            }
                            else
                            {
                                bl_MiniMapItem item = gameObject.AddComponent<bl_MiniMapItem>();
                                item.Icon = AtavismSettings.Instance.MinimapSettings.minimapBankIcon;
                                item.Size = AtavismSettings.Instance.MinimapSettings.minimapBankIconSize;
                                item.SetColor(AtavismSettings.Instance.MinimapSettings.minimapBankIconColor);
                                item.InfoItem = "";
                                item.DestroyWithObject = true;
                            }
                        }
                        else
                        {
                            if (mobQuest.GetComponent<TextMeshPro>() != null)
                                mobQuest.GetComponent<TextMeshPro>().text = "";
                            if (!node.Oid.Equals(ClientAPI.GetPlayerOid()))
                                if (GetComponent<bl_MiniMapItem>() != null)
                                {
                                    bl_MiniMapItem item = GetComponent<bl_MiniMapItem>();
                                    item.SetIcon(AtavismSettings.Instance.MinimapSettings.minimapIcon);
                                    item.Size = AtavismSettings.Instance.MinimapSettings.minimapIconSize;
                                    item.IconColor = AtavismSettings.Instance.MinimapSettings.minimapIconColor;
                                    item.DestroyWithObject = true;
                                }
                                else
                                {
                                    bl_MiniMapItem item = gameObject.AddComponent<bl_MiniMapItem>();
                                    item.Icon = AtavismSettings.Instance.MinimapSettings.minimapIcon;
                                    item.Size = AtavismSettings.Instance.MinimapSettings.minimapIconSize;
                                    item.IconColor = AtavismSettings.Instance.MinimapSettings.minimapIconColor;
                                    item.InfoItem = "";
                                    item.DestroyWithObject = true;
                                }
                        }
                        if (node.CheckBooleanProperty("deadstate"))
                        {
                            mobName.GetComponent<TextMeshPro>().enabled = false;
                            mobQuest.GetComponent<TextMeshPro>().enabled = false;
                            if (GetComponent<bl_MiniMapItem>() != null)
                            {
                                bl_MiniMapItem item = GetComponent<bl_MiniMapItem>();
                                item.SetColor(Color.red);
                                item.DestroyWithObject = true;

                            }
                        }
                    }
                UpdateNameDisplay(true);
                yield return delay;
            }
        }

        IEnumerator UpdateTimer()
        {
            WaitForSeconds delay = new WaitForSeconds(0.02f);
            //     upadteIsRunning = true;
            while (Camera.main == null)
            {
                yield return delay;
            }
            while (Camera.main != null)
            {
                if (mobName != null)
                {
                    float distance = Vector3.Distance(mobName.transform.position, Camera.main.transform.position);
                    if (distance < renderDistance)
                    {
                        if (!death)
                        {
                            mobName.GetComponent<TextMeshPro>().enabled = true;
                            mobName.transform.rotation = Camera.main.transform.rotation;
                            mobName.GetComponent<TextMeshPro>().fontSize = minFontSize + distance * (maxFontSize - minFontSize) / renderDistance;
                        }
                    }
                    else
                        mobName.GetComponent<TextMeshPro>().enabled = false;
                }
                yield return delay;
            }
            //    upadteIsRunning = false;
            yield return 0f;
        }

        void UpdateNameDisplay(bool showName)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Equals(ClientAPI.Instance.characterSceneName))
                return;
            string ownerName = "";
            if (node != null)
            {

                if (node.PropertyExists("nameDisplay") && !node.CheckBooleanProperty("nameDisplay"))
                {
                    showName = false;
                }
                if (node.PropertyExists("pet") && node.CheckBooleanProperty("pet"))
                {
                    if (node.PropertyExists("petOwner"))
                    {
                        OID owner = (OID)node.GetProperty("petOwner");
                        ownerName = " (" + ClientAPI.GetObjectNode(owner.ToLong()).Name + ")";
                    }
                }

            }
            string text = "";
            if (showName && node != null)
            {
                // Display a mobs SubTile if one is set in Mob Editor plugin.
                string SubTitle = "";
                if (node.PropertyExists("subTitle"))
                    SubTitle = (string)node.GetProperty("subTitle");
                if (SubTitle != null && SubTitle != "")
                    SubTitle = "\n<" + SubTitle + ">";
                string title = "";
                if (showTitle)
                {
                    if (node.PropertyExists("title"))
                        title = (string)node.GetProperty("title");
                    if (title != null && title != "")
                        title = "\n<" + title + ">";
                }
                string species = "";
                if (node.PropertyExists("species"))
                    species = (string)node.GetProperty("species");
                if (species != null && species != "")
                    species = "\n" + species + "";
                // Display GuildName
                string guildName = "";
                if (node.PropertyExists("guildName"))
                    guildName = (string)node.GetProperty("guildName");
                if (guildName != null && guildName != "")
                    guildName = "\n<" + guildName + ">";

                text = "<#ffff00>";
                if (node.PropertyExists("reaction"))
                {
                    int targetType = (int)node.GetProperty("reaction");
                    if (node.PropertyExists("aggressive"))
                    {
                        if ((bool)node.GetProperty("aggressive"))
                        {
                            targetType = -1;
                        }
                    }
                    if (targetType < 0)
                    {
                        text = "<#ff0000>";
                    }
                    else if (targetType > 0)
                    {
                        text = "<#00ff00>";
                    }
                }

                if (nameOnSelf && node != null && node.Oid.Equals(ClientAPI.GetPlayerOid()))
                {
                    text = "<#00ff00>";
                }
                text = text + mName + ownerName + "</color>" + SubTitle + title+ guildName;
                if (mobName != null && node.PropertyExists("reaction"))
                    mobName.GetComponent<TextMeshPro>().text = text;
            }
            else
            {
                text = "<#00ff00>" + textfield + "</color>";
            }

            if (nameOnSelf || node != null && !node.Oid.Equals(ClientAPI.GetPlayerOid()))
            {
                if (mobName != null && mobName.GetComponent<TextMeshPro>() != null)
                    mobName.GetComponent<TextMeshPro>().text = text;
            }
            else
            {
                if (mobName != null && mobName.GetComponent<TextMeshPro>() != null)
                    mobName.GetComponent<TextMeshPro>().text = "";
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (node == null)
            {
                node = GetComponent<AtavismNode>();
                if (node != null)
                {
                    node.RegisterObjectPropertyChangeHandler("title", LevelHandler);
                    node.RegisterObjectPropertyChangeHandler("level", LevelHandler);
                    node.RegisterObjectPropertyChangeHandler("reaction", TargetTypeHandler);
                    node.RegisterObjectPropertyChangeHandler("aggressive", TargetTypeHandler);
                    node.RegisterObjectPropertyChangeHandler("adminLevel", AdminLevelHandler);
                    node.RegisterObjectPropertyChangeHandler("questavailable", QuestAvailableHandler);
                    node.RegisterObjectPropertyChangeHandler("questinprogress", QuestInProgressHandler);
                    node.RegisterObjectPropertyChangeHandler("questconcludable", QuestConcludableHandler);
                    node.RegisterObjectPropertyChangeHandler("dialogue_available", DialogueAvailableHandler);
                    node.RegisterObjectPropertyChangeHandler("itemstosell", ItemsToSellHandler);
                    node.RegisterObjectPropertyChangeHandler("nameDisplay", NameDisplayHandler);
                    node.RegisterObjectPropertyChangeHandler("guildName", GuildNameDisplayHandler);
                    node.RegisterObjectPropertyChangeHandler("deadstate", HandleDeadState);
                    node.RegisterObjectPropertyChangeHandler("bankteller", HandleDeadState);


                }
            }
            if (mob == null)
            {
                mob = new GameObject("mob");
                mob.layer = LayerMask.NameToLayer(AtavismCursor.Instance.layerForTexts);
                mob.transform.SetParent(transform, false);
                mobName = new GameObject("mobName");
                mobName.layer = LayerMask.NameToLayer(AtavismCursor.Instance.layerForTexts);
                mobName.transform.SetParent(mob.transform, false);
                if (transform.gameObject.GetComponent<CharacterController>() != null)
                    height = transform.gameObject.GetComponent<CharacterController>().height > 2 * transform.gameObject.GetComponent<CharacterController>().radius ? transform.gameObject.GetComponent<CharacterController>().height : transform.gameObject.GetComponent<CharacterController>().radius * 2;
                else
                    height = textheight;
                mob.transform.localPosition = new Vector3(0f, height, 0f);


                mobName.transform.localPosition = AtavismSettings.Instance.MobNamePosition;
                TextMeshPro textMeshPro = mobName.AddComponent<TextMeshPro>();
                textMeshPro.alignment = AtavismSettings.Instance.MobNameAlignment;// TextAlignmentOptions.Midline;
                textMeshPro.margin = AtavismSettings.Instance.MobNameMargin;
                textMeshPro.fontSize = AtavismSettings.Instance.MobNameFontSize;

                textMeshPro.color = AtavismSettings.Instance.MobNameDefaultColor;
                if (AtavismSettings.Instance.MobNameFont == null)
                {
                    TMP_FontAsset font1 = Resources.Load("Lato-BoldSDFNames", typeof(TMP_FontAsset)) as TMP_FontAsset;
                    mobName.GetComponent<TextMeshPro>().font = font1;
                }
                else
                {
                    mobName.GetComponent<TextMeshPro>().font = AtavismSettings.Instance.MobNameFont;
                }
                mobName.GetComponent<TextMeshPro>().outlineWidth = AtavismSettings.Instance.MobNameOutlineWidth;
                mobQuest = new GameObject("mobQuest");
                mobQuest.layer = LayerMask.NameToLayer(AtavismCursor.Instance.layerForTexts);
                mobQuest.transform.SetParent(mobName.transform, false);
                mobQuest.transform.localPosition = AtavismSettings.Instance.GetNpcInfoTextPosition;
                TextMeshPro textMeshProQuest = mobQuest.AddComponent<TextMeshPro>();
                textMeshProQuest.alignment = TextAlignmentOptions.Midline;
                textMeshProQuest.fontSize = AtavismSettings.Instance.GetNpcInfoTextSize;
                textMeshProQuest.color = AtavismSettings.Instance.GetNpcInfoTextColor;
                textMeshProQuest.text = "";
                textMeshProQuest.spriteAsset = AtavismSettings.Instance.GetSpriteAsset;

               






            }
            if (showTitle != AtavismSettings.Instance.GetGeneralSettings().showTitle)
            {
                showTitle = AtavismSettings.Instance.GetGeneralSettings().showTitle;
                UpdateNameDisplay(true);
            }
        }
        static string ToRGBHex(Color c)
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}", ToByte(c.r), ToByte(c.g), ToByte(c.b));
        }

        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }
        void OnDestroy()
        {
            if (node != null)
            {
                node.RemoveObjectPropertyChangeHandler("title", LevelHandler);
                node.RemoveObjectPropertyChangeHandler("level", LevelHandler);
                node.RemoveObjectPropertyChangeHandler("reaction", TargetTypeHandler);
                node.RemoveObjectPropertyChangeHandler("aggressive", TargetTypeHandler);
                node.RemoveObjectPropertyChangeHandler("adminLevel", AdminLevelHandler);
                node.RemoveObjectPropertyChangeHandler("questavailable", QuestAvailableHandler);
                node.RemoveObjectPropertyChangeHandler("questinprogress", QuestInProgressHandler);
                node.RemoveObjectPropertyChangeHandler("questconcludable", QuestConcludableHandler);
                node.RemoveObjectPropertyChangeHandler("dialogue_available", DialogueAvailableHandler);
                node.RemoveObjectPropertyChangeHandler("itemstosell", ItemsToSellHandler);
                node.RemoveObjectPropertyChangeHandler("nameDisplay", NameDisplayHandler);
                node.RemoveObjectPropertyChangeHandler("guildName", GuildNameDisplayHandler);
                node.RemoveObjectPropertyChangeHandler("deadstate", HandleDeadState);
                node.RemoveObjectPropertyChangeHandler("bankteller", HandleDeadState);
            }
            AtavismEventSystem.UnregisterEvent("UPDATE_LANGUAGE", this);
            //StopCoroutine(chIcon);
            //StopCoroutine(chTimer);
            StopAllCoroutines();
        }
    }
}