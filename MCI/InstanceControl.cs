using InnerNet;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace MCI
{
    public static class InstanceControl
    {
        public static readonly Dictionary<int, ClientData> clients = new();
        public static readonly Dictionary<byte, int> PlayerIdClientId = new();
        public static bool Any<T>(this Il2CppSystem.Collections.Generic.List<T> list, Func<T, bool> predicate) => list.ToSystem().Any(predicate);


        public static int AvailableId()
        {
            for (var i = 1; i < 128; i++)
            {
                if (!AmongUsClient.Instance.allClients.Any(x => x.Id == i) && !clients.ContainsKey(i) && PlayerControl.LocalPlayer.OwnerId != i)
                    return i;
            }

            return -1;
        }

        public static PlayerControl CurrentPlayerInPower { get; set; }

        public static void SwitchTo(byte playerId)
        {
            var newPlayer = Utils.PlayerById(playerId);

            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(PlayerControl.LocalPlayer.transform.position);
            PlayerControl.LocalPlayer.moveable = false;

            var light = PlayerControl.LocalPlayer.lightSource;
            var savedPlayer = PlayerControl.LocalPlayer;

            var pos = PlayerControl.LocalPlayer.transform.position;
            var pos2 = newPlayer.transform.position;


            PlayerControl.LocalPlayer = newPlayer;
            newPlayer.lightSource = light;
            newPlayer.moveable = true;

            AmongUsClient.Instance.ClientId = PlayerControl.LocalPlayer.OwnerId;
            AmongUsClient.Instance.HostId = PlayerControl.LocalPlayer.OwnerId;

            HudManager.Instance.SetHudActive(true);
            HudManager.Instance.KillButton.buttonLabelText.gameObject.SetActive(false);

            //hacky "fix" for twix and det

            HudManager.Instance.KillButton.transform.parent.GetComponentsInChildren<Transform>().ToList().ForEach((x) => { if (x.gameObject.name == "KillButton(Clone)") UnityEngine.Object.Destroy(x.gameObject); });
            HudManager.Instance.KillButton.transform.GetComponentsInChildren<Transform>().ToList().ForEach((x) => { if (x.gameObject.name == "KillTimer_TMP(Clone)") UnityEngine.Object.Destroy(x.gameObject); });
            HudManager.Instance.transform.GetComponentsInChildren<Transform>().ToList().ForEach((x) => { if (x.gameObject.name == "KillButton(Clone)") UnityEngine.Object.Destroy(x.gameObject); });

            light.transform.SetParent(PlayerControl.LocalPlayer.transform, false);
            light.transform.localPosition = newPlayer.Collider.offset;
            
            Camera.main.GetComponent<FollowerCamera>().SetTarget(PlayerControl.LocalPlayer);
            PlayerControl.LocalPlayer.MyPhysics.ResetMoveState(true);
            KillAnimation.SetMovement(PlayerControl.LocalPlayer, true);
            PlayerControl.LocalPlayer.MyPhysics.inputHandler.enabled = true;
            CurrentPlayerInPower = newPlayer;

            newPlayer.NetTransform.RpcSnapTo(pos2);
            savedPlayer.NetTransform.RpcSnapTo(pos);
        }
    }
}
