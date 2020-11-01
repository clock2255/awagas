using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
//using Ceto;
using UnityEngine.Audio;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Linq;
//using VolumetricFogAndMist;
#if AT_PPS2_PRESET
using UnityEngine.Rendering.PostProcessing;
#endif
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif


namespace Atavism
{

    [Serializable]
    public class AtavismQualitySetingsDefault
    {
        public string name;
        public int pixelLightCount = 3;
        public int masterTextureLimit = 0;
        public AnisotropicFiltering anisotropicFiltering = AnisotropicFiltering.ForceEnable;
        public bool softParticles;
        public bool realtimeReflectionProbes;
        public bool billboardsFaceCameraPosition;
        public ShadowQuality shadows;
        public ShadowResolution shadowResolution;
        public float shadowDistance;
        public int shadowCascades;
        public SkinWeights blendWeights;
        public int verticalSync = 1;//false=Dont sync; True =Every V Bank
        public float lodBias = 2f;
        public int particleRaycastBudget = 4096;
    }





    [Serializable]
    public class AtCred
    {
        private string _l = "";
        private string _p = "";
        public string l
        {
            get
            {
                byte[] decodedBytes = Convert.FromBase64String(_l);
                return Encoding.UTF8.GetString(decodedBytes);
            }
            set
            {
                byte[] bytesToEncode = Encoding.UTF8.GetBytes(value);
                _l = Convert.ToBase64String(bytesToEncode);
            }
        }
        public string p
        {
            get
            {
                byte[] decodedBytes = Convert.FromBase64String(_p);
                return Encoding.UTF8.GetString(decodedBytes);
            }
            set
            {
                byte[] bytesToEncode = Encoding.UTF8.GetBytes(value);
                _p = Convert.ToBase64String(bytesToEncode);
            }
        }
    }

    [Serializable]
    public class AtavismGeneralSettings
    {
        public bool freeCamera = false;
        public string language;
        public float sensitivityMouse = 0.035f;
        public float sensitivityWheelMouse = -1f;
        public bool showHelmet = true;
        public bool saveCredential = false;
        public bool showTitle = true;
    }

    [Serializable]
    public class AtavismAudioSettings
    {
        public float masterLevel = -20f;
        public float sfxLevel = -20f;
        public float musicLevel = -15f;
        public float uiLevel = -20f;
        public float ambientLevel = -20f;
        public float footstepsLevel = -20f;

    }

    [Serializable]
    public class AtavismVideoSettings
    {
        public bool fps = false;
        public int quality = 5;
        public bool customSettings = false;
        //Quality Custom Settings
        public int shadows = 2;//0=Disable; 1=Hard only; 2=All(Soft&Hard)
        public int shadowDistance = 150;
        public int shadowResolution = 1;//0=Low; 1=Medium; 2=High; 3=VeryHigh
        public int verticalSync = 1;//false=Dont sync; True =Every V Bank
        [Range(0, 3)]
        public int lodBias = 2;
        public int particleRaycastBudget = 4096;
        public int masterTextureLimit = 0;
        public bool softParticles = true;
        //Camera Effects
        public bool amplifyOcclusionEffect = true;
        public bool seScreenSpaceShadows = true;
        public bool volumetricFog = true;
        public bool hxVolumetricCamera = true;
        // PostProcessingBehaviour Start
        public int antialiasing = 0;
        public bool depthOfField = true;
        public bool vignette = true;
        public bool chromaticAberration = true;
        public bool motionBlur = true;
        public bool bloom = true;
        public bool screenSpaceReflections = true;
        public bool dithering = true;
        public bool colorGrading = true;
        public bool autoExposure = true;
        // PostProcessingBehaviour End
        public bool ambientOcclusion = true;
        public bool depthBlur = true;
        //trawa
        [Range(0, 1)]
        public float detailObjectDensity = 1f;



    }


    [Serializable]
    public class AtavismAllSettings
    {
        public AtavismVideoSettings videoSettings = new AtavismVideoSettings();
        //    public ZbWaterSettings waterSettings = new ZbWaterSettings();
        public AtavismAudioSettings audioSettings = new AtavismAudioSettings();
        public AtavismGeneralSettings generalSettings = new AtavismGeneralSettings();
        public Dictionary<string, AtavismWindowsSettings> windowsSettings = new Dictionary<string, AtavismWindowsSettings>();
        public Dictionary<long, List<long>> questListSelected = new Dictionary<long, List<long>>();
      //  public List<long> questListSelected = new List<long>();
        public AtavismKeySettings keySettings = new AtavismKeySettings();
        public AtCred credential = new AtCred();
    }

    [Serializable]
    public class AtavismKeySettings
    {
        public KeyCode strafeLeft = KeyCode.Q;
        public KeyCode strafeRight = KeyCode.E;
        public KeyCode moveForward = KeyCode.W;
        public KeyCode altMoveForward = KeyCode.UpArrow;
        public KeyCode moveBackward = KeyCode.S;
        public KeyCode altMoveBackward = KeyCode.DownArrow;
        public KeyCode turnLeft = KeyCode.A;
        public KeyCode altTurnLeft = KeyCode.LeftArrow;
        public KeyCode turnRight = KeyCode.D;
        public KeyCode altTurnRight = KeyCode.RightArrow;
        public KeyCode autoRun = KeyCode.Numlock;
        public KeyCode walkRun = KeyCode.Backslash;
        public KeyCode jump = KeyCode.Space;
        public KeyCode showWeapon = KeyCode.V;
        public KeyCode inventory = KeyCode.I;
        public KeyCode character = KeyCode.H;
        public KeyCode mail = KeyCode.M;
        public KeyCode guild = KeyCode.G;
        public KeyCode quest = KeyCode.L;
        public KeyCode skiles = KeyCode.U;
        public KeyCode map = KeyCode.P;
        public KeyCode arena = KeyCode.B;
        public KeyCode social = KeyCode.K;

    }

