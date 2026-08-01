using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;
using System.Collections.Generic;
using System.ComponentModel;

public class LobbyUIManager : MonoBehaviour
{
    [System.Serializable]
    public struct PlayerCardUI
    {
        public GameObject cardRoot;
        public TextMeshProUGUI nameText;
        public GameObject loadingObject;
    }
    [System.Serializable]
    public class PlayerCardGroup
    {
        public List<PlayerCardUI> cards = new List<PlayerCardUI>();
    }
    [SerializeField] Canvas[] canvasArray;
    [SerializeField] PlayerCardGroup[] playerCardGroups;
    [SerializeField] GameObject[] skinPrefab;
    [SerializeField] TextMeshProUGUI passwordOutput;
    GameObject NetworkObj;
    int modeNumber2;
    int playerCount;
    float size;
    Vector3[] positions;

    // 生成したキャラクターの情報を記録しておくリスト
    private List<GameObject> spawnedModels = new List<GameObject>();

    void Start()
    {
        NetworkRunner runner = FindFirstObjectByType<NetworkRunner>();
        NetworkObj = FindFirstObjectByType<NetworkRunner>().gameObject;
        modeNumber2 = NetworkObj.GetComponent<StartFusion>().modeNumber;
        playerCount = NetworkObj.GetComponent<StartFusion>().modePlayerCount;
        UpdateLobbyUI();
        passwordOutput.text = runner.SessionInfo.Name;
        DynamicGI.UpdateEnvironment();
    }

    public void OnPlayerJoined()
    {
        
    }

    public void UpdateLobbyUI()
    {
        canvasArray[modeNumber2].gameObject.SetActive(true);

        NetworkPlayerInfo[] rawPlayers = FindObjectsByType<NetworkPlayerInfo>(FindObjectsSortMode.None);
        List<NetworkPlayerInfo> allPlayers = new List<NetworkPlayerInfo>();

        foreach (var p in rawPlayers)
        {   
            if (p != null && p.Object != null && p.Object.IsValid && !p.IsLeaving)
            {
                allPlayers.Add(p);
            }
        }

        // if (roomCountText != null) roomCountText.text = allPlayers.Count + "/" + playerCount.ToString();

        while (spawnedModels.Count < allPlayers.Count)
        {
            var startFusion = NetworkObj.GetComponent<StartFusion>();
            var state = startFusion != null ? startFusion.State : null;

            int skinIndex = (state != null && state.Object != null && state.Object.IsValid)  ? startFusion.skinObjNumber : 0;

            GameObject skinObj;
            spawnedModels.Add(skinObj = Instantiate(skinPrefab[skinIndex], Vector3.zero, Quaternion.identity));
        }
        while (spawnedModels.Count > allPlayers.Count)
        {
            Destroy(spawnedModels[spawnedModels.Count - 1]);
            spawnedModels.RemoveAt(spawnedModels.Count - 1);
        }

        if(modeNumber2 == 0)
        {
            positions = new Vector3[]
            {
                new Vector3(0f, 0f, -3.5f),
            };
            size = 0.4f;
        }
        if(modeNumber2 == 1)
        {
            positions = new Vector3[]
            {
                new Vector3(-5f, 0f, -3.5f),
                new Vector3(-2.5f, 0f, -3.5f),
                new Vector3(0f, 0f, -3.5f),
                new Vector3(2.5f, 0f, -3.5f),
                new Vector3(5f, 0f, -3.5f)
            };
            size = 0.4f;
        }
        if(modeNumber2 == 2)
        {
            positions = new Vector3[]
            {
                new Vector3(-5f, -1f, -3.5f),
                new Vector3(-2.5f, -1f, -3.5f),
                new Vector3(0f, -1f, -3.5f),
                new Vector3(2.5f, -1f, -3.5f),
                new Vector3(5f, -1f, -3.5f),
                new Vector3(-3.75f, 2f, -3.5f),
                new Vector3(-1.25f, 2f, -3.5f),
                new Vector3(1.25f, 2f, -3.5f),
                new Vector3(3.75f, 2f, -3.5f)
            };   
            size = 0.25f;
        }
        if(modeNumber2 == 3)
        {
            positions = new Vector3[]
            {
                new Vector3(-5.5f, -1f, -3.5f),
                new Vector3(-3.3f, -1f, -3.5f),
                new Vector3(-1.1f, -1f, -3.5f),
                new Vector3(1.1f, -1f, -3.5f),
                new Vector3(3.3f, -1f, -3.5f),
                new Vector3(5.5f, -1f, -3.5f),
                new Vector3(-5.5f, 1f, -3.5f),
                new Vector3(-3.3f, 1f, -3.5f),
                new Vector3(-1.1f, 1f, -3.5f),
                new Vector3(1.1f, 1f, -3.5f),
                new Vector3(3.3f, 1f, -3.5f),
                new Vector3(5.5f, 1f, -3.5f),
                new Vector3(4.4f, 3f, -3.5f)
            };   
            size = 0.2f;
        }
        if(modeNumber2 == 4)
        {
            positions = new Vector3[]
            {
                new Vector3(-2.5f, 0f, -3.5f),
                new Vector3(2.5f, 0f, -3.5f)
            };   
            size = 0.4f;
        }

        List<PlayerCardUI> playerCards = playerCardGroups[modeNumber2].cards;

        for (int i = 0; i < playerCards.Count; i++)
        {
            if (i < allPlayers.Count)
            {
                if (allPlayers[i].HasInputAuthority)
                {
                    playerCards[i].nameText.text = "あなた (" + allPlayers[i].PlayerName.ToString() + ")";
                }
                else
                {
                    playerCards[i].nameText.text = allPlayers[i].PlayerName.ToString();
                }
                playerCards[i].loadingObject.SetActive(false);

                // 位置とスケールの設定
                spawnedModels[i].transform.position = positions[i];
                Vector3 sizeVector3 = new Vector3(size, size, size);
                spawnedModels[i].transform.localScale = sizeVector3;
                
                // アイテムの同期処理（各プレイヤーのNetworkPlayerInfoからItemIDを取得して適用）
                int currentItemID = allPlayers[i].itemID;
                foreach (Transform child in spawnedModels[i].transform)
                {
                    if (child.gameObject.name == "Items")
                    {
                        foreach (Transform child2 in child.transform)
                        {
                            child2.gameObject.SetActive(child2.gameObject.name == "Item" + currentItemID.ToString());
                        }
                    }
                }
            }
            else
            {
                playerCards[i].nameText.text = "参加待ち";
                playerCards[i].loadingObject.SetActive(true);
            }
        }
    }
}