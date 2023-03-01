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
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Il2CppSystem.Reflection;
using Il2CppSystem.Runtime.InteropServices;
using Sons.Gameplay.TreeCutting;
using Sons.Physics;
using UnityEngine;
using Sons.Save;
using SonsApi;
using GCHandleType = Il2CppSystem.Runtime.InteropServices.GCHandleType;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace ThomasTheDestroyer
{
    [BepInPlugin("dev.mythic.sotf.thomas", "ThomasTheDestroyer", PluginInfo.PLUGIN_VERSION)]
    public class ThomasPlugin : BasePlugin
    {
        public static ThomasPlugin Instance;

        private ConfigEntry<uint> _spawnCheckInterval;
        private ConfigEntry<float> _spawnChance;

        public override void Load()
        {
            Instance = this;
            _spawnCheckInterval = Config.Bind<uint>(
                "General",
                "SpawnCheckInterval",
                1800,
                "The amount of seconds to wait between thomas spawn attempts. 30 minutes by default."
            );
            _spawnChance = Config.Bind<float>(
                "General",
                "SpawnChance",
                0.2f,
                "The chance for thomas to spawn when the interval is checked (0.2 = 20%)"
            );
            Instance.Log.LogInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            ClassInjector.RegisterTypeInIl2Cpp(typeof(ThomasController));
            var manager = AddComponent<ThomasManager>();
            manager.StartScheduler(_spawnCheckInterval.Value, _spawnChance.Value);
        }
    }

    public class ThomasManager : MonoBehaviour
    {
        private Il2CppReferenceField<GameObject> thomasPrefab;
        private Il2CppReferenceField<AssetBundle> bundle;
        private GCHandle thomasPrefabHandle;
        private GCHandle bundleHandle;
        private bool lastInputState = false;
        private bool _audioLoaded = false;

        private void Awake()
        {
            ThomasPlugin.Instance.Log.LogDebug($"Loading assets...");
            ThomasPlugin.Instance.Log.LogDebug(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var pluginDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            bundle.Set(AssetBundle.LoadFromFile(Path.Join(pluginDir, "StreamingAssets", "mythic", "thomas")));
            bundleHandle = GCHandle.Alloc(bundle, GCHandleType.Normal);
            thomasPrefab.Set(bundle.Value.LoadAsset("Assets/ThomasTheTankEngine/ThomasTheDestroyer.prefab", Il2CppType.Of<GameObject>()).TryCast<GameObject>());
            thomasPrefabHandle = GCHandle.Alloc(thomasPrefab, GCHandleType.Normal);
            ThomasPlugin.Instance.Log.LogDebug($"Finished loading asset: {thomasPrefab.ToString()}");
        }

        public void StartScheduler(uint intervalSeconds, float spawnChance)
        {
            StartCoroutine(ThomasSchedule(intervalSeconds, spawnChance).WrapToIl2Cpp());
        }

        private void LoadAudio()
        {
            if (!_audioLoaded)
            {
                var pluginDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                FMODManager.LoadBankFile(Path.Join(pluginDir, "StreamingAssets", "mythic", "Thomas.strings.bank"));
                FMODManager.LoadBankFile(Path.Join(pluginDir, "StreamingAssets", "mythic", "Thomas.bank"));
                _audioLoaded = true;
            }
        }

        private void OnDestroy()
        {
            thomasPrefabHandle.Free();
            bundleHandle.Free();
        }

        private void SpawnThomas()
        {
            if (!GameStateApi.IsInGame() || GameStateApi.GetHostMode() != GameStateApi.HostMode.SinglePlayer)
                return;

            LoadAudio();
            ThomasPlugin.Instance.Log.LogInfo($"Instantiating thomas: {thomasPrefab.Value.ToString()}");
            var camera = GameObject.FindWithTag("MainCamera");
            if (camera != null)
            {
                var obj = Instantiate<GameObject>(
                    thomasPrefab,
                    camera.transform.position + transform.forward * 50,
                    camera.transform.rotation
                );
                var fmodEmitter = obj.AddComponent<FMOD_StudioEventEmitter>();
                fmodEmitter.SetEventPath("event:/ThomasBgm");
                fmodEmitter.Play();
                var comp = obj.AddComponent(Il2CppType.Of<ThomasController>()).TryCast<ThomasController>();
                comp.SetTarget(camera.transform);
            }
        }

        IEnumerator ThomasSchedule(uint intervalSeconds, float spawnChance)
        {
            ThomasPlugin.Instance.Log.LogInfo($"Starting thomas schedule, running every {intervalSeconds} seconds with spawn chance {spawnChance}");
            while (true)
            {
                yield return new WaitForSeconds(intervalSeconds);

                try
                {
                    if (!GameStateApi.IsInGame() || GameStateApi.GetHostMode() != GameStateApi.HostMode.SinglePlayer)
                    {
                        ThomasPlugin.Instance.Log.LogDebug("Not valid game state, skipping");
                        continue;
                    }

                    if (Random.value < spawnChance)
                    {
                        SpawnThomas();
                    }
                }
                catch (Exception e)
                {
                    ThomasPlugin.Instance.Log.LogError(e.Message);
                }
            }
        }

        // void Update()
        // {
        //     var inputState = Input.GetKeyInt(BepInEx.Unity.IL2CPP.UnityEngine.KeyCode.F8);
        //     var currentLastState = lastInputState;
        //     lastInputState = inputState;
        //
        //     if (inputState && !currentLastState)
        //     {
        //         SpawnThomas();
        //     }
        // }
    }

    public class ThomasController : MonoBehaviour
    {
        private Il2CppReferenceField<Transform> _target;
        public float speed = 4;
        private float endTime;
        private Il2CppReferenceField<FallingTreeDamage> _damage;
        private Il2CppReferenceField<MeleeImpactData> _impact;

        private void Awake()
        {
            endTime = Time.fixedTime + 75;
            _damage.Set(gameObject.AddComponent<FallingTreeDamage>());
            _damage.Value._damage = 1000;
            _damage.Value._minImpactForce = 100;
            _damage.Value._fallingTreeRoot = transform;
            _impact.Set(new MeleeImpactData(_damage.Value.TryCast<IImpactSender>(), ImpactMeleeType.Blunt, 1000, Vector3.forward, Vector3.forward));
        }

        public void SetTarget(Transform target)
        {
            _target.Set(target);
        }

        private void OnCollisionEnter(Collision collision)
        {
            var damageReceiver = collision.gameObject.GetComponent<FallingTreeDamageReceiver>();
            if (damageReceiver != null)
            {
                damageReceiver.OnImpact(_damage.Value.TryCast<IImpactSender>(), _impact.Value.TryCast<IImpactData>());
            }

            var vitals = collision.other.GetComponent<Vitals>();
            if (vitals != null)
            {
                vitals.ApplyDamage(100000);
                Destroy(gameObject);
            }

            var swapper = collision.gameObject.GetComponent<TreeSwapper>();
            if (swapper != null)
            {
                swapper.InstantCutForceFall(Vector3.fwd);
            }
            else
            {
                var manager = collision.gameObject.GetComponent<TreeCutManager>();
                if (manager != null)
                {
                    manager.InstantCut(true);
                }
            }
        }

        void FixedUpdate()
        {
            if (Time.fixedTime > endTime)
            {
                Destroy(gameObject);
            }
            else
            {
                var targetPos = _target.Value.position + (Vector3.down * 0.75f);
                if (_target.Value == null) return;
                var dir = targetPos - transform.position;
                transform.rotation = Quaternion.LookRotation(dir);
                transform.position =
                    Vector3.MoveTowards(transform.position, targetPos, speed * Time.fixedDeltaTime);
            }
        }
    }
}