    [Serializable]
    public class AtavismWindowsSettings
    {
        public string windowName = "";
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;
    }
    [Serializable]
    public class DsMiniMapSettings
    {
        public GameObject minimapItemPrefab;
        public Sprite minimapIcon;
        public float minimapIconSize = 30;
        public Color minimapIconColor = Color.white;
        public Sprite minimapQuestConcludableIcon;
        public float minimapQuestConcludableIconSize = 30;
        public Color minimapQuestConcludableIconColor = Color.white;
        public Sprite minimapQuestProgressIcon;
        public float minimapQuestProgressIconSize = 30;
        public Color minimapQuestProgressIconColor = Color.white;
        public Sprite minimapQuestAvailableIcon;
        public float minimapQuestAvailableIconSize = 30;
        public Color minimapQuestAvailableIconColor = Color.white;
        public Sprite minimapQuestMobArea;
        public float minimapQuestMobAreaSize = 30;
        public Color minimapQuestMobAreaColor = Color.white;
        public Sprite minimapQuestTarget;
        public float minimapQuestTargetSize = 30;
        public Color minimapQuestTargetColor = Color.white;
        public Sprite minimapShopIcon;
        public float minimapShopIconSize = 30;
        public Color minimapShopIconColor = Color.white;
        public Sprite minimapBankIcon;
        public float minimapBankIconSize = 30;
        public Color minimapBankIconColor = Color.white;
        public Sprite minimapBossIcon;
        public float minimapBossIconSize = 30;
        public Color minimapBossIconColor = Color.white;
    }
    [Serializable]
    public class DsAdminLocation
    {
        [SerializeField]
        string _name;
        [SerializeField]
        Vector3 _loc;
        public string Name
        {
            get
            {
                return _name;
            }
        }
        public Vector3 Loc
        {
            get
            {
                return _loc;
            }
        }
    }

    [Serializable]
    public class ClassAvatar
    {
        public string name = "ExampleClass";
        public Sprite[] avatars;
    }

    [Serializable]
    public class GenderAvatar
    {
        public string name = "ExampleGender";
        public List<ClassAvatar> classes = new List<ClassAvatar>();
    }

    [Serializable]
    public class RaceAvatar
    {
        public string name= "ExampleRace";
        public List<GenderAvatar> genders =new List<GenderAvatar>();
    }

    [Serializable]
    public class DsAdminPanelSettings
    {
        [SerializeField]
        string _instanceName;
        [SerializeField]
        List<DsAdminLocation> _locs = new List<DsAdminLocation>();
        public string InstanceName
        {
            get
            {
                return _instanceName;
            }
        }
        public List<DsAdminLocation> Loc
        {
            get
            {
                return _locs;
            }
        }
    }



    public class AtavismSettings : MonoBehaviour
    {
        static AtavismSettings instance;

        #region Variable Settings
        [AtavismSeparator("General Settings")]
        public bool autoPlayCharacter = false;
        [SerializeField] AtavismAllSettings settings = new AtavismAllSettings();
        [SerializeField] List<DsAdminPanelSettings> _adminLocationSettings = new List<DsAdminPanelSettings>();
        [SerializeField] int questPrevLimit = 4;
        public Sprite expIcon;




        public bool saveInFile = true;
        [AtavismSeparator("prefabs")]
        public UGUIAtavismActivatable actionBarPrefab;
        public UGUIAtavismActivatable inventoryItemPrefab;
        public UGUIAtavismActivatable abilitySlotPrefab;
        [AtavismSeparator("Inventory ")]
        public List<Color> itemQualityColor;
        public Sprite defaultBagIcon;
        public Color itemDropColorTrue = Color.blue;
        public Color itemDropColorFalse = Color.red;
        /*  [AtavismSeparator("Quest Minimap Icons")]
           public Sprite questAvailableIcon;
           public Sprite questInProgressIcon;
           public Sprite questConcludableIcon;
           public Sprite dialogAvailableIcon;
           public Sprite itemToSellIcon;*/
        [AtavismSeparator("Player/Mob Names Settings")]
        [SerializeField] bool visibleMobsName = true;
        [SerializeField] TMP_FontAsset mobNameFont;
        [SerializeField] Vector3 mobNamePosition = new Vector3(0f, 0.5f, 0f);
        [SerializeField] int mobNameFontSize = 2;
        [SerializeField] Color mobNameDefaultColor = Color.white;
        [SerializeField] TextAlignmentOptions mobNameAlignment = TextAlignmentOptions.Midline;
        [SerializeField] Vector4 mobNameMargin = new Vector4(7f, 2f, 7f, 2f);
        [SerializeField] float mobNameOutlineWidth = 0.2f;
        [SerializeField] string questNewText = "!";
        [SerializeField] string questConcludableText = "*";
        [SerializeField] string questProgressText = "?";
        [SerializeField] string shopText = "";
        [SerializeField] string bankText = "bank";
        [SerializeField] int npcInfoTextSize = 10;
        [SerializeField] Color npcInfoTextColor = Color.white;
        [SerializeField] Vector3 npcInfoTextPosition = new Vector3(0f, 0.5f, 0f);
        [SerializeField] TMP_SpriteAsset npcInfoSpriteAsset;

        [AtavismSeparator("Avatar Icons")]
       // public Sprite[] meleAvatars;
       // public Sprite[] femaleAvatars;

        [SerializeField] List<RaceAvatar> races = new List<RaceAvatar>();
        [AtavismSeparator("Audio Settings")]
        public AudioMixer masterMixer;
        public AudioMixerSnapshot mixerFocus;
        public AudioMixerSnapshot mixerNoFocus;
        //private bool gameFocus = true;
        GameObject mainCamera;
        GameObject effectCamera;
        Camera CharacterPanelCamera;
        Camera OtherCharacterPanelCamera;
        Transform CharacterPanelSpawn;
        Transform OtherCharacterPanelSpawn;
        GameObject characterAvatar;
        GameObject otherCharacterAvatar;
        [AtavismSeparator("Graphic Quality")]
        [SerializeField]
        List<AtavismQualitySetingsDefault> _defaultQualitySettings = new List<AtavismQualitySetingsDefault>();
#if AT_PPS2_PRESET
    List<PostProcessProfile> postProcessProfiles = new List<PostProcessProfile>();
    List<PostProcessVolume> postProcessVolumes = new List<PostProcessVolume>();
#endif
        string createURL_PL = "https://www.atavismonline.com";
        string forgotURL_PL = "https://www.atavismonline.com";
        string webURL_PL = "https://www.atavismonline.com";
        string shopURL_PL = "https://www.atavismonline.com";
        string createURL_EN = "https://www.atavismonline.com";
        string forgotURL_EN = "https://www.atavismonline.com";
        string webURL_EN = "https://www.atavismonline.com";
        string shopURL_EN = "https://www.atavismonline.com";

