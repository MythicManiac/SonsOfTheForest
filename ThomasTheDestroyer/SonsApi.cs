using Sons.Save;

namespace SonsApi;

using UnityEngine.SceneManagement;
using Sons.Gameplay.GameSetup;

public static class GameStateApi
{
    public enum HostMode
    {
        SinglePlayer,
        Multiplayer,
        MultiplayerClient,
    }

    public static bool IsInGame()
    {
        return SceneManager.GetSceneByName("SonsMain").IsValid() &&
               !SceneManager.GetSceneByName("SonsMainLoading").IsValid();
    }

    public static HostMode? GetHostMode()
    {
        var saveType = GameSetupManager.GetSaveGameType();
        switch (saveType)
        {
            case SaveGameType.SinglePlayer:
                return HostMode.SinglePlayer;
            case SaveGameType.Multiplayer:
                return HostMode.Multiplayer;
            case SaveGameType.MultiplayerClient:
                return HostMode.MultiplayerClient;
        }

        return null;
    }
}
