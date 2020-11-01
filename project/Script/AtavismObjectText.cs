using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;

namespace Atavism
{

    public class AtavismObjectText : MonoBehaviour
    {
        [SerializeField] float height = 2f;
        GameObject objectText;
        GameObject mobQuest;
        GameObject obj;
        [SerializeField] string textfield;
        [SerializeField] float renderDistance = 50f;
        [SerializeField] float minFontSize = 1f;
        [SerializeField] float maxFontSize = 5f;
        [SerializeField] Color textColour = Color.green;
        [SerializeField] Vector3 textPosition = Vector3.zero;
        [SerializeField] Vector4 textMargin = Vector4.zero;
        //    bool upadteIsRunning = false;
        //   Coroutine chTimer;
        // Use this for initialization
        void Start()
        {
            //   node = GetComponent<AtavismNode>();
            obj = new GameObject("obj");
            obj.layer = LayerMask.NameToLayer(AtavismCursor.Instance.layerForTexts);
            obj.transform.SetParent(transform, false);
            objectText = new GameObject("ObjectText");
            objectText.layer = LayerMask.NameToLayer(AtavismCursor.Instance.layerForTexts);
            objectText.transform.SetParent(obj.transform, false);

            obj.transform.localPosition = new Vector3(0f, height, 0f);
            objectText.transform.localPosition = textPosition;
            TextMeshPro textMeshPro = objectText.AddComponent<TextMeshPro>();
            textMeshPro.alignment = TextAlignmentOptions.Midline;
            textMeshPro.margin = textMargin;
            textMeshPro.fontSize = 2;

            textMeshPro.color = textColour;
            TMP_FontAsset font1 = Resources.Load("Lato-BoldSDFNames", typeof(TMP_FontAsset)) as TMP_FontAsset;
            textMeshPro.font = font1;
            textMeshPro.outlineWidth = 0.2f;

#if AT_I2LOC_PRESET
           if (!string.IsNullOrEmpty(I2.Loc.LocalizationManager.GetTranslation(textfield)))  objectText.GetComponent<TextMeshPro>().text = I2.Loc.LocalizationManager.GetTranslation(textfield);
#else
            textMeshPro.text = textfield;
#endif

            AtavismEventSystem.RegisterEvent("UPDATE_LANGUAGE", this);
            if (!AtavismSettings.Instance.NameVisable)
            {
                textMeshPro.enabled = false;
            }
            StartCoroutine(UpdateTimer());
        }

        private void Update()
        {
            if (obj != null)
                if (obj.transform.localPosition != new Vector3(0f, height, 0f))
                    obj.transform.localPosition = new Vector3(0f, height, 0f);
            if (objectText != null && objectText.GetComponent<TextMeshPro>() != null)
            {
                if (objectText.GetComponent<TextMeshPro>().margin != textMargin)
                    objectText.GetComponent<TextMeshPro>().margin = textMargin;
                if (objectText.transform.localPosition != textPosition)
                    objectText.transform.localPosition = textPosition;
                if (objectText.GetComponent<TextMeshPro>().color != textColour)
                    objectText.GetComponent<TextMeshPro>().color = textColour;

            }
        }


        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "UPDATE_LANGUAGE")
            {
#if AT_I2LOC_PRESET
            if (!string.IsNullOrEmpty(I2.Loc.LocalizationManager.GetTranslation(textfield))) objectText.GetComponent<TextMeshPro>().text = I2.Loc.LocalizationManager.GetTranslation(textfield);
#endif
            }
        }


        IEnumerator UpdateTimer()
        {
            WaitForSeconds delay = new WaitForSeconds(0.02f);
            //   upadteIsRunning = true;
            while (Camera.main == null)
            {
                yield return delay;
            }
            while (Camera.main != null)
            {
                if (objectText != null)
                {
                    float distance = Vector3.Distance(objectText.transform.position, Camera.main.transform.position);
                    if (distance < renderDistance)
                    {
                        objectText.GetComponent<TextMeshPro>().enabled = true;
                        objectText.transform.rotation = Camera.main.transform.rotation;
                        objectText.GetComponent<TextMeshPro>().fontSize = minFontSize + distance * (maxFontSize - minFontSize) / renderDistance;
                    }
                    else
                        objectText.GetComponent<TextMeshPro>().enabled = false;
                }
                yield return delay;
            }
            //  upadteIsRunning = false;
            yield return 0f;
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
            AtavismEventSystem.UnregisterEvent("UPDATE_LANGUAGE", this);
            StopAllCoroutines();
        }
    }
}