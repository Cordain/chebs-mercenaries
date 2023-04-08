using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using ChebsValheimLibrary.Minions;
using UnityEngine;

namespace ChebsMercenaries.Minions
{
    public class HumanMinion : ChebGonazMinion
    {
        public static ConfigEntry<DropType> DropOnDeath;
        public static ConfigEntry<bool> PackDropItemsIntoCargoCrate;
        public static ConfigEntry<bool> Commandable;
        public static ConfigEntry<float> FollowDistance, RunDistance;

        public static void CreateConfigs(BaseUnityPlugin plugin)
        {
            DropOnDeath = plugin.Config.Bind("HumanMinion (Server Synced)", 
                "DropOnDeath",
                DropType.JustResources, new ConfigDescription("Whether a minion refunds anything when it dies.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

            PackDropItemsIntoCargoCrate = plugin.Config.Bind("HumanMinion (Server Synced)", 
                "PackDroppedItemsIntoCargoCrate",
                true, new ConfigDescription("If set to true, dropped items will be packed into a cargo crate. This means they won't sink in water, which is useful for more valuable drops like Surtling Cores and metal ingots.", null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
            
            Commandable = plugin.Config.Bind("HumanMinion (Client)", "Commandable",
                true, new ConfigDescription("If true, minions can be commanded individually with E (or equivalent) keybind."));
            
            FollowDistance = plugin.Config.Bind("HumanMinion (Client)", "FollowDistance",
                3f, new ConfigDescription("How closely a minion will follow you (0 = standing on top of you, 3 = default)."));
            
            RunDistance = plugin.Config.Bind("HumanMinion (Client)", "RunDistance",
                3f, new ConfigDescription("How close a following minion needs to be to you before it stops running and starts walking (0 = always running, 10 = default)."));
        }

        private void Awake()
        {
            StartCoroutine(WaitForZNet());
        }

        IEnumerator WaitForZNet()
        {
            yield return new WaitUntil(() => ZNetScene.instance != null);

            if (!TryGetComponent(out Humanoid humanoid))
            {
                Jotunn.Logger.LogError("Humanoid component missing!");
                yield break;
            }

            // VisEquipment remembers what armor the skeleton is wearing.
            // Exploit this to reapply the armor so the armor values work
            // again.
            var equipmentHashes = new List<int>()
            {
                humanoid.m_visEquipment.m_currentChestItemHash,
                humanoid.m_visEquipment.m_currentLegItemHash,
                humanoid.m_visEquipment.m_currentHelmetItemHash,
            };
            equipmentHashes.ForEach(hash =>
            {
                var equipmentPrefab = ZNetScene.instance.GetPrefab(hash);
                if (equipmentPrefab != null)
                {
                    humanoid.GiveDefaultItem(equipmentPrefab);
                }
            });

            // todo: implement custom emblems like for skeletons
            // var shoulderHash = humanoid.m_visEquipment.m_currentShoulderItemHash;
            // var shoulderPrefab = ZNetScene.instance.GetPrefab(shoulderHash);
            // if (shoulderPrefab != null
            //     && shoulderPrefab.TryGetComponent(out ItemDrop itemDrop)
            //     && itemDrop.name.Equals("CapeLox"))
            // {
            //     var emblem = Emblem;
            //     if (Emblem.Contains(emblem))
            //     {
            //         var material = NecromancerCape.Emblems[Emblem];
            //         humanoid.m_visEquipment.m_shoulderItemInstances.ForEach(g => 
            //             g.GetComponentsInChildren<SkinnedMeshRenderer>().ToList().ForEach(m =>
            //             {
            //                 var mats = m.materials;
            //                 for (int i = 0; i < mats.Length; i++)
            //                 {
            //                     mats[i] = material;
            //                 }
            //                 m.materials = mats;
            //             })
            //         );   
            //     }
            // }

            RestoreDrops();
        }
    }
}