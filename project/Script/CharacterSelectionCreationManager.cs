using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

namespace Atavism
{

    [Serializable]
    public class AtavismGender
    {
        public string name;
        public Button genderButton;
        public Text genderText;
        public TextMeshProUGUI TMPGenderText;

    }


    public enum CreationState
    {
        Body,
        Head,
        Face,
        Hair
    }

    /// <summary>
    /// Handles the selection and creation of the players characters. This script must be added
    /// as a component in the Character Creation / Selection scene.
    /// </summary>
    public class CharacterSelectionCreationManager : MonoBehaviour
    {
        protected static CharacterSelectionCreationManager instance;

      //  public Texture cursorOverride;

        public List<GameObject> createUI;
        public List<GameObject> selectUI;
        public List<UGUICharacterSelectSlot> characterSlots;
        public GameObject enterUI;
        public Button createButton;
        public Text nameUI;
        public TextMeshProUGUI TMPNameUI;
        public Button deleteButton;
        public Text serverNameText;
        public TextMeshProUGUI TMPServerNameText;
        public UGUIServerList serverListUI;
        public GameObject characterCamera;
        public UGUIDialogPopup dialogWindow;
        public Transform spawnPosition;
        public InputField createCaracterName;
        public TMP_InputField TMPCreateCaracterName;

        protected GameObject character;
        protected GameObject characterDCS;

        protected List<CharacterEntry> characterEntries;
        protected CreationState creationState;
        protected LoginState loginState;
        protected string dialogMessage = "";
        protected string errorMessage = "";

        // Character select fields
        protected CharacterEntry characterSelected = null;
        protected string characterName = "";
        protected AtavismRaceData race;
        protected AtavismClassData aspect;
        protected string gender = "Male";
        public List<UGUICharacterRaceSlot> races;
        public List<UGUICharacterClassSlot> classes;

        public Image raceIcon;
        public Text raceTitle;
        public TextMeshProUGUI TMPRaceTitle;
        public Text raceDescription;
        public TextMeshProUGUI TMPRaceDescription;

        public Image classIcon;
        public Text classTitle;
        public TextMeshProUGUI TMPClassTitle;
        public Text classDescription;
        public TextMeshProUGUI TMPClassDescription;

        public Text genderMaleText;
        public TextMeshProUGUI TMPGenderMaleText;
        public Image GenderMaleImage;
        public Text genderFemaleText;
        public TextMeshProUGUI TMPGenderFemaleText;
        public Image GenderFemaleImage;
        public Color defaultButomTextColor = Color.white;
        public Color selectedButomTextColor = Color.green;
        public List<AtavismGender> genderList = new List<AtavismGender>();
        public GameObject createPanelRace;
        public UGUIAvatarList avatarList;
        public Image avatarIcon;

        // Camera fields
        public Vector3 cameraInLoc = new Vector3(0.193684f, 2.4f, 4.743689f);
        public Vector3 cameraOutLoc = new Vector3(0.4418643f, 1.21f, 6.72f);
        protected bool zoomingIn = false;
        protected bool zoomingOut = false;
        public float characterRotationSpeed = 250.0f;
        protected float x = 180;
        protected float y = 0;

        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;
            AtavismEventSystem.RegisterEvent("World_Error", this);

            StartCharacterSelection();
            if (characterEntries.Count == 0)
            {
                StartCharacterCreation();
            }
            if (characterCamera != null)
                characterCamera.SetActive(true);
        }

