using TMPro;
using UnityEngine;
using System.Linq;

public class PlayersNumberText : MonoBehaviour
{
    string number;
    TextMeshPro textMesh;
    GameObject cameraObj;

    void Start()
    {
        number = transform.parent.gameObject.name.Replace("EnemyPlayer", "");
        textMesh = GetComponent<TextMeshPro>();
        textMesh.text = number;
        cameraObj = Camera.main.gameObject;
    }

    void Update()
    {
        // 1. 今シーンにいる「自分以外の」敵を数字順に並べて取得
        var sortedEnemies = GameObject.FindGameObjectsWithTag("Player")
            .OrderBy(obj => {
                string numStr = obj.name.Replace("EnemyPlayer", "");
                int num;
                return int.TryParse(numStr, out num) ? num : int.MaxValue;
            })
            .ToList(); // IndexOfを使うためにListにする

        // 2. このテキストの「親（敵本体）」がリストの何番目にいるか探す
        // 親オブジェクト自体もリストに含まれているはずなので、Indexを取得
        int myIndex = sortedEnemies.IndexOf(transform.parent.gameObject);

        // 3. テキストを更新 (0から始まるので +1 する)
        if (myIndex != -1)
        {
            textMesh.text = (myIndex + 1).ToString();
        }
        textMesh.transform.forward = cameraObj.transform.forward;
    }
}
