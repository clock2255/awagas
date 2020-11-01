using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Atavism
{
    public class AtavismLevelUp : MonoBehaviour
    {
        AtavismNode node;
        [SerializeField] int level = 0;
        GameObject go;
        float timeDelay;

        void Start()
        {
            node = GetComponent<AtavismNode>();
            if (node != null)
            {
                if (node.PropertyExists("level"))
                    level = (int)node.GetProperty("level");
                node.RegisterObjectPropertyChangeHandler("level", LevelHandler);
            }
        }

        public void LevelHandler(object sender, PropertyChangeEventArgs args)
        {
            if (node != null)
            {
                if (level == 0)
                    level = (int)node.GetProperty("level");
                if (level != (int)node.GetProperty("level"))
                {
                    if (level != 0 && timeDelay < Time.time)
                        if (AtavismSettings.Instance.LevelUpPrefab != null)
                            go = (GameObject)Instantiate(AtavismSettings.Instance.LevelUpPrefab, transform.position, transform.rotation, transform);
                    level = (int)node.GetProperty("level");
                    timeDelay = Time.time + 1f;
                }
            }
            if (go != null)
                Destroy(go, AtavismSettings.Instance.LevelUpPrefabDuration);
        }

        void OnDestroy()
        {
            if (node != null)
            {
                node.RemoveObjectPropertyChangeHandler("level", LevelHandler);
            }
        }
    }
}