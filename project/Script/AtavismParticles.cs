using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{
    public class AtavismParticles : MonoBehaviour
    {

        static AtavismParticles instance;

        public GameObject lootParticle;
        Dictionary<long, GameObject> attachedLootParticles = new Dictionary<long, GameObject>();

        // Use this for initialization
        void Start()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;
        }

        void ClientReady()
        {
            ClientAPI.WorldManager.RegisterObjectPropertyChangeHandler("lootable", LootParticlesHandler);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void LootParticlesHandler(object sender, ObjectPropertyChangeEventArgs args)
        {
            AtavismObjectNode worldObj = ClientAPI.WorldManager.GetObjectNode(args.Oid);
            if (worldObj == null)
            {
                AtavismLogger.LogWarning("Loot Particles: found no object");
                return;
            }
            if (worldObj.CheckBooleanProperty("lootable"))
            {
                GameObject newLootParticle = (GameObject)GameObject.Instantiate(lootParticle, worldObj.GameObject.transform.position, Quaternion.identity);
                newLootParticle.transform.parent = worldObj.GameObject.transform;
                if (attachedLootParticles.ContainsKey(args.Oid))
                {
                    if (attachedLootParticles[args.Oid] != null)
                    {
                        Destroy(attachedLootParticles[args.Oid]);
                    }
                        attachedLootParticles.Remove(args.Oid);
                }
                attachedLootParticles.Add(args.Oid, newLootParticle);
            }
            else if (attachedLootParticles.ContainsKey(args.Oid))
            {
                Destroy(attachedLootParticles[args.Oid]);
                attachedLootParticles.Remove(args.Oid);
            }
        }
    }
}