        private void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("World_Error", this);
        }

        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "World_Error")
            {
                ShowDialog(eData.eventArgs[0], true);
            }
        }


                // Update is called once per frame
                void Update()
        {
            float moveRate = 1.0f;
            if (zoomingIn)
            {
                characterCamera.transform.position = Vector3.Lerp(characterCamera.transform.position, cameraInLoc, Time.deltaTime * moveRate);
            }
            else if (zoomingOut)
            {
                characterCamera.transform.position = Vector3.Lerp(characterCamera.transform.position, cameraOutLoc, Time.deltaTime * moveRate);
            }
        }

        /// <summary>
        /// Handles character rotation if the mouse button is down and the player is dragging it.
        /// </summary>
        void LateUpdate()
        {
            //TODO: currently this is artificially limited to only work in the middle 1/3 of the screen, this restriction should be removed
            if (character && Input.GetMouseButton(0) && !AtavismCursor.Instance.IsMouseOverUI() /*(Input.mousePosition.x > Screen.width / 3) && (Input.mousePosition.x < Screen.width / 3 * 2)*/)
            {
                x -= Input.GetAxis("Mouse X") * characterRotationSpeed * 0.02f;

                Quaternion rotation = Quaternion.Euler(y, x, 0);

                //position.y = height;
                character.transform.rotation = rotation;
            }
        }

       /* void OnGUI()
        {
          if (cursorOverride != null)
            {
                UnityEngine.Cursor.visible = false;
                GUI.DrawTexture(new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y, 32, 32), cursorOverride);
            }
        }*/
          
        #region Character Selection
        public void StartCharacterSelection()
        {
            characterEntries = ClientAPI.GetCharacterEntries();
            ShowSelectionUI();

            if (characterEntries.Count > 0)
            {
                CharacterSelected(characterEntries[0]);
                if (enterUI != null)
                {
                    enterUI.SetActive(true);
                }
                if (nameUI != null)
                {
                    nameUI.gameObject.SetActive(true);
                }
                if (TMPNameUI != null)
                {
                    TMPNameUI.gameObject.SetActive(true);
                }
                if (deleteButton != null)
                {
                    deleteButton.gameObject.SetActive(true);
                }
            }
            else
            {
                if (character != null)
                    Destroy(character);
                characterSelected = null;
                if (enterUI != null)
                {
                    enterUI.SetActive(false);
                }
                if (nameUI != null)
                {
                    nameUI.gameObject.SetActive(false);
                }
                if (TMPNameUI != null)
                {
                    TMPNameUI.gameObject.SetActive(false);
                }

                if (deleteButton != null)
                {
                    deleteButton.gameObject.SetActive(false);
                }
            }
            if (characterEntries.Count < characterSlots.Count)
            {
                createButton.gameObject.SetActive(true);
            }
            else
            {
                createButton.gameObject.SetActive(false);
            }

            // Set the slots up
            for (int i = 0; i < characterSlots.Count; i++)
            {
                if (characterEntries.Count > i)
                {
                    // Set slot data
                    characterSlots[i].gameObject.SetActive(true);
                    characterSlots[i].SetCharacter(characterEntries[i]);
                    characterSlots[i].CharacterSelected(characterSelected);
                }
                else
                {
                    // Set slot data to null
                    characterSlots[i].gameObject.SetActive(false);
                    //characterSlots[i].SendMessage("SetCharacter", null);
                }
            }

            loginState = LoginState.CharacterSelect;
        }

        public void StartCharacterSelection(List<CharacterEntry> characterEntries)
        {
            this.characterEntries = characterEntries;
            StartCharacterSelection();
        }

        public virtual void CharacterSelected(CharacterEntry entry)
        {
            characterSelected = entry;
            foreach (UGUICharacterSelectSlot charSlot in characterSlots)
            {
                charSlot.CharacterSelected(characterSelected);
            }
            if (character != null)
                Destroy(character);
            race = GetRaceDataByName((string)characterSelected["race"]);
            gender = (string)characterSelected["gender"];
            Dictionary<string, object> appearanceProps = new Dictionary<string, object>();
            foreach (string key in entry.Keys)
            {
                if (key.StartsWith("custom:appearanceData:"))
                {
                    appearanceProps.Add(key.Substring(23), entry[key]);
                }
            }
            // Dna settings
            string prefabName = (string)characterSelected["model"];
            if (prefabName.Contains(".prefab"))
            {
                int resourcePathPos = prefabName.IndexOf("Resources/");
                prefabName = prefabName.Substring(resourcePathPos + 10);
                prefabName = prefabName.Remove(prefabName.Length - 7);
            }
            GameObject prefab = (GameObject)Resources.Load(prefabName);
            if (prefab != null)
                character = (GameObject)Instantiate(prefab, spawnPosition.position, spawnPosition.rotation);
            else
            {
                Debug.LogError("prefab = null model: " + prefabName + " Loading ExampleCharacter");
                prefab = (GameObject)Resources.Load("ExampleCharacter");
                if (prefab != null)
                    character = (GameObject)Instantiate(prefab, spawnPosition.position, spawnPosition.rotation);
            }  
            //SetCharacter (prefab);
            // Set equipment
            if (characterSelected.ContainsKey("weaponDisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("weaponDisplayID", (string)characterSelected["weaponDisplayID"]);
            }
            if (characterSelected.ContainsKey("weapon2DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("weapon2DisplayID", (string)characterSelected["weapon2DisplayID"]);
            }
            // UMA-equipment properties
            if (characterSelected.ContainsKey("legDisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("legDisplayID", (string)characterSelected["legDisplayID"]);
            }
            if (characterSelected.ContainsKey("chestDisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("chestDisplayID", (string)characterSelected["chestDisplayID"]);
            }
            if (characterSelected.ContainsKey("headDisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("headDisplayID", (string)characterSelected["headDisplayID"]);
            }
            if (characterSelected.ContainsKey("feetDisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("feetDisplayID", (string)characterSelected["feetDisplayID"]);
            }
            if (characterSelected.ContainsKey("handDisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("handDisplayID", (string)characterSelected["handDisplayID"]);
            }
            if (characterSelected.ContainsKey("capeDisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("capeDisplayID", (string)characterSelected["capeDisplayID"]);
            }
            if (characterSelected.ContainsKey("shoulderDisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("shoulderDisplayID", (string)characterSelected["shoulderDisplayID"]);
            }
            if (characterSelected.ContainsKey("beltDisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("beltDisplayID", (string)characterSelected["beltDisplayID"]);
            }
            if (characterSelected.ContainsKey("ringDisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("ringDisplayID", (string)characterSelected["ringDisplayID"]);
            }
            if (characterSelected.ContainsKey("ring2DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("ring2DisplayID", (string)characterSelected["ring2DisplayID"]);
            }
            if (characterSelected.ContainsKey("earringDisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("earringDisplayID", (string)characterSelected["earringDisplayID"]);
            }
            if (characterSelected.ContainsKey("earring2DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("earring2DisplayID", (string)characterSelected["earring2DisplayID"]);
            }
            if (characterSelected.ContainsKey("rangedDisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("rangedDisplayID", (string)characterSelected["rangedDisplayID"]);
            }
            if (characterSelected.ContainsKey("neckDisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("neckDisplayID", (string)characterSelected["neckDisplayID"]);
            }
            if (characterSelected.ContainsKey("slot1DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("slot1DisplayID", (string)characterSelected["slot1DisplayID"]);
            }
            if (characterSelected.ContainsKey("slot2DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("slot2DisplayID", (string)characterSelected["slot2DisplayID"]);
            }
            if (characterSelected.ContainsKey("slot3DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("slot3DisplayID", (string)characterSelected["slot3DisplayID"]);
            }
            if (characterSelected.ContainsKey("slot4DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("slot4DisplayID", (string)characterSelected["slot4DisplayID"]);
            }
            if (characterSelected.ContainsKey("slot5DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("slot5DisplayID", (string)characterSelected["slot5DisplayID"]);
            }
            if (characterSelected.ContainsKey("slot6DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("slot6DisplayID", (string)characterSelected["slot6DisplayID"]);
            }
            if (characterSelected.ContainsKey("slot7DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("slot7DisplayID", (string)characterSelected["slot7DisplayID"]);
            }
            if (characterSelected.ContainsKey("slot8DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("slot8DisplayID", (string)characterSelected["slot8DisplayID"]);
            }
            if (characterSelected.ContainsKey("slot9DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("slot9DisplayID", (string)characterSelected["slot9DisplayID"]);
            }
            if (characterSelected.ContainsKey("slot10DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("slot10DisplayID", (string)characterSelected["slot10DisplayID"]);
            }
            if (characterSelected.ContainsKey("slot11DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("slot11DisplayID", (string)characterSelected["slot11DisplayID"]);
            }
            if (characterSelected.ContainsKey("slot12DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("slot12DisplayID", (string)characterSelected["slot12DisplayID"]);
            }
            if (characterSelected.ContainsKey("slot13DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("slot13DisplayID", (string)characterSelected["slot13DisplayID"]);
            }
            if (characterSelected.ContainsKey("slot14DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("slot14DisplayID", (string)characterSelected["slot14DisplayID"]);
            }
            if (characterSelected.ContainsKey("slot15DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("slot15DisplayID", (string)characterSelected["slot15DisplayID"]);
            }
            if (characterSelected.ContainsKey("slot16DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("slot16DisplayID", (string)characterSelected["slot16DisplayID"]);
            }
            if (characterSelected.ContainsKey("slot17DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("slot17DisplayID", (string)characterSelected["slot17DisplayID"]);
            }
            if (characterSelected.ContainsKey("slot18DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("slot18DisplayID", (string)characterSelected["slot18DisplayID"]);
            }
            if (characterSelected.ContainsKey("slot19DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("slot19DisplayID", (string)characterSelected["slot19DisplayID"]);
            }
            if (characterSelected.ContainsKey("slot20DisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("slot20DisplayID", (string)characterSelected["slot20DisplayID"]);
            }
            if (characterSelected.ContainsKey("fashionDisplayID"))
            {
                character.GetComponent<AtavismMobAppearance>().UpdateEquipDisplay("fashionDisplayID", (string)characterSelected["fashionDisplayID"]);
            }
            


            // Name
                if (nameUI != null)
                nameUI.text = (string)entry["characterName"];
            if (TMPNameUI != null)
                TMPNameUI.text = (string)entry["characterName"];

            // Temp
            if (character!=null && character.GetComponent<CustomisedHair>() != null)
            {
                CustomisedHair customHair = character.GetComponent<CustomisedHair>();
                if (characterSelected.ContainsKey(customHair.hairPropertyName))
                {
                    customHair.UpdateHairModel((string)characterSelected[customHair.hairPropertyName]);
                }
            }
        }
        private float clicklimit = 0;
        public void Play()
        {
            if (clicklimit > Time.time)
                return;
            clicklimit = Time.time + 1f;
            dialogMessage = "Entering World...";
            AtavismClient.Instance.EnterGameWorld(characterSelected.CharacterId);
            //   Debug.LogError(dialogMessage);
        }

        public void DeleteCharacter()
        {
            dialogWindow.gameObject.SetActive(true);
#if AT_I2LOC_PRESET
        dialogWindow.ShowDialogOptionPopup(I2.Loc.LocalizationManager.GetTranslation("Do you want to delete character") + ": " + characterSelected["characterName"]);
#else
            dialogWindow.ShowDialogOptionPopup("Do you want to delete character: " + characterSelected["characterName"]);
#endif
        }

        public virtual void DeleteCharacterConfirmed()
        {
            Dictionary<string, object> attrs = new Dictionary<string, object>();
            attrs.Add("characterId", characterSelected.CharacterId);
            NetworkAPI.DeleteCharacter(attrs);
            characterSelected = null;
            foreach (CharacterEntry charEntry in characterEntries)
            {
                if (charEntry != characterSelected)
                {
                    CharacterSelected(charEntry);
                    characterSelected = charEntry;
                    StartCharacterSelection();
                    return;
                }
            }
            StartCharacterCreation();
        }
        #endregion Character Selection

        #region Character Creation
        public virtual void StartCharacterCreation()
        {
            ShowCreationUI();
            if (createCaracterName != null)
                createCaracterName.text = "";
            if (TMPCreateCaracterName != null)
                TMPCreateCaracterName.text = "";
            if (enterUI != null)
                enterUI.SetActive(false);
            if (races[0] != null)
                race = races[0].raceData;
            foreach (UGUICharacterRaceSlot raceSlot in races)
            {
                raceSlot.RaceSelected(race);
            }
            UpdateRaceDetails();
            aspect = classes[0].classData;
            foreach (UGUICharacterClassSlot classSlot in classes)
            {
                classSlot.ClassSelected(aspect);
            }


            if (character != null)
                Destroy(character);
            characterName = "";

            int randomResult = UnityEngine.Random.Range(0, 2);
            if (randomResult == 0)
            {
                gender = "Male";
            }
            else
            {
                gender = "Female";
            }
            if (avatarList != null)
                avatarList.PreparSlots(race.raceName,gender, aspect.className);

            // Do this after gender so the icons can be updated 
            UpdateClassDetails();

            ResetModel();

            /*if (classes.Count > 0)
                aspect = classes [0];
            SetCharacter ();*/
            loginState = LoginState.CharacterCreate;
            creationState = CreationState.Body;
        }

        void ZoomCameraIn()
        {
            zoomingIn = true;
            zoomingOut = false;
        }

        void ZoomCameraOut()
        {
            zoomingOut = true;
            zoomingIn = false;
        }

        public void ToggleAnim()
        {
            Animator anim = character.GetComponentInChildren<Animator>();
            if (anim.speed == 0)
                anim.speed = 1;
            else
                anim.speed = 0;
        }

        public void SetCharacterName(string characterName)
        {
            this.characterName = characterName;
        }

        public void CreateCharacterWithName(string characterName)
        {
            this.characterName = characterName;
            CreateCharacter();
        }

        /// <summary>
        /// Sets the characters race resulting in a new UMA model being generated.
        /// </summary>
        /// <param name="race">Race.</param>
        public virtual void SetCharacterRace(AtavismRaceData race)
        {
            this.race = race;
            ResetModel();
            foreach (UGUICharacterRaceSlot raceSlot in races)
            {
                raceSlot.RaceSelected(race);
            }
            UpdateRaceDetails();
        }

        protected void UpdateRaceDetails()
        {
            if (raceIcon != null)
                raceIcon.sprite = race.raceIcon;
            if (raceTitle != null)
                raceTitle.text = race.raceName;
            if (raceDescription != null)
                raceDescription.text = race.description;
            if (TMPRaceTitle != null)
                TMPRaceTitle.text = race.raceName;
            if (TMPRaceDescription != null)
                TMPRaceDescription.text = race.description;
        }

        public virtual void SetCharacterClass(AtavismClassData classData)
        {
            this.aspect = classData;
            foreach (UGUICharacterClassSlot classSlot in classes)
            {
                if (classSlot != null)
                    classSlot.ClassSelected(aspect);
            }
            UpdateClassDetails();
        }

        protected void UpdateClassDetails()
        {
            if (classIcon != null)
            {
                if (gender == "Male")
                    classIcon.sprite = aspect.maleClassIcon;
                else if (gender == "Female")
                    classIcon.sprite = aspect.femaleClassIcon;
            }
            if (classTitle != null)
                classTitle.text = aspect.className;
            if (classDescription != null)
                classDescription.text = aspect.description;
            if (TMPClassTitle != null)
                TMPClassTitle.text = aspect.className;
            if (TMPClassDescription != null)
                TMPClassDescription.text = aspect.description;

            foreach (UGUICharacterClassSlot classSlot in classes)
            {
                if (classSlot != null)
                    classSlot.GenderChanged(gender);
            }
            if (avatarList != null)
                avatarList.PreparSlots(race.raceName, gender, aspect.className);
        }

        /// <summary>
        /// Sends the Create Character message to the server with a collection of properties
        /// to save to the new character.
        /// </summary>
        public virtual void CreateCharacter()
        {
            if (characterName == "")
                return;
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("characterName", characterName);
            if (gender == "Male")
            {
                properties.Add("prefab", race.maleCharacterPrefab.name);
            }
            else if (gender == "Female")
            {
                properties.Add("prefab", race.femaleCharacterPrefab.name);
            }
            properties.Add("race", race.raceName);
            properties.Add("aspect", aspect.className);
            properties.Add("gender", gender);
            if (PortraitManager.Instance.portraitType == PortraitType.Custom)
            {
                Sprite[] icons = null;//= { new Sprite() };
                icons = AtavismSettings.Instance.Avatars(race.raceName, gender, aspect.className);

               /* if (gender == "Male")
                    icons = AtavismSettings.Instance.meleAvatars;
                if (gender == "Female")
                    icons = AtavismSettings.Instance.femaleAvatars;*/
                if (icons != null && icons.Length > 0)
                {
                    if (avatarList == null)
                    {
                        Debug.LogError("CharacterSelectionCreationManager avatarList is null", gameObject);
                    }
                    else
                    {
                        if (icons.Length > avatarList.Selected())
                            if (icons[avatarList.Selected()] == null)
                            {
                                Debug.LogError("CharacterSelectionCreationManager icons for " + race.raceName+" "+gender + " "+ aspect.className+" is null ; avatarList selected " + avatarList.Selected(), gameObject);
                            }
                            else
                            {
                                properties.Add("custom:portrait", icons[avatarList.Selected()].name);
                                //     properties.Add("portrait", icons[avatarList.Selected()].name);
                            }
                    }
                }
            }

            if (PortraitManager.Instance.portraitType == PortraitType.Class)
            {
                Sprite portraitSprite = PortraitManager.Instance.GetCharacterSelectionPortrait(gender, race.raceName, aspect.className, PortraitType.Class);
                properties.Add("custom:portrait", portraitSprite.name);
            }

            // If the character has the customisable hair, save the property
            /*if (character.GetComponent<CustomisedHair>() != null) {
                CustomisedHair customHair = character.GetComponent<CustomisedHair>();
                properties.Add("custom:" + customHair.hairPropertyName, customHair.ActiveHair.name);
            }*/
#if AT_I2LOC_PRESET
        dialogMessage = I2.Loc.LocalizationManager.GetTranslation("Please wait...");
#else
            dialogMessage = "Please wait...";
#endif
            errorMessage = "";
            characterSelected = AtavismClient.Instance.NetworkHelper.CreateCharacter(properties);
            if (characterSelected == null)
            {
                errorMessage = "Unknown Error";
            }
            else
            {
                if (!characterSelected.Status)
                {
                    if (characterSelected.ContainsKey("errorMessage"))
                    {
                        errorMessage = (string)characterSelected["errorMessage"];
                    }
                }
            }
            dialogMessage = "";
            if (errorMessage == "")
            {
                StartCharacterSelection();
                //nameUI.text = characterName;
                // Have to rename all the properties. This seems kind of pointless.
                Dictionary<string, object> newProps = new Dictionary<string, object>();
                foreach (string prop in properties.Keys)
                {
                    if (prop.Contains(":"))
                    {
                        string[] newPropParts = prop.Split(':');
                        if (newPropParts.Length > 2 && newPropParts[2] != null)
                        {
                            string newProp = "uma" + newPropParts[2];
                            newProps.Add(newProp, properties[prop]);
                        }
                    }
                }
                foreach (string prop in newProps.Keys)
                {
                    if (!characterSelected.ContainsKey(prop))
                        characterSelected.Add(prop, newProps[prop]);
                }
            }
            else
            {
#if AT_I2LOC_PRESET
            ShowDialog(I2.Loc.LocalizationManager.GetTranslation(errorMessage), true);
#else
                ShowDialog(errorMessage, true);
#endif
            }
        }

        /*public void SetRace(string race, string gender) 
        {
            SetRace (race + gender);
            this.gender = gender;
        }*/

        /// <summary>
        /// Cancels character creation and returns back to the selection screen
        /// </summary>
        public virtual void CancelCharacterCreation()
        {
            Destroy(character);
            if (characterSelected != null)
            {
                race = GetRaceDataByName((string)characterSelected["race"]);
                gender = (string)characterSelected["gender"];
                CharacterSelected(characterSelected);
            }
            //ShowSelectionUI ();
            StartCharacterSelection();
        }

        void ShowSelectionUI()
        {
            loginState = LoginState.CharacterSelect;
            foreach (GameObject ui in selectUI)
            {
                ui.SetActive(true);
            }
            foreach (GameObject ui in createUI)
            {
                ui.SetActive(false);
            }
            if (serverNameText != null)
            {
                serverNameText.text = AtavismClient.Instance.WorldId;
            }
            if (TMPServerNameText != null)
            {
                TMPServerNameText.text = AtavismClient.Instance.WorldId;
            }
        }

        void ShowCreationUI()
        {
            foreach (GameObject ui in selectUI)
            {
                ui.SetActive(false);
            }
            foreach (GameObject ui in createUI)
            {
                ui.SetActive(true);
            }
        }

        protected void ShowDialog(string message, bool showButton)
        {
            if (dialogWindow == null)
                return;
            dialogWindow.gameObject.SetActive(true);
            dialogWindow.ShowDialogPopup(message, showButton);
        }

        public virtual void SetGenderMale()
        {
            if (gender == "Male")
                return;
            gender = "Male";
            if (genderFemaleText)
                genderFemaleText.color = defaultButomTextColor;
            if (genderMaleText)
                genderMaleText.color = selectedButomTextColor;
            if (TMPGenderFemaleText)
                TMPGenderFemaleText.color = defaultButomTextColor;
            if (TMPGenderMaleText)
                TMPGenderMaleText.color = selectedButomTextColor;
            if (GenderMaleImage)
                GenderMaleImage.color = selectedButomTextColor;
            if (GenderFemaleImage)
                GenderFemaleImage.color = defaultButomTextColor;

            UpdateClassDetails();
            ResetModel();
            if (avatarList != null)
                avatarList.PreparSlots(race.raceName, gender, aspect.className);


        }

        public virtual void SetGenderFemale()
        {
            if (gender == "Female")
                return;
            gender = "Female";
            if (genderMaleText)
                genderMaleText.color = defaultButomTextColor;
            if (genderFemaleText)
                genderFemaleText.color = selectedButomTextColor;
            if (TMPGenderMaleText)
                TMPGenderMaleText.color = defaultButomTextColor;
            if (TMPGenderFemaleText)
                TMPGenderFemaleText.color = selectedButomTextColor;
            if (GenderMaleImage)
                GenderMaleImage.color = defaultButomTextColor;
            if (GenderFemaleImage)
                GenderFemaleImage.color = selectedButomTextColor;
            UpdateClassDetails();
            ResetModel();
            if (avatarList != null)
                avatarList.PreparSlots(race.raceName, gender, aspect.className);

        }

        void ResetModel()
        {
            if (character != null)
                Destroy(character);

            if (gender == "Male")
            {
                if (genderFemaleText)
                    genderFemaleText.color = defaultButomTextColor;
                if (genderMaleText)
                    genderMaleText.color = selectedButomTextColor;
                if (TMPGenderFemaleText)
                    TMPGenderFemaleText.color = defaultButomTextColor;
                if (TMPGenderMaleText)
                    TMPGenderMaleText.color = selectedButomTextColor;
                if (GenderMaleImage)
                    GenderMaleImage.color = selectedButomTextColor;
                if (GenderFemaleImage)
                    GenderFemaleImage.color = defaultButomTextColor;
                character = (GameObject)Instantiate(race.maleCharacterPrefab, spawnPosition.position, spawnPosition.rotation);
            }
            else if (gender == "Female")
            {
                if (genderMaleText)
                    genderMaleText.color = defaultButomTextColor;
                if (genderFemaleText)
                    genderFemaleText.color = selectedButomTextColor;
                if (TMPGenderMaleText)
                    TMPGenderMaleText.color = defaultButomTextColor;
                if (TMPGenderFemaleText)
                    TMPGenderFemaleText.color = selectedButomTextColor;
                if (GenderMaleImage)
                    GenderMaleImage.color = defaultButomTextColor;
                if (GenderFemaleImage)
                    GenderFemaleImage.color = selectedButomTextColor;
                character = (GameObject)Instantiate(race.femaleCharacterPrefab, spawnPosition.position, spawnPosition.rotation);
            }
            x = 180;
        }

        public void DialogYesClicked()
        {
            DeleteCharacterConfirmed();
            dialogWindow.gameObject.SetActive(false);
        }

        public void DialogNoClicked()
        {
            dialogWindow.gameObject.SetActive(false);
        }
        public void ShowAvatarList()
        {
            avatarList.gameObject.SetActive(true);
            createPanelRace.SetActive(false);

        }
        public void AvatarSelected()
        {
            avatarList.gameObject.SetActive(false);
            createPanelRace.SetActive(true);
            if (avatarIcon != null)
                avatarIcon.sprite = avatarList.icons[avatarList.Selected()];
        }

        public void CloseAvatarList()
        {
            if (avatarList != null)
                avatarList.gameObject.SetActive(false);
            if (createPanelRace != null)
                createPanelRace.SetActive(true);
        }

        #endregion Character Creation

        // Temp:
        public void SwitchHair()
        {
            if (character.GetComponent<CustomisedHair>() != null)
            {
                character.GetComponent<CustomisedHair>().SwitchHairForward();
            }
        }

        public AtavismRaceData GetRaceDataByName(string raceName)
        {
            foreach (UGUICharacterRaceSlot raceSlot in races)
            {
                if (raceSlot != null)
                    if (raceSlot.raceData.raceName == raceName)
                    {
                        return raceSlot.raceData;
                    }
            }
            return null;
        }

        public AtavismClassData GetClassDataByName(string className)
        {
            foreach (UGUICharacterClassSlot classSlot in classes)
            {
                if (classSlot.classData.className == className)
                {
                    return classSlot.classData;
                }
            }
            return null;
        }

        public void ChangeScene(string sceneName)
        {
            //Application.LoadLevel(sceneName);
            SceneManager.LoadScene(sceneName);
        }

        public void Quit()
        {
            Application.Quit();
        }

        public static CharacterSelectionCreationManager Instance
        {
            get
            {
                return instance;
            }
        }

        public string DialogMessage
        {
            get
            {
                return dialogMessage;
            }
            set
            {
                dialogMessage = value;
            }
        }

        public string ErrorMessage
        {
            get
            {
                return errorMessage;
            }
            set
            {
                errorMessage = value;
            }
        }

        public LoginState State
        {
            get
            {
                return loginState;
            }
        }

        public GameObject Character
        {
            get
            {
                return character;
            }
        }
        public GameObject CharacterDCS
        {
            get
            {
                return characterDCS;
            }
        }
    }
}