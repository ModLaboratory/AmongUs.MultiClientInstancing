using BepInEx.Unity.IL2CPP.Utils;
using InnerNet;
using System.Collections; 
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MCI
{
    public static class Utils
    {
        public static void CleanUpLoad()
        {
            if (GameData.Instance.AllPlayers.Count == 1)
            {
                InstanceControl.clients.Clear();
                InstanceControl.PlayerIdClientId.Clear();
            }
        }

        public static void CreatePlayerInstances(int count) => AmongUsClient.Instance.StartCoroutine(CoCreatePlayerInstances(count));

        public static IEnumerator CoCreatePlayerInstances(int count)
        {
            for (var i = 0; i < count; i++)
                yield return CoCreatePlayerInstance();
        }

        public static void CreatePlayerInstance() => AmongUsClient.Instance.StartCoroutine(CoCreatePlayerInstance());

        public static IEnumerator CoCreatePlayerInstance()
        {
            var sampleId = InstanceControl.AvailableId();
            var sampleC = new ClientData(sampleId, $"Bot-{sampleId}", new()
            {
                Platform = Platforms.StandaloneWin10,
                PlatformName = "Bot"
            }, 1, "", "robotmodeactivate");

            AmongUsClient.Instance.GetOrCreateClient(sampleC);
            yield return AmongUsClient.Instance.CreatePlayer(sampleC);

            sampleC.Character.SetName($"Bot {sampleC.Character.PlayerId}");
            sampleC.Character.SetSkin(HatManager.Instance.allSkins[Random.Range(0, HatManager.Instance.allSkins.Count)].ProdId, 0);
            sampleC.Character.SetColor(Random.Range(0, Palette.PlayerColors.Length));
            sampleC.Character.SetHat("hat_NoHat", 0);
            sampleC.Character.SetVisor("visor_EmptyVisor", 0);
            sampleC.Character.SetNamePlate(HatManager.Instance.allNamePlates[Random.Range(0, HatManager.Instance.allNamePlates.Count)].ProdId);
            sampleC.Character.SetPet(HatManager.Instance.allPets[Random.Range(0, HatManager.Instance.allPets.Count)].ProdId);

            if (!InstanceControl.clients.ContainsKey(sampleId))
            {
                InstanceControl.clients.Add(sampleId, sampleC);
            }

            InstanceControl.PlayerIdClientId.Add(sampleC.Character.PlayerId, sampleId);

            sampleC.Character.MyPhysics.ResetAnimState();
            sampleC.Character.MyPhysics.ResetMoveState();

            if (SubmergedCompatibility.Loaded)
            {
                SubmergedCompatibility.ImpartSub(sampleC.Character);
            }

            yield return sampleC;
        }



        public static void UpdateNames(string name)
        {
            foreach (byte playerId in InstanceControl.PlayerIdClientId.Keys)
            {
                PlayerById(playerId).SetName(name + $" {playerId}");
                if (MCIPlugin.IKnowWhatImDoing) PlayerById(playerId).SetName(name + $" {{{PlayerById(playerId).PlayerId}:{InstanceControl.PlayerIdClientId[playerId]}}}");
            }
        }

        public static List<T> ToSystem<T>(this Il2CppSystem.Collections.Generic.List<T> list) => new(list.ToArray());
        public static List<PlayerControl> AllPlayers() => PlayerControl.AllPlayerControls.ToSystem();
        public static PlayerControl PlayerById(byte id) => AllPlayers().Find(x => x.PlayerId == id);



        public static void RemovePlayer(byte id)
        {
            int clientId = InstanceControl.clients.FirstOrDefault(x => x.Value.Character.PlayerId == id).Key;
            InstanceControl.clients.Remove(clientId, out ClientData outputData);
            InstanceControl.PlayerIdClientId.Remove(id);
            AmongUsClient.Instance.RemovePlayer(clientId, DisconnectReasons.ExitGame);
            AmongUsClient.Instance.allClients.Remove(outputData);
        }

        /*
        public static void RemoveAllPlayers()
        {
            InstanceControl.PlayerIdClientId.Keys.ForEach(RemovePlayer);
            InstanceControl.SwitchTo(0);
        }
        */

        public static void RemoveAllPlayers()
        {
            foreach (var playerId in InstanceControl.PlayerIdClientId.Keys.ToList())
            {
                RemovePlayer(playerId);
            }
            InstanceControl.SwitchTo(0);
        }
    }
}
