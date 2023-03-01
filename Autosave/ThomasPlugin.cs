using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.UnityEngine;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Il2CppSystem.Reflection;
using Il2CppSystem.Runtime.InteropServices;
using UnityEngine;
using Sons.Save;
using SonsApi;
using GCHandleType = Il2CppSystem.Runtime.InteropServices.GCHandleType;
using Object = UnityEngine.Object;

namespace Autosave
{
    [BepInPlugin("dev.mythic.sotf.thomas", "Thomas", PluginInfo.PLUGIN_VERSION)]
    public class ThomasPlugin : BasePlugin
    {
        public static ThomasPlugin Instance;

        public override void Load()
        {
            Instance = this;
            Instance.Log.LogInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var manager = AddComponent<ThomasManager>();
        }
    }

    public class ThomasManager : MonoBehaviour
    {
        public Il2CppReferenceField<Il2CppSystem.Collections.Generic.List<Il2CppSystem.Object>> loadedObjects;
        private GCHandle loadedObjectsHandle;

        private Il2CppReferenceField<GameObject> thomasPrefab;
        private GCHandle thomasPrefabHandle;
        private bool lastInputState = false;

        private void Awake()
        {
            var objs = new Il2CppSystem.Collections.Generic.List<Il2CppSystem.Object>();
            loadedObjectsHandle = GCHandle.Alloc(loadedObjects, GCHandleType.Normal);
            loadedObjects.Set(objs);

            ThomasPlugin.Instance.Log.LogInfo($"Loading assets...");
            ThomasPlugin.Instance.Log.LogInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var pluginDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var bundle = AssetBundle.LoadFromFile(Path.Join(pluginDir, "StreamingAssets", "mythic", "thomas"));
            thomasPrefab.Set(bundle.LoadAsset("Assets/ThomasTheTankEngine/ThomasTheDestroyer.prefab", Il2CppType.Of<GameObject>()).TryCast<GameObject>());
            thomasPrefabHandle = GCHandle.Alloc(thomasPrefab, GCHandleType.Normal);
            loadedObjects.Value.Add(thomasPrefab);
            ThomasPlugin.Instance.Log.LogInfo($"Finished loading asset: {thomasPrefab.ToString()}");
        }

        private void OnDestroy()
        {
            thomasPrefabHandle.Free();
            loadedObjectsHandle.Free();
        }

        void Update()
        {
            var inputState = Input.GetKeyInt(BepInEx.Unity.IL2CPP.UnityEngine.KeyCode.F8);
            var currentLastState = lastInputState;
            lastInputState = inputState;

            if (inputState != currentLastState)
            {
                ThomasPlugin.Instance.Log.LogInfo($"Input state flipped, now: {inputState}");
            }

            if (inputState && !currentLastState)
            {
                ThomasPlugin.Instance.Log.LogInfo($"Instantiating thomas: {thomasPrefab.ToString()}");
                ThomasPlugin.Instance.Log.LogInfo($"Instantiating thomas: {thomasPrefab.Value.ToString()}");
                ThomasPlugin.Instance.Log.LogInfo($"Instantiating thomas: {thomasPrefabHandle.BoxIl2CppObject()}");
                Instantiate(thomasPrefab);
            }
        }
    }
}
