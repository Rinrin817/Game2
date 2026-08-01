using Fusion;
using UnityEngine;

public class NetworkPlayerInfo : NetworkBehaviour
{
    // 全員に同期するネットワーク変数
    [Networked] public NetworkString<_16> PlayerName { get; set; }
    [Networked] public int SkinType { get; set; } // スキンの種類番号（あれば）
    [Networked] public int itemID { get; set; }

    // このオブジェクトの所有者のPlayerRefを保持
    [Networked] public PlayerRef OwnerRef { get; set; }
    public bool IsLeaving { get; private set; } = false;

    public override void Spawned()
    {
        // 自分が生成したオブジェクトなら、自分の名前を入力して同期させる
        if (HasInputAuthority)
        {
            PlayerName = PlayerData.PlayerName;
            // SkinType = 選択したスキンのID;
            OwnerRef = Runner.LocalPlayer;
            StartFusion startFusion = FindFirstObjectByType<StartFusion>();
            if (startFusion != null)
            {
                itemID = startFusion.nowItemID;
            }
            StartFusion sf = FindFirstObjectByType<StartFusion>();
            if (sf != null)
            {
                itemID = sf.nowItemID;
            }
        }

        // 生成されたら、ロビー画面のUIに「更新して！」と通知を送る
        LobbyUIManager ui = FindFirstObjectByType<LobbyUIManager>();
        if (ui != null) ui.UpdateLobbyUI();

        DontDestroyOnLoad(gameObject);
    }

    public override void Despawned(NetworkRunner runner, bool hasStateAuthority)
    {
        IsLeaving = true;
        // 退室時にもUIを更新する
        LobbyUIManager ui = FindFirstObjectByType<LobbyUIManager>();
        if (ui != null) ui.UpdateLobbyUI();
    }
}