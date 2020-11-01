using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace Atavism
{
    public class AtavismGetModel : MonoBehaviour
    {
        [SerializeField]
        string modelName = "";
        [SerializeField]
        Vector3 ScaleForModel = Vector3.one;
        GameObject m_model = null;
        // Use this for initialization
        void Start()
        {
            if (modelName == "")
            {
                AtavismLogger.LogError("AtavismGetModel No Model Name :" + name);
                return;
            }
            StartCoroutine(GetModel(modelName));
        }

        IEnumerator GetModel(string modelName)
        {
            //     Profiler.BeginSample("AtavismGetModel.GetModel");
            WaitForSeconds delay = new WaitForSeconds(0.5f);
            while (m_model == null)
            {
                if (AtavismAssetBundlesManager.Instance != null)
                {
                    var model = AtavismAssetBundlesManager.Instance.GetModel(modelName);
                    if (model != null)
                    {
                        //  Profiler.BeginSample("AtavismGetModel.GetModel.Instantiate");
                        m_model = (GameObject)Instantiate(model, transform);
                        m_model.GetComponent<Transform>().localScale = ScaleForModel;
                        m_model.GetComponent<Transform>().localPosition = Vector3.zero;
                        m_model.GetComponent<Transform>().localRotation = new Quaternion(0f, 0f, 0f, 0f);

                        AtavismMobSockets ams = m_model.GetComponent<AtavismMobSockets>();
                        if (ams != null)
                        {
                            AtavismMobAppearance ama = GetComponent<AtavismMobAppearance>();
                            if (ama != null)
                            {
                                ama.SetupSockets(ams);
                            }
                        }

                        //     Profiler.EndSample();
                    }
                    //   else { Debug.LogError("AtavismGetModel.GetModel No Model: " + modelName); }
                    // } else { AtavismLogger.LogError("AtavismGetModel.GetModel No Model"); }
                }
                yield return delay;
            }
            //  Profiler.EndSample();

        }
    }
}