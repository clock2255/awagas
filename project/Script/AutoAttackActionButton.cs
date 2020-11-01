using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Atavism
{

    [System.Obsolete("Class Obsotete Use CoordProjectileEffect instead CoordObjectEffect", true)]
    public class AutoAttackActionButton : MonoBehaviour
    {

        public KeyCode activateKey;
        AtavismAction action;

        // Use this for initialization
        void Start()
        {
            ClientAPI.GetPlayerObject().RegisterPropertyChangeHandler("combat.autoability", AutoAttackAbilityHandler);

            if (ClientAPI.GetPlayerObject() != null)
            {
                if (ClientAPI.GetPlayerObject().PropertyExists("combat.autoability"))
                {
                    int abilityID = (int)ClientAPI.GetPlayerObject().GetProperty("combat.autoability");
                    UpdateButton(abilityID);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(activateKey) && !ClientAPI.UIHasFocus())
            {
                Activate();
            }
        }

        public void Activate()
        {
            if (action != null)
                action.Activate();
        }

        public void AutoAttackAbilityHandler(object sender, PropertyChangeEventArgs args)
        {
            int abilityID = (int)ClientAPI.GetPlayerObject().GetProperty("combat.autoability");
            UpdateButton(abilityID);
        }

        void UpdateButton(int abilityID)
        {
            if (abilityID > 0)
            {
                AtavismAbility ability = Abilities.Instance.GetAbility(abilityID);
                action = new AtavismAction();
                action.actionObject = ability;
                GetComponent<Button>().image.sprite = ability.icon;
                GetComponent<Button>().image.color = new Color(1, 1, 1, 1);
            }
            else
            {
                GetComponent<Button>().image.color = new Color(0, 0, 0, 0);
            }
        }
    }
}