using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using TMPro;

namespace Atavism
{

    public enum LoginState
    {
        Login,
        Register,
        Authenticating,
        CharacterSelect,
        CharacterCreate
    }

    public class LoginController : MonoBehaviour
    {

        public GameObject loginPanel;
        public GameObject registerPanel;
        public UGUIDialogPopup dialogPanel;
        public GameObject soundMenu;
        public GameObject musicObject;
        public Texture cursorOverride;
        public bool useMd5Encryption = false;
        string characterScene = "CharacterSelection";
        [SerializeField] InputField userField;
        [SerializeField] TMP_InputField TMPUserField;
        [SerializeField] InputField passField;
        [SerializeField] TMP_InputField TMPPassField;
        [SerializeField] Toggle savePassToggle;
        [SerializeField] Text messageText;
        [SerializeField] TextMeshProUGUI TMPMessageText;
        float loginclick;
        #region fields
        LoginState loginState;

        // Login fields
        string username = "";
        string password = "";

        // Registration fields
        string regUsername = "";
        string regPassword = "";
        string password2 = "";
        string email = "";
        string email2 = "";
        bool loaded = false;
        #endregion fields


        // Use this for initialization
        void Start()
        {
            loginState = LoginState.Login;
            AtavismEventSystem.RegisterEvent("LOGIN_RESPONSE", this);
            AtavismEventSystem.RegisterEvent("REGISTER_RESPONSE", this);
            AtavismEventSystem.RegisterEvent("SETTINGS_LOADED", this);
            // Play music
            SoundSystem.LoadSoundSettings();
            if (musicObject != null)
                SoundSystem.PlayMusic(musicObject.GetComponent<AudioSource>());

        }

        private void OnEnable()
        {
            if (AtavismSettings.Instance != null)
            {
                if (savePassToggle != null)
                    savePassToggle.isOn = AtavismSettings.Instance.GetGeneralSettings().saveCredential;
                if (userField != null)
                    userField.text = AtavismSettings.Instance.GetCredentials().l;
                if (passField != null)
                    passField.text = AtavismSettings.Instance.GetCredentials().p;
                if (TMPUserField != null)
                    TMPUserField.text = AtavismSettings.Instance.GetCredentials().l;
                if (TMPPassField != null)
                    TMPPassField.text = AtavismSettings.Instance.GetCredentials().p;
            }

        }
        public void ToggleSave()
        {
            if (savePassToggle != null)
                AtavismSettings.Instance.GetGeneralSettings().saveCredential = savePassToggle.isOn;
        }

        void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("LOGIN_RESPONSE", this);
            AtavismEventSystem.UnregisterEvent("REGISTER_RESPONSE", this);
            AtavismEventSystem.UnregisterEvent("SETTINGS_LOADED", this);
        }