        [AtavismSeparator("Uma Settings")]
        [SerializeField] GameObject UMAGameObject;
        [SerializeField] GameObject wardrobeGameObject;

        [SerializeField] GameObject sceneLoaderPrefab;
        private AssetBundle m_assetsMob;
        private AssetBundle m_assetsNpc;
        //    public vAudioSurface defaultSurface;
        //    public List<vAudioSurface> customSurfaces;

        [AtavismSeparator("UGUI Mini Map Settings")]
        [SerializeField] DsMiniMapSettings minimapSettings = new DsMiniMapSettings();

        [AtavismSeparator("LevelUp Effect Setting")]
         [SerializeField] GameObject levelUpPrefab;
        // [MinMax(1f, 99f), DisplayName("Filtering (%)"), Tooltip("")]
        [SerializeField] float levelUpPrefabDuration = 2f;
       //  Dictionary<string, DsWindowsSettings> windowsSettings;

        [AtavismSeparator("Instance settings")]
        [SerializeField] List<string> gameInstances = new List<string>();
        [SerializeField] List<string> arenaInstances = new List<string>();
        [SerializeField] List<string> dungeonInstances = new List<string>();
        #endregion
        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;
            mainCamera = GameObject.Find("MainCamera");
            effectCamera = GameObject.Find("EffectCamera");
            Load();
            LoadUmaWardrobe();
            LoadLoader();
        }


