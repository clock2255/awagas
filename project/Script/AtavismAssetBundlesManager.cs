using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

namespace Atavism
{
    public class AtavismAssetBundlesManager : MonoBehaviour
    {
        static AtavismAssetBundlesManager instance;
        [SerializeField]
        List<string> listBundlesNames = new List<string>();
        private List<AssetBundle> assetBundles;
        Dictionary<string, GameObject> modelsAssets = new Dictionary<string, GameObject>();

        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;
            assetBundles = new List<AssetBundle>();
            StartCoroutine(LoadAssets());
        }

        private IEnumerator LoadAssets()
        {
            //        Profiler.BeginSample("AtavismAssetBundlesManager.LoadAssets");
            foreach (string asset in listBundlesNames)
            {
                if (!String.IsNullOrEmpty(asset))
                {
                    if (File.Exists(Path.Combine(Application.streamingAssetsPath, asset)))
                    {
                        var mbundleLoadRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, asset));
                        yield return mbundleLoadRequest;
                        AssetBundle m_asset = mbundleLoadRequest.assetBundle;
                        if (m_asset == null)
                        {
                            AtavismLogger.LogError("Failed to load Asset " + asset + "!");
                            yield break;
                        }
                        if (m_asset != null)
                            if (!assetBundles.Contains(m_asset))
                                assetBundles.Add(m_asset);
                    }
                    else
                    {
                        Debug.LogError("Asset Bundle File " + asset + " not exist");
                    }
                }
            }
            //        Profiler.EndSample();
        }

        public GameObject GetModel(string modelName)
        {
            //        Profiler.BeginSample("AtavismAssetBundlesManager.GetMobModel");
            GameObject model = null;
            if (modelsAssets.ContainsKey(modelName))
            {
                //            Profiler.BeginSample("AtavismAssetBundlesManager.GetModel");
                model = modelsAssets[modelName];
                //            Profiler.EndSample();
            }
            else
            {
                if (assetBundles.Count == 0)
                {
                    //                AtavismLogger.LogError("AtavismAssetBundlesManager.GetModel AssetsBundles not Loaded ");
                    return null;
                }
                //            AtavismLogger.LogDebugMessage("AtavismAssetBundlesManager.GetModel " + modelName);
                //            Profiler.BeginSample("AtavismAssetBundlesManager.GetModel");
                foreach (AssetBundle asset in assetBundles)
                {
                    if (asset != null)
                    {
                        AssetBundleRequest ss = asset.LoadAssetAsync<GameObject>(modelName);
                        model = ss.asset as GameObject;
                        if (model != null)
                            break;
                    }
                    else
                    {
                        //                    AtavismLogger.LogError("AtavismAssetBundlesManager.GetModel AssetsBundle is null ");
                    }
                }
                //            Profiler.EndSample();
                //            Profiler.BeginSample("AtavismAssetBundlesManager.GetModel store readed model");
                if (model != null)
                    modelsAssets.Add(modelName, model);
                //            Profiler.EndSample();
            }
            //        Profiler.EndSample();
            return model;
        }

        public static AtavismAssetBundlesManager Instance
        {
            get
            {
                return instance;
            }
        }
    }
}