        void OnGUI()
        {
            if (cursorOverride != null)
            {
                UnityEngine.Cursor.visible = false;
                GUI.DrawTexture(new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y, 32, 32), cursorOverride);
            }
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (AtavismSettings.UIHasFocus())
                {
                    if (loginState == LoginState.Login)
                    {
                        Login();
                    }
                }
            }
            if (!loaded)
                if (AtavismSettings.Instance != null)
                {
                    if (savePassToggle != null)
                        savePassToggle.isOn = AtavismSettings.Instance.GetGeneralSettings().saveCredential;
                    if (AtavismSettings.Instance.GetGeneralSettings().saveCredential)
                    {
                        if (userField != null)
                            userField.text = AtavismSettings.Instance.GetCredentials().l;
                        if (passField != null)
                            passField.text = AtavismSettings.Instance.GetCredentials().p;
                        if (TMPUserField != null)
                            TMPUserField.text = AtavismSettings.Instance.GetCredentials().l;
                        if (TMPPassField != null)
                            TMPPassField.text = AtavismSettings.Instance.GetCredentials().p;
                    }
                    loaded = true;
                }

        }

        public void ShowLoginPanel()
        {
            loginState = LoginState.Login;
            loginPanel.SetActive(true);
            registerPanel.SetActive(false);
        }

        public void ShowRegisterPanel()
        {
            loginState = LoginState.Register;
            loginPanel.SetActive(false);
            registerPanel.SetActive(true);
        }

        public void CancelRegistration()
        {
            ShowLoginPanel();
        }

        public void SetUserName(string username)
        {
            if (loginState == LoginState.Login)
            {
                this.username = username;
            }
            else
            {
                this.regUsername = username;
            }
        }

        public void SetPassword(string password)
        {
            if (loginState == LoginState.Login)
            {
                this.password = password;
            }
            else
            {
                this.regPassword = password;
            }
        }

        public void SetPassword2(string password2)
        {
            this.password2 = password2;
        }

        public void SetEmail(string email)
        {
            this.email = email;
        }

        public void SetEmail2(string email2)
        {
            this.email2 = email2;
        }

        public void Login()
        {
            if (username == "")
            {
#if AT_I2LOC_PRESET
   			ShowDialog(I2.Loc.LocalizationManager.GetTranslation("Please enter a username"), true);

#else
                ShowDialog("Please enter a username", true);
#endif
                return;
            }
            if (loginclick > Time.time)
                return;
            loginclick = Time.time + 1f;

            //ShowDialog("Logging In...", false);
            Dictionary<string, object> props = new Dictionary<string, object>();
            /*props.Add("stringprop", "hi");
            props.Add("intprop", 5);
            props.Add("boolprop", true);
            props.Add("floatprop", -0.3f);*/
            if (AtavismSettings.Instance.GetGeneralSettings().saveCredential)
            {
                AtavismSettings.Instance.GetCredentials().l = username;
                AtavismSettings.Instance.GetCredentials().p = password;
            }
            if (useMd5Encryption)
            {
                AtavismClient.Instance.Login(username, AtavismEncryption.Md5Sum(password), props);
            }
            else
            {
                AtavismClient.Instance.Login(username, password, props);
            }
        }

        public void Register()
        {
            if (regUsername == "")
            {
#if AT_I2LOC_PRESET
			ShowDialog(I2.Loc.LocalizationManager.GetTranslation("Please enter a username"), true);
#else
                ShowDialog("Please enter a username", true);
#endif
                return;
            }
            if (regUsername.Length < 4)
            {
#if AT_I2LOC_PRESET
			ShowDialog(I2.Loc.LocalizationManager.GetTranslation("Your username must be at least 4 characters long"), true);
#else
                ShowDialog("Your username must be at least 4 characters long", true);
#endif
                return;
            }
            foreach (char chr in regUsername)
            {
                if ((chr < 'a' || chr > 'z') && (chr < 'A' || chr > 'Z') && (chr < '0' || chr > '9'))
                {
#if AT_I2LOC_PRESET
				ShowDialog(I2.Loc.LocalizationManager.GetTranslation("Your username can only contain letters and numbers"), true);
#else
                    ShowDialog("Your username can only contain letters and numbers", true);
#endif
                    return;
                }
            }
            if (regPassword == "")
            {
#if AT_I2LOC_PRESET
			ShowDialog(I2.Loc.LocalizationManager.GetTranslation("Please enter a password"), true);
#else
                ShowDialog("Please enter a password", true);
#endif
                return;
            }
            foreach (char chr in regPassword)
            {
                if (chr == '*' || chr == '\'' || chr == '"' || chr == '/' || chr == '\\' || chr == ' ')
                {
#if AT_I2LOC_PRESET
				ShowDialog(I2.Loc.LocalizationManager.GetTranslation("Your password cannot contain * \' \" / \\ or spaces"), true);
#else
                    ShowDialog("Your password cannot contain * \' \" / \\ or spaces", true);
#endif
                    return;
                }
            }
            if (regPassword.Length < 6)
            {
#if AT_I2LOC_PRESET
			ShowDialog(I2.Loc.LocalizationManager.GetTranslation("Your password must be at least 6 characters long"), true);
#else
                ShowDialog("Your password must be at least 6 characters long", true);
#endif
                return;
            }
            if (regPassword != password2)
            {
#if AT_I2LOC_PRESET
			ShowDialog(I2.Loc.LocalizationManager.GetTranslation("Your passwords must match"), true);
#else
                ShowDialog("Your passwords must match", true);
#endif
                return;
            }
            if (email == "")
            {
#if AT_I2LOC_PRESET
			ShowDialog(I2.Loc.LocalizationManager.GetTranslation("Please enter an email address"), true);
#else
                ShowDialog("Please enter an email address", true);
#endif
                return;
            }
            if (!ValidateEmail(email))
            {
#if AT_I2LOC_PRESET
			ShowDialog(I2.Loc.LocalizationManager.GetTranslation("Please enter a valid email address"), true);
#else
                ShowDialog("Please enter a valid email address", true);
#endif
                return;
            }
            if (email != email2)
            {
#if AT_I2LOC_PRESET
			ShowDialog(I2.Loc.LocalizationManager.GetTranslation("Your email addresses must match"), true);
#else
                ShowDialog("Your email addresses must match", true);
#endif
                return;
            }
            if (useMd5Encryption)
            {
                AtavismClient.Instance.CreateAccount(regUsername, AtavismEncryption.Md5Sum(regPassword), email);
            }
            else
            {
                AtavismClient.Instance.CreateAccount(regUsername, regPassword, email);
            }

        }

        private bool ValidateEmail(string email)
        {
            // Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$");
            Match match = regex.Match(email);
            if (match.Success)
                return true;
            else
                return false;
        }


        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "LOGIN_RESPONSE")
            {
                if (eData.eventArgs[0] == "Success")
                {
                    //Application.LoadLevel(characterScene);
                }
                else
                {
                    string errorType = eData.eventArgs[0];
#if AT_I2LOC_PRESET
				string errorMessage = I2.Loc.LocalizationManager.GetTranslation(errorType);
#else
                    string errorMessage = errorType;
#endif
                    if (errorType == "LoginFailure")
                    {
#if AT_I2LOC_PRESET
					errorMessage = I2.Loc.LocalizationManager.GetTranslation("Invalid username or password");
#else
                        errorMessage = "Invalid username or password";
#endif
                    }
                    else if (errorType == "NoAccessFailure")
                    {
#if AT_I2LOC_PRESET
					errorMessage = I2.Loc.LocalizationManager.GetTranslation("Your account does not have access to log in");
#else
                        errorMessage = "Your account does not have access to log in";
#endif
                    }
                    else if (errorType == "BannedFailure")
                    {
#if AT_I2LOC_PRESET
					errorMessage = I2.Loc.LocalizationManager.GetTranslation("Your account has been banned");
#else
                        errorMessage = "Your account has been banned";
#endif
                    }
                    else if (errorType == "SubscriptionExpiredFailure")
                    {
#if AT_I2LOC_PRESET
					errorMessage = I2.Loc.LocalizationManager.GetTranslation("Your account does not have an active subscription");
#else
                        errorMessage = "Your account does not have an active subscription";
#endif
                    }
                    ShowDialog(errorMessage, true);
                }
            }
            else if (eData.eventType == "REGISTER_RESPONSE")
            {
                if (eData.eventArgs[0] == "Success")
                {
                    ShowLoginPanel();
#if AT_I2LOC_PRESET
				ShowDialog(I2.Loc.LocalizationManager.GetTranslation("Account created. You can now log in"), true);
#else
                    ShowDialog("Account created. You can now log in", true);
#endif
                }
                else
                {
                    string errorType = eData.eventArgs[0];
                    string errorMessage = errorType;
                    if (errorType == "UsernameUsed")
                    {
#if AT_I2LOC_PRESET
					errorMessage = I2.Loc.LocalizationManager.GetTranslation("An account with that username already exists");
#else
                        errorMessage = "An account with that username already exists";
#endif
                    }
                    else if (errorType == "EmailUsed")
                    {
#if AT_I2LOC_PRESET
					errorMessage = I2.Loc.LocalizationManager.GetTranslation("An account with that email address already exists");
#else
                        errorMessage = "An account with that email address already exists";
#endif
                    }
                    else if (errorType == "Unknown")
                    {
#if AT_I2LOC_PRESET
					errorMessage = I2.Loc.LocalizationManager.GetTranslation("Unknown error. Please let the Dragonsan team know");
#else
                        errorMessage = "Unknown error. Please let the Dragonsan team know";
#endif
                    }
                    else if (errorType == "MasterTcpConnectFailure")
                    {
#if AT_I2LOC_PRESET
					errorMessage = I2.Loc.LocalizationManager.GetTranslation("Unable to connect to the Authentication Server");
#else
                        errorMessage = "Unable to connect to the Authentication Server";
#endif
                    }
                    else if (errorType == "NoAccessFailure")
                    {
#if AT_I2LOC_PRESET
					errorMessage = I2.Loc.LocalizationManager.GetTranslation("Account creation has been disabled on this server");
#else
                        errorMessage = "Account creation has been disabled on this server";
#endif
                    }
                    ShowDialog(errorMessage, true);
                }
            }
            else if (eData.eventType == "SETTINGS_LOADED")
            {
                if (savePassToggle != null)
                    savePassToggle.isOn = AtavismSettings.Instance.GetGeneralSettings().saveCredential;
                if (userField != null && AtavismSettings.Instance.GetGeneralSettings().saveCredential)
                    userField.text = AtavismSettings.Instance.GetCredentials().l;
                if (passField != null && AtavismSettings.Instance.GetGeneralSettings().saveCredential)
                    passField.text = AtavismSettings.Instance.GetCredentials().p;

            }
        }

        void ShowDialog(string message, bool showButton)
        {
            if (dialogPanel == null)
            {
                if (messageText != null)
                    messageText.text = message;
                return;
            }
            dialogPanel.gameObject.SetActive(true);
            dialogPanel.ShowDialogPopup(message, showButton);
        }

        void ShowDialogWithButton(string message)
        {
            if (dialogPanel == null)
            {
                if (messageText != null)
                    messageText.text = message;
                return;
            }
            dialogPanel.gameObject.SetActive(true);
            dialogPanel.ShowDialogPopup(message, true);
        }

        public string CharacterScene
        {
            get
            {
                return characterScene;
            }
            set
            {
                characterScene = value;
            }
        }
    }
}