        void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            //     if (GameObject.Find("UMA") == null && UMAGameObject) {
            //          Instantiate(UMAGameObject);
            //      }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if ((scene.name == "Login" || scene.name == ClientAPI.Instance.characterSceneName) && Quests.Instance != null)
            {
                Quests.Instance.QuestLogEntries.Clear();
                Quests.Instance.QuestHistoryLogEntries.Clear();
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                if (GameObject.Find("/UMA") == null && UMAGameObject)
                {
                    Instantiate(UMAGameObject);
                }
            }
        }
        void Update()
        {
            if (ClientAPI.Instance == null && !SceneManager.GetActiveScene().name.Equals("Login"))
            {
                SceneManager.LoadScene("Login");
            }
        }

        void OnEnabled()
        {

        }

        void Save()
        {

            //   Debug.LogError("Save settings");
            if (saveInFile)
            {
                //    Debug.LogError("Save settings start");
                if (!Directory.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/" + Application.productName))
                    Directory.CreateDirectory(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/" + Application.productName);
                FileStream file = File.Create(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/" + Application.productName + "/user.settings");
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(file, settings);
                }
                catch (SerializationException e)
                {
                    Debug.LogError("Failed to serialize. Reason: " + e.Message);

                    throw;
                }
                finally
                {
                    file.Close();
                }
                //      Debug.LogError("Save settings end");

            }
            else
            {
                PlayerPrefs.SetFloat("masterLevel", settings.audioSettings.masterLevel);
                PlayerPrefs.SetFloat("sfxLevel", settings.audioSettings.sfxLevel);
                PlayerPrefs.SetFloat("musicLevel", settings.audioSettings.musicLevel);
                PlayerPrefs.SetFloat("uiLevel", settings.audioSettings.uiLevel);
                PlayerPrefs.SetFloat("ambientLavel", settings.audioSettings.ambientLevel);
                PlayerPrefs.SetFloat("footstepsLevel", settings.audioSettings.footstepsLevel);

                PlayerPrefs.SetInt("Video fps", settings.videoSettings.fps == true ? 1 : 0);
                PlayerPrefs.SetInt("Video texture limit", settings.videoSettings.masterTextureLimit);
                PlayerPrefs.SetInt("Video quality", settings.videoSettings.quality);
                PlayerPrefs.SetInt("Video ambient occlusion", settings.videoSettings.ambientOcclusion == true ? 1 : 0);
                PlayerPrefs.SetInt("Video deep blur", settings.videoSettings.depthBlur == true ? 1 : 0);
                PlayerPrefs.SetInt("Video bloom", settings.videoSettings.bloom == true ? 1 : 0);
                PlayerPrefs.SetInt("Video Vertical sync", settings.videoSettings.verticalSync);
                PlayerPrefs.SetInt("Video shadows", settings.videoSettings.shadows);
                PlayerPrefs.SetFloat("Video lod bias", settings.videoSettings.lodBias);
                PlayerPrefs.SetInt("Video antialiasing", settings.videoSettings.antialiasing);
                //General Settings
                PlayerPrefs.SetString("Language", settings.generalSettings.language);
                PlayerPrefs.SetInt("freeCamera", settings.generalSettings.freeCamera == true ? 1 : 0);
                PlayerPrefs.SetFloat("mSens", settings.generalSettings.sensitivityMouse);
                PlayerPrefs.SetFloat("mWSens", settings.generalSettings.sensitivityWheelMouse);
                PlayerPrefs.SetInt("showHelm", settings.generalSettings.showHelmet == true ? 1 : 0);
                PlayerPrefs.SetInt("savec", settings.generalSettings.saveCredential == true ? 1 : 0);

                //Keys
                PlayerPrefs.SetInt("ksl", (int)settings.keySettings.strafeLeft);
                PlayerPrefs.SetInt("ksr", (int)settings.keySettings.strafeRight);
                PlayerPrefs.SetInt("kmf", (int)settings.keySettings.moveForward);
                PlayerPrefs.SetInt("kamf", (int)settings.keySettings.altMoveForward);
                PlayerPrefs.SetInt("kmb", (int)settings.keySettings.moveBackward);
                PlayerPrefs.SetInt("kamb", (int)settings.keySettings.altMoveBackward);
                PlayerPrefs.SetInt("ktl", (int)settings.keySettings.turnLeft);
                PlayerPrefs.SetInt("katl", (int)settings.keySettings.altTurnLeft);
                PlayerPrefs.SetInt("ktr", (int)settings.keySettings.turnRight);
                PlayerPrefs.SetInt("katr", (int)settings.keySettings.altTurnRight);
                PlayerPrefs.SetInt("kar", (int)settings.keySettings.autoRun);
                PlayerPrefs.SetInt("kwr", (int)settings.keySettings.walkRun);
                PlayerPrefs.SetInt("kju", (int)settings.keySettings.jump);
                PlayerPrefs.SetInt("ksw", (int)settings.keySettings.showWeapon);
                PlayerPrefs.SetInt("kinv", (int)settings.keySettings.inventory);
                PlayerPrefs.SetInt("kchar", (int)settings.keySettings.character);
                PlayerPrefs.SetInt("kmail", (int)settings.keySettings.mail);
                PlayerPrefs.SetInt("kquild", (int)settings.keySettings.guild);
                PlayerPrefs.SetInt("kquest", (int)settings.keySettings.quest);
                PlayerPrefs.SetInt("kskill", (int)settings.keySettings.skiles);
                PlayerPrefs.SetInt("kmap", (int)settings.keySettings.map);
                PlayerPrefs.SetInt("karena", (int)settings.keySettings.arena);
                PlayerPrefs.SetInt("ksoc", (int)settings.keySettings.social);
                //Quests
               // PlayerPrefs.SetString("qs", string.Join(";", settings.questListSelected.Select(x => x.ToString()).ToArray()));
                PlayerPrefs.SetString("qs", string.Join("!", settings.questListSelected.Select(x => x.Key + "=" + string.Join(";", x.Value.Select(y => y.ToString()).ToArray())).ToArray()));
               // string.Join(";", x.Value.Select(y => y.ToString()).ToArray()));
                //Credential
                PlayerPrefs.SetString("cl", settings.credential.l);
                PlayerPrefs.SetString("cp", settings.credential.p);
                // PlayerPrefs.SetString("Windows",)
                PlayerPrefs.Save();
            }
        }

        void Load()
        {
            //  Debug.LogError("Load settings");
            if (saveInFile)
            {
                if (File.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/" + Application.productName + "/user.settings"))
                {
                    FileStream file = File.Open(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/" + Application.productName + "/user.settings", FileMode.Open);
                    try
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        settings = (AtavismAllSettings)bf.Deserialize(file);
                    }
                    catch (SerializationException e)
                    {
                        Debug.LogError("Failed to deserialize. Reason: " + e.Message);
                        throw;
                    }
                    finally
                    {
                        file.Close();
                    }
                    applySettings();
                }
                else
                {
                    Save();
                }
            }
            else
            {
                if (settings == null)
                    settings = new AtavismAllSettings();
                settings.credential.p = PlayerPrefs.GetString("cp");
                settings.credential.l = PlayerPrefs.GetString("cl");
                settings.audioSettings.masterLevel = PlayerPrefs.GetFloat("masterLevel", settings.audioSettings.masterLevel);
                settings.audioSettings.sfxLevel =  PlayerPrefs.GetFloat("sfxLevel", settings.audioSettings.sfxLevel);
                settings.audioSettings.musicLevel = PlayerPrefs.GetFloat("musicLevel", settings.audioSettings.musicLevel);
                settings.audioSettings.uiLevel = PlayerPrefs.GetFloat("uiLevel", settings.audioSettings.uiLevel);
                settings.audioSettings.ambientLevel = PlayerPrefs.GetFloat("ambientLavel", settings.audioSettings.ambientLevel);
                settings.audioSettings.footstepsLevel = PlayerPrefs.GetFloat("footstepsLevel", settings.audioSettings.footstepsLevel);

                settings.videoSettings.fps = PlayerPrefs.GetInt("Video fps")==1;//, settings.videoSettings.fps == true ? 1 : 0);
                settings.videoSettings.masterTextureLimit = PlayerPrefs.GetInt("Video texture limit");//, settings.videoSettings.masterTextureLimit);
                settings.videoSettings.quality = PlayerPrefs.GetInt("Video quality");//, settings.videoSettings.quality);
                settings.videoSettings.ambientOcclusion = PlayerPrefs.GetInt("Video ambient occlusion") == 1;//, settings.videoSettings.ambientOcclusion == true ? 1 : 0);
                settings.videoSettings.depthBlur = PlayerPrefs.GetInt("Video deep blur") == 1;//, settings.videoSettings.depthBlur == true ? 1 : 0);
                settings.videoSettings.bloom = PlayerPrefs.GetInt("Video bloom") == 1;//, settings.videoSettings.bloom == true ? 1 : 0);
                settings.videoSettings.verticalSync = PlayerPrefs.GetInt("Video Vertical sync");//, settings.videoSettings.verticalSync);
                settings.videoSettings.shadows = PlayerPrefs.GetInt("Video shadows");//, settings.videoSettings.shadows);
                settings.videoSettings.lodBias = PlayerPrefs.GetInt("Video lod bias");//, settings.videoSettings.lodBias);
                settings.videoSettings.antialiasing = PlayerPrefs.GetInt("Video antialiasing");//, settings.videoSettings.antialiasing);
                //General Settings
                settings.generalSettings.language = PlayerPrefs.GetString("Language");//, settings.generalSettings.language);
                settings.generalSettings.freeCamera = PlayerPrefs.GetInt("freeCamera") == 1;//, settings.generalSettings.freeCamera == true ? 1 : 0);
                settings.generalSettings.sensitivityMouse = PlayerPrefs.GetFloat("mSens");//, settings.generalSettings.sensitivityMouse);
                settings.generalSettings.sensitivityWheelMouse = PlayerPrefs.GetFloat("mWSens");//, settings.generalSettings.sensitivityWheelMouse);
                settings.generalSettings.showHelmet = PlayerPrefs.GetInt("showHelm") == 1;//, settings.generalSettings.showHelmet == true ? 1 : 0);
                settings.generalSettings.saveCredential = PlayerPrefs.GetInt("savec") == 1;//, settings.generalSettings.saveCredential == true ? 1 : 0);
                                                                                           //Quests
                                                                                           // settings.questListSelected = (PlayerPrefs.GetString("qs").Split(';').Select(n => Convert.ToInt32(n)).ToArray()).OfType<long>().ToList();
                Dictionary<long, List<long>> qs = new Dictionary<long, List<long>>();
//                settings.questListSelected = (PlayerPrefs.GetString("qs").Split(';').Select(n => Convert.ToInt32(n)).ToArray()).OfType<long>().ToList();
                settings.questListSelected = PlayerPrefs.GetString("qs").Split(new[] { '!' }, StringSplitOptions.RemoveEmptyEntries)
               .Select(part => part.Split('='))
               .ToDictionary(s => (long)Convert.ToInt32(s[0]), s => (s[1].Split(';').Select(n => Convert.ToInt32(n)).ToArray()).OfType<long>().ToList());
                ;
                //Keys
                settings.keySettings.strafeLeft = (KeyCode)PlayerPrefs.GetInt("ksl");//, settings.keySettings.strafeLeft.ToString());
                settings.keySettings.strafeRight = (KeyCode)PlayerPrefs.GetInt("ksr");//, settings.keySettings.strafeRight.ToString());
                settings.keySettings.moveForward = (KeyCode)PlayerPrefs.GetInt("kmf");//, settings.keySettings.moveForward.ToString());
                settings.keySettings.altMoveForward = (KeyCode)PlayerPrefs.GetInt("kamf");//, settings.keySettings.altMoveForward.ToString());
                settings.keySettings.moveBackward = (KeyCode)PlayerPrefs.GetInt("kmb");//, settings.keySettings.moveBackward.ToString());
                settings.keySettings.altMoveBackward = (KeyCode)PlayerPrefs.GetInt("kamb");//, settings.keySettings.altMoveBackward.ToString());
                settings.keySettings.turnLeft = (KeyCode)PlayerPrefs.GetInt("ktl");//, settings.keySettings.turnLeft.ToString());
                settings.keySettings.altTurnLeft = (KeyCode)PlayerPrefs.GetInt("katl");//, settings.keySettings.altTurnLeft.ToString());
                settings.keySettings.turnRight = (KeyCode)PlayerPrefs.GetInt("ktr");//, settings.keySettings.turnRight.ToString());
                settings.keySettings.altTurnRight = (KeyCode)PlayerPrefs.GetInt("katr");//, settings.keySettings.altTurnRight.ToString());
                settings.keySettings.autoRun = (KeyCode)PlayerPrefs.GetInt("kar");//, settings.keySettings.autoRun.ToString());
                settings.keySettings.walkRun = (KeyCode)PlayerPrefs.GetInt("kwr");//, settings.keySettings.walkRun.ToString());
                settings.keySettings.jump = (KeyCode)PlayerPrefs.GetInt("kju");//, settings.keySettings.jump.ToString());
                settings.keySettings.showWeapon = (KeyCode)PlayerPrefs.GetInt("ksw");//, settings.keySettings.showWeapon.ToString());
                settings.keySettings.inventory = (KeyCode)PlayerPrefs.GetInt("kinv");//, settings.keySettings.inventory.ToString());
                settings.keySettings.character = (KeyCode)PlayerPrefs.GetInt("kchar");//, settings.keySettings.character.ToString());
                settings.keySettings.mail = (KeyCode)PlayerPrefs.GetInt("kmail");//, settings.keySettings.mail.ToString());
                settings.keySettings.guild = (KeyCode)PlayerPrefs.GetInt("kquild");//, settings.keySettings.guild.ToString());
                settings.keySettings.quest = (KeyCode)PlayerPrefs.GetInt("kquest");//, settings.keySettings.quest.ToString());
                settings.keySettings.skiles = (KeyCode)PlayerPrefs.GetInt("kskill");//, settings.keySettings.skiles.ToString());
                settings.keySettings.map = (KeyCode)PlayerPrefs.GetInt("kmap");//, settings.keySettings.map.ToString());
                settings.keySettings.arena = (KeyCode)PlayerPrefs.GetInt("karena");//, settings.keySettings.arena.ToString());
                settings.keySettings.social = (KeyCode)PlayerPrefs.GetInt("ksoc");//, settings.keySettings.social.ToString());
            }
            //  Debug.LogError("Load settings apply");
            applySettings();
            //    Debug.LogError("Load settings seng message");

            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("SETTINGS_LOADED", args);
            //   Debug.LogError("Load settings send message " +settings.credential.p+" "+settings.credential.l+ "  "+settings.generalSettings.saveCredential);
        }

        private void applySettings()
        {
            //Applay Audio Settings
            if (masterMixer != null)
            {
                masterMixer.SetFloat("masterVol", settings.audioSettings.masterLevel);
                masterMixer.SetFloat("musicVol", settings.audioSettings.musicLevel);
                masterMixer.SetFloat("sfxVol", settings.audioSettings.sfxLevel);
                masterMixer.SetFloat("uiVol", settings.audioSettings.uiLevel);
                masterMixer.SetFloat("AmbientVol", settings.audioSettings.ambientLevel);
                masterMixer.SetFloat("FootstepsVol", settings.audioSettings.footstepsLevel);
            }

            //Applay Video Settings
            if (!mainCamera)
                mainCamera = GameObject.Find("MainCamera");
            if (!effectCamera)
                effectCamera = GameObject.Find("EffectCamera");
            //      Screen.SetResolution(gSettings.videoSettings.resolutionWidth, gSettings.videoSettings.resolutionHeight, gSettings.videoSettings.fullscreen);
            QualitySettings.SetQualityLevel(settings.videoSettings.quality, false);
            if (settings.videoSettings.customSettings)
            {
                switch (settings.videoSettings.shadows)
                {
                    case 0:
                        QualitySettings.shadows = ShadowQuality.Disable;
                        break;
                    case 1:
                        QualitySettings.shadows = ShadowQuality.HardOnly;
                        break;
                    case 2:
                        QualitySettings.shadows = ShadowQuality.All;
                        break;
                }
                //   QualitySettings.shadowDistance = settings.videoSettings.shadowDistance;
                switch (settings.videoSettings.shadowDistance)
                {
                    case 0:
                        QualitySettings.shadowDistance = 50;
                        break;
                    case 1:
                        QualitySettings.shadowDistance = 100;
                        break;
                    case 2:
                        QualitySettings.shadowDistance = 150;
                        break;
                    case 3:
                        QualitySettings.shadowDistance = 300;
                        break;
                    case 4:
                        QualitySettings.shadowDistance = 500;
                        break;
                }

                switch (settings.videoSettings.shadowResolution)
                {
                    case 0:
                        QualitySettings.shadowResolution = ShadowResolution.Low;
                        break;
                    case 1:
                        QualitySettings.shadowResolution = ShadowResolution.Medium;
                        break;
                    case 2:
                        QualitySettings.shadowResolution = ShadowResolution.High;
                        break;
                    case 3:
                        QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
                        break;
                }
                QualitySettings.vSyncCount = settings.videoSettings.verticalSync;
                //  QualitySettings.lodBias = settings.videoSettings.lodBias;
                switch (settings.videoSettings.lodBias)
                {
                    case 0:
                        QualitySettings.lodBias = 0.3f;
                        break;
                    case 1:
                        QualitySettings.lodBias = 0.4f;
                        break;
                    case 2:
                        QualitySettings.lodBias = 0.7f;
                        break;
                    case 3:
                        QualitySettings.lodBias = 1f;
                        break;
                    case 4:
                        QualitySettings.lodBias = 1.5f;
                        break;
                    case 5:
                        QualitySettings.lodBias = 2f;
                        break;
                }     //   QualitySettings.particleRaycastBudget = settings.videoSettings.particleRaycastBudget;

                switch (settings.videoSettings.shadowResolution)
                {
                    case 0:
                        QualitySettings.particleRaycastBudget = 4;
                        break;
                    case 1:
                        QualitySettings.particleRaycastBudget = 16;
                        break;
                    case 2:
                        QualitySettings.particleRaycastBudget = 64;
                        break;
                    case 3:
                        QualitySettings.particleRaycastBudget = 256;
                        break;
                    case 4:
                        QualitySettings.particleRaycastBudget = 1024;
                        break;
                    case 5:
                        QualitySettings.particleRaycastBudget = 2048;
                        break;
                }
                QualitySettings.masterTextureLimit = 3 - settings.videoSettings.masterTextureLimit;
                QualitySettings.softParticles = settings.videoSettings.softParticles;
            }
            ApplyCamEffect();
        }

        public void ApplyCamEffect()
        {
#if AT_PPS2_PRESET
        if (postProcessVolumes.Count > 0) {
            foreach(PostProcessVolume ppv in postProcessVolumes) {
                foreach (PostProcessEffectSettings ppes in ppv.sharedProfile.settings){
                    if (ppes.name.Equals("Bloom")) {
                        ppes.enabled.Override(settings.videoSettings.bloom);
                    }
                    if (ppes.name.Equals("DepthOfField")) {
                        ppes.enabled.Override(settings.videoSettings.depthOfField);
                    }
                    if (ppes.name.Equals("Vignette")) {
                        ppes.enabled.Override(settings.videoSettings.vignette);
                    }
                    if (ppes.name.Equals("AmbientOcclusion")) {
                        ppes.enabled.Override(settings.videoSettings.ambientOcclusion);
                    }
                    if (ppes.name.Equals("AutoExposure")) {
                        ppes.enabled.Override(settings.videoSettings.autoExposure);
                    }
                    if (ppes.name.Equals("ChromaticAberration")) {
                        ppes.enabled.Override(settings.videoSettings.chromaticAberration);
                    }
                    if (ppes.name.Equals("ColorGrading")) {
                        ppes.enabled.Override(settings.videoSettings.colorGrading);
                    }
                    if (ppes.name.Equals("Dithering")) {
                        ppes.enabled.Override(settings.videoSettings.dithering);
                    }
                    if (ppes.name.Equals("MotionBlur")) {
                        ppes.enabled.Override(settings.videoSettings.motionBlur);
                    }
                    if (ppes.name.Equals("ScreenSpaceReflections")) {
                        ppes.enabled.Override(settings.videoSettings.screenSpaceReflections);
                    }
                }
            }
        }

        Camera cam = Camera.main;
        if (cam != null) {
            PostProcessLayer ppl = cam.GetComponent<PostProcessLayer>();
            if (ppl != null) {
                    switch (settings.videoSettings.antialiasing) {
                        case 0:
                            ppl.antialiasingMode = PostProcessLayer.Antialiasing.None;
                            break;
                        case 1:
                            ppl.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
                            break;
                        case 2:
                            ppl.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
                            break;
                        case 3:
                            ppl.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                            break;
                    }
            }
/*
            SEScreenSpaceShadows sesss = cam.GetComponent<SEScreenSpaceShadows>();
            if (sesss != null) {
                sesss.enabled = settings.videoSettings.seScreenSpaceShadows;
            }

            VolumetricFog vf = cam.GetComponent<VolumetricFog>();
            if (vf != null) {
                vf.enabled = settings.videoSettings.volumetricFog;
            }
            HxVolumetricCamera hvc = cam.GetComponent<HxVolumetricCamera>();
            if (hvc != null) {
                hvc.enabled = settings.videoSettings.hxVolumetricCamera;
            }
            HxVolumetricImageEffectOpaque hvieo = cam.GetComponent<HxVolumetricImageEffectOpaque>();
            if (hvieo != null) {
                hvieo.enabled = settings.videoSettings.hxVolumetricCamera;
            }*/
        }
#else
            //Debug.Log("");
#endif


#if AT_I2LOC_PRESET
        //  QualitySettings.shadowQuality=
        if (settings.generalSettings.language != null)
            I2.Loc.LocalizationManager.CurrentLanguage = settings.generalSettings.language;
#endif

            Terrain[] terrains = GameObject.FindObjectsOfType<Terrain>();
            if (terrains != null)
                for (int i = 0; i < terrains.Length; i++)
                {
                    terrains[i].detailObjectDensity = settings.videoSettings.detailObjectDensity;
                }
        }
        //  void
        void OnApplicationQuit()
        {
            Debug.Log("Application ending after " + Time.time + " seconds");
            Save();
        }
        void OnDestroy()
        {
            Save();
        }



        void OnApplicationFocus(bool state)
        {
            //  gameFocus = state;
            if (masterMixer)
            {
                if (state)
                {
                    mixerFocus.TransitionTo(.1f);
                    // masterMixer.FindSnapshot("Fokus").TransitionTo(.01f);
                }
                else
                {
                    mixerNoFocus.TransitionTo(.2f);
                    //masterMixer.FindSnapshot("noFokus").TransitionTo(.01f);
                }
            }
        }

        public void ResetWindows()
        {
            settings.windowsSettings.Clear();
            string[] args = new string[1];
            AtavismEventSystem.DispatchEvent("WINDOWS_RESET", args);
        }

        public Vector3 GetWindowPosition(string winName)
        {
            if (settings.windowsSettings.ContainsKey(winName))
            {
                return new Vector3(settings.windowsSettings[winName].x, settings.windowsSettings[winName].y, settings.windowsSettings[winName].z);
            }
            return Vector3.zero;
        }

        public void SetWindowPosition(string winName, Vector3 winPosition)
        {
            if (settings.windowsSettings.ContainsKey(winName))
            {
                settings.windowsSettings[winName].x = winPosition.x;
                settings.windowsSettings[winName].y = winPosition.y;
                settings.windowsSettings[winName].z = winPosition.z;
            }
            else
            {
                AtavismWindowsSettings winSet = new AtavismWindowsSettings();
                winSet.windowName = winName;
                winSet.x = winPosition.x;
                winSet.y = winPosition.y;
                winSet.z = winPosition.z;
                settings.windowsSettings.Add(winName, winSet);
            }
        }

        public static AtavismSettings Instance
        {
            get
            {
                return instance;
            }
        }

        public static bool UIHasFocus()
        {
            if (EventSystem.current.currentSelectedGameObject != null
                && (EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null || EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null))
                return true;
            return false;
        }

          void LoadLoader() {
            if (sceneLoaderPrefab != null)
            {
                GameObject goLoader = GameObject.Find("/"+ sceneLoaderPrefab.name);
                if (goLoader == null)
                    goLoader = Instantiate(sceneLoaderPrefab);
              //  goLoader.name = "CanvasLoader";
                goLoader.transform.SetAsLastSibling();
            }
        }
          
        void LoadUmaWardrobe()
        {
            GameObject goUma = GameObject.Find("/UMA");
            GameObject goPrzeb = GameObject.Find("/Przebieralnia");
            if (UMAGameObject != null)
                if (goUma == null)
                    goUma = Instantiate(UMAGameObject);
            if (wardrobeGameObject != null)
                if (goPrzeb == null)
                    goPrzeb = Instantiate(wardrobeGameObject);

            if (goUma != null)
            {
                goUma.name = "UMA";
                DontDestroyOnLoad(goUma);
            }
            if (goPrzeb != null)
            {
                goPrzeb.name = "Przebieralnia";
                DontDestroyOnLoad(goPrzeb);
                if (CharacterPanelCamera == null)
                {
                    Camera[] cams = goPrzeb.GetComponentsInChildren<Camera>(true);
                    foreach (Camera c in cams)
                    {
                        if (c.name == "CameraCharacterPanel")
                        {
                            CharacterPanelCamera = c;
                            c.enabled = false;
                        }
                        else
                        {
                            OtherCharacterPanelCamera = c;
                            c.enabled = false;
                        }
                    }
                    CapsuleCollider[] ccol = goPrzeb.GetComponentsInChildren<CapsuleCollider>(true);
                    foreach (CapsuleCollider c in ccol)
                    {
                        if (c.name == "CharacterCapsulePoint")
                        {
                            CharacterPanelSpawn = c.transform;
                        }
                        else
                        {
                            OtherCharacterPanelSpawn = c.transform;
                        }
                    }
                }
            }
        }



        public void ClickCreateUrl()
        {
#if AT_I2LOC_PRESET
            if (I2.Loc.LocalizationManager.CurrentLanguage == "English") {
                Application.OpenURL(createURL_EN);
            } else {
                Application.OpenURL(createURL_PL);
            }
#else
            Application.OpenURL(createURL_EN);
#endif
        }
        public void ClickForgotUrl()
        {
#if AT_I2LOC_PRESET
            if (I2.Loc.LocalizationManager.CurrentLanguage == "English") {
                Application.OpenURL(forgotURL_EN);
            } else {
                Application.OpenURL(forgotURL_PL);
            }
#else
            Application.OpenURL(forgotURL_EN);
#endif
        }

        public void ClickWebPageUrl()
        {
#if AT_I2LOC_PRESET
            if (I2.Loc.LocalizationManager.CurrentLanguage == "English") {
                Application.OpenURL(webURL_EN);
            } else {
                Application.OpenURL(webURL_PL);
            }
#else
            Application.OpenURL(webURL_EN);
#endif
        }
        public void ClickShopWebPageUrl()
        {
#if AT_I2LOC_PRESET
            if (I2.Loc.LocalizationManager.CurrentLanguage == "English") {
                Application.OpenURL(webURL_EN);
            } else {
                Application.OpenURL(webURL_PL);
            }
#else
            Application.OpenURL(webURL_EN);
#endif
        }


        public Dictionary<long, List<long>> GetQuestListSelected()
        {
            if (settings.questListSelected == null)
                settings.questListSelected = new Dictionary<long, List<long>>();
            return settings.questListSelected;
        }
        public AtCred GetCredentials()
        {
            return settings.credential;
        }

        public AtavismGeneralSettings GetGeneralSettings()
        {
            return settings.generalSettings;
        }
        public AtavismAudioSettings GetAudioSettings()
        {
            return settings.audioSettings;
        }
        public AtavismVideoSettings GetVideoSettings()
        {
            return settings.videoSettings;
        }

        public AtavismKeySettings GetKeySettings()
        {
            return settings.keySettings;
        }
        public Camera GetCharacterPanelCamera()
        {
            return CharacterPanelCamera;
        }
        public Camera GetOtherCharacterPanelCamera()
        {
            return OtherCharacterPanelCamera;
        }
        public Transform GetCharacterPanelSpawn()
        {
            return CharacterPanelSpawn;
        }
        public Transform GetOtherCharacterPanelSpawn()
        {
            return OtherCharacterPanelSpawn;
        }

        public int GetQuestPrevLimit
        {
            get
            {
                return questPrevLimit;
            }
        }

            private bl_MiniMap _minimap = null;

            public bl_MiniMap MiniMap {
                set {
                    _minimap = value;
                }
                get {
                    return _minimap;
                }
            }
            private bl_MaskHelper _maskHelper = null;

            public bl_MaskHelper MaskHelper {
                set { _maskHelper = value; }
                get { return _maskHelper; }
            }
        
        public List<string> GameInstances
        {
            get
            {
                return gameInstances;
            }
        }
        public List<string> ArenaInstances
        {
            get
            {
                return arenaInstances;
            }
        }
        public List<string> DungeonInstances
        {
            get
            {
                return dungeonInstances;
            }
        }

           public DsMiniMapSettings MinimapSettings {
               get { return minimapSettings; }
           }
        public GameObject LevelUpPrefab
        {
            get
            {
                return levelUpPrefab;
            }
        }
        public float LevelUpPrefabDuration
        {
            get
            {
                return levelUpPrefabDuration;
            }
        }
        public AtavismQualitySetingsDefault GetDefaultQuality(int id)
        {
            return _defaultQualitySettings[id];
        }

        public bool NameVisable
        {
            get
            {
                return visibleMobsName;
            }
        }
        public TMP_FontAsset MobNameFont
        {
            get
            {
                return mobNameFont;
            }
        }
        public Vector3 MobNamePosition
        {
            get
            {
                return mobNamePosition;
            }
        }
        public int MobNameFontSize
        {
            get
            {
                return mobNameFontSize;
            }
        }
        public Color MobNameDefaultColor
        {
            get
            {
                return mobNameDefaultColor;
            }
        }
        public TextAlignmentOptions MobNameAlignment
        {
            get
            {
                return mobNameAlignment;
            }
        }
        public Vector4 MobNameMargin
        {
            get
            {
                return mobNameMargin;
            }
        }
        public float MobNameOutlineWidth
        {
            get
            {
                return mobNameOutlineWidth;
            }
        }
         public string QuestNewText
        {
            get
            {
                return questNewText;
            }
        }
        public string QuestConcludableText
        {
            get
            {
                return questConcludableText;
            }
        }
        public string QuestProgressText
        {
            get
            {
                return questProgressText;
            }
        }
        public string ShopText
        {
            get
            {
                return shopText;
            }
        }
        public string BankText
        {
            get
            {
                return bankText;
            }
        }
        public TMP_SpriteAsset GetSpriteAsset
        {
            get
            {
                return npcInfoSpriteAsset;
            }
        }
        public int GetNpcInfoTextSize
        {
            get
            {
                return npcInfoTextSize;
            }
        }
        public Color GetNpcInfoTextColor
        {
            get
            {
                return npcInfoTextColor;
            }
        }
        public Vector3 GetNpcInfoTextPosition
        {
            get
            {
                return npcInfoTextPosition;
            }
        }
        
       GameObject _contextMenu;
        /// <summary>
        /// Przechowuje otwarte menu kontextowe wcelu zamkniecia w przypadku otwarciz innego
        /// </summary>
        /// <param name="obj"></param>
        public void DsContextMenu(GameObject obj)
        {
            if (obj != null)
            {
                if (_contextMenu != null && _contextMenu != obj)
                    _contextMenu.SetActive(false);
                _contextMenu = obj;
            }
            else
            {
                if (_contextMenu != null)
                {
                    _contextMenu.SetActive(false);
                    _contextMenu = null;
                }
            }
        }
        
        public DsAdminPanelSettings GetAdminLocations()
        {
            foreach (DsAdminPanelSettings s in _adminLocationSettings)
            {
                if (s.InstanceName == SceneManager.GetActiveScene().name)
                    return s;
            }
            return null;
        }
        public Color ItemQualityColor(int index)
        {
            if (itemQualityColor.Count - 1 < index)
            {
                //    Debug.LogError("Add Color to Item Quality Color in AtavismSettings");
                return Color.white;
            }

            return itemQualityColor[index-1];

        }
        public GameObject CharacterAvatar
        {
            get
            {
                return characterAvatar;
            }
            set
            {
                characterAvatar = value;
            }
        }
        public GameObject OtherCharacterAvatar
        {
            get
            {
                return characterAvatar;
            }
            set
            {
                characterAvatar = value;
            }
        }
        public Sprite[] Avatars(string Race,string Gender, string Class)
        {
            foreach(RaceAvatar ra in races)
            {
                if (ra.name.Equals(Race))
                {
                    foreach (GenderAvatar ga in ra.genders)
                    {
                        if (ga.name.Equals(Gender))
                        {
                            foreach (ClassAvatar ca in ga.classes)
                            {
                                if (ca.name.Equals(Class))
                                {
                                    return ca.avatars;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        public Sprite Avatar(string path)
        {
            foreach (RaceAvatar ra in races)
            {
                foreach (GenderAvatar ga in ra.genders)
                {
                    foreach (ClassAvatar ca in ga.classes)
                    {
                        foreach (Sprite s in ca.avatars)
                        {
                            if(s!=null)
                            if (s.name.Equals(path))
                            {
                                return s;
                            }
                        }
                    }
                }
            }
            return null;
        }

#if AT_PPS2_PRESET
    public List<PostProcessVolume> PostProcessVolumes
    {
        set
        {
            postProcessVolumes = value;
            ApplyCamEffect();
        }
        get
        {
            return postProcessVolumes;
        }
    }
#endif
    }
}