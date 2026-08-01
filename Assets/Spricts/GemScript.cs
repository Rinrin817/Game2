using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GemScript : MonoBehaviour
{
    [SerializeField] GameObject effectObj;
    List<GameObject> PlayerObj = new List<GameObject>();
    List<PlayerController> playerController = new List<PlayerController>();
    public int gemNumber;
    public bool effect;
    float timeLimit = 1f;
    float timeCount;
    // Start is called before the first frame update
    void Start()
    {
        timeCount = 0;
        effect = false;
    }

    public void AddPlayer(GameObject player)
    {
        if (!PlayerObj.Contains(player))
        {
            PlayerObj.Add(player);
            playerController.Add(player.GetComponent<PlayerController>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(effect)
        {
            effectObj.SetActive(true);
            GetComponent<MeshRenderer>().enabled = false;
            /*
            foreach (var objCol in GetComponents<Collider>())
            {
                foreach (var player in PlayerObj)
                {
                    if (player == null) continue;
                    foreach (var playerCol in player.GetComponents<Collider>())
                    {
                        Physics.IgnoreCollision(objCol, playerCol);
                    }
                }
            }
            */
            timeCount += Time.deltaTime;
            if(timeCount >= 1.5f)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
