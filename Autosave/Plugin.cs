using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using UnityEngine;
using Sons.Save;
using SonsApi;

namespace Autosave
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class AutosavePlugin : BasePlugin
    {
        public static AutosavePlugin Instance;

        private ConfigEntry<uint> _autosaveIntervalSeconds;

        public override void Load()
        {
            Instance = this;
            _autosaveIntervalSeconds = Config.Bind<uint>(
                "General",
                "AutosaveIntervalSeconds",
                300,
                "The amount of seconds to wait between each autosave."
            );
            var scheduler = AddComponent<AutosaveScheduler>();
            scheduler.StartScheduler(_autosaveIntervalSeconds.Value);
        }
    }

    public class AutosaveScheduler : MonoBehaviour
    {
        public void StartScheduler(uint intervalSeconds)
        {
            StartCoroutine(AutosaveSchedule(intervalSeconds).WrapToIl2Cpp());
        }

        IEnumerator AutosaveSchedule(uint intervalSeconds)
        {
            AutosavePlugin.Instance.Log.LogInfo($"Starting autosave schedule, running every {intervalSeconds} seconds");
            while (true)
            {
                yield return new WaitForSeconds(intervalSeconds);

                try
                {
                    if (!GameStateApi.IsInGame())
                    {
                        AutosavePlugin.Instance.Log.LogInfo("Not in game, skipping autosave");
                        continue;
                    }

                    SaveGameType? saveType = null;
                    switch (GameStateApi.GetHostMode())
                    {
                        case GameStateApi.HostMode.SinglePlayer:
                            saveType = SaveGameType.SinglePlayer;
                            break;
                        case GameStateApi.HostMode.Multiplayer:
                            saveType = SaveGameType.Multiplayer;
                            break;
                        case GameStateApi.HostMode.MultiplayerClient:
                            saveType = SaveGameType.MultiplayerClient;
                            break;
                    }

                    if (!saveType.HasValue)
                    {
                        AutosavePlugin.Instance.Log.LogInfo("Unable to determine host mode, skipping autosave");
                        continue;
                    }

                    AutosavePlugin.Instance.Log.LogInfo("Performing autosave");
                    SaveGameManager.Save(saveType.Value, (int) SaveGameManager.GetNewRandomIndex(saveType.Value));
                }
                catch (Exception e)
                {
                    AutosavePlugin.Instance.Log.LogError(e.Message);
                }
            }
        }
    }
}
