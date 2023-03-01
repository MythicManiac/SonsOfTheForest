using FMOD;
using FMOD.Studio;
using Il2CppSystem;
using UnityEngine;

namespace ThomasTheDestroyer;

public class FMODManager
{
    private static void LoadBank(TextAsset asset)
    {
        FMOD_StudioSystem system;
        var instanceResult = FMOD_StudioSystem.TryGetInstance(out system);
        ThomasPlugin.Instance.Log.LogDebug($"FMOD system loading success: {instanceResult}");
        var result = system.System.loadBankMemory(asset.bytes, FMOD.Studio.LOAD_BANK_FLAGS.UNENCRYPTED, out var loadedBank);
        ThomasPlugin.Instance.Log.LogDebug($"FMOD bank loading result: {result}");
        ThomasPlugin.Instance.Log.LogDebug($"FMOD bank loading success: {(result == RESULT.OK)}");
    }

    public static void LoadBankFile(string filepath)
    {
        var instanceResult = FMOD_StudioSystem.TryGetInstance(out var system);
        ThomasPlugin.Instance.Log.LogDebug($"FMOD system loading success: {instanceResult}");
        var result = system.System.loadBankFile(filepath, FMOD.Studio.LOAD_BANK_FLAGS.UNENCRYPTED, out var loadedBank);
        ThomasPlugin.Instance.Log.LogDebug($"FMOD bank loading result: {result}");
        ThomasPlugin.Instance.Log.LogDebug($"FMOD bank loading success: {(result == RESULT.OK)}");
        var sampleRes = loadedBank.loadSampleData();
        ThomasPlugin.Instance.Log.LogDebug($"FMOD samples loading success: {sampleRes}");
        var evtResult = system.System.getEvent("event:/ThomasBgm", out var evtDesc);
        ThomasPlugin.Instance.Log.LogDebug($"Event fetch result: {evtResult.ToString()}");
        ThomasPlugin.Instance.Log.LogDebug($"Event is valid: {evtDesc.isValid()}");
    }

    public static void PlayMp3(string filepath)
    {
        var instanceResult = FMOD_StudioSystem.TryGetInstance(out var system);
        ThomasPlugin.Instance.Log.LogDebug($"FMOD system loading: {instanceResult}");
        var coreResult = system.System.getCoreSystem(out var coreSystem);
        ThomasPlugin.Instance.Log.LogDebug($"FMOD core system loading: {coreResult}");
        var createResult = coreSystem.createSound(filepath, MODE.DEFAULT, out var sound);
        ThomasPlugin.Instance.Log.LogDebug($"FMOD sound loading: {createResult}");
        var channelGroupResult = coreSystem.getMasterChannelGroup(out var channelGroup);
        ThomasPlugin.Instance.Log.LogDebug($"FMOD channel group discovery result: {channelGroupResult}");
        var playResult = coreSystem.playSound(sound, channelGroup, false, out _);
        ThomasPlugin.Instance.Log.LogDebug($"FMOD play result: {playResult}");
    }

    public static void DebugBuses()
    {
        var instanceResult = FMOD_StudioSystem.TryGetInstance(out var system);
        ThomasPlugin.Instance.Log.LogDebug($"FMOD system loading: {instanceResult}");
        var busRes = system.System.getBus("bus:/", out var bus);
        ThomasPlugin.Instance.Log.LogDebug($"FMOD system bus fetch: {busRes}");
        var guidRes = bus.getID(out var busId);
        ThomasPlugin.Instance.Log.LogDebug($"FMOD system guid fetch: {guidRes}");
        ThomasPlugin.Instance.Log.LogDebug($"Master bus GUID: {busId.ToString()}");
        var evtResult = system.System.getEvent("event:/ThomasBgm", out var evtDesc);
        ThomasPlugin.Instance.Log.LogDebug($"Event fetch result: {evtResult.ToString()}");
        ThomasPlugin.Instance.Log.LogDebug($"Event is valid: {evtDesc.isValid()}");
    }
}
