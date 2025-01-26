using HarmonyLib;
using UnityEngine;

namespace MCI.Patches
{
    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public sealed class Keyboard_Joystick
    {
        private static int controllingFigure;

        public static void Postfix()
        {
            if (!MCIPlugin.Enabled) return;
            if (Input.GetKeyDown(KeyCode.F5))
            {
                controllingFigure = PlayerControl.LocalPlayer.PlayerId;
                if (PlayerControl.AllPlayerControls.Count == 15 && !Input.GetKeyDown(KeyCode.F6)) return; //press f6 and f5 to bypass limit
                Utils.CleanUpLoad();
                Utils.CreatePlayerInstance();
            }

            if (Input.GetKeyDown(KeyCode.F9) || Input.GetKeyDown(KeyCode.F10))
            {
                int total = PlayerControl.AllPlayerControls.Count;

                if (total > 0)
                {
                    controllingFigure = (controllingFigure + 1) % total;
                    InstanceControl.SwitchTo((byte)controllingFigure);
                }
            }

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F6))
            {
                MCIPlugin.IKnowWhatImDoing = !MCIPlugin.IKnowWhatImDoing;
                Utils.UpdateNames(MCIPlugin.RobotName);
            }
            else if (Input.GetKeyDown(KeyCode.F6))
            {
                MCIPlugin.Persistence = !MCIPlugin.Persistence;
            }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                Utils.RemoveAllPlayers();
            }
        }
    }
}
