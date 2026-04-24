using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject cameraObj;
    [SerializeField] Material[] materials;
    [SerializeField] GameObject ManagerObj;
    [SerializeField] GameObject[] prisonObj;
    [SerializeField] Animator animator;
    [SerializeField] Collider myCollider;
    [SerializeField] PhysicMaterial defaultFriction;
    [SerializeField] PhysicMaterial noFriction;
    [SerializeField] Transform prisonTransform;
    public float speed;
    public float jumpForce;
    public int roleNumber;
    public int stateNumber;
    public bool canMove;
    CameraController cameraController;
    EnemyPlayerController enemyPlayerController;
    VariableManager variableManager;
    GameObject collisionObj;
    int jumpCount;
    int jumpCountLimit;
    public bool isStand;
    int missionSub;
    float x;
    float z;

    void Start()
    {
        GetComponent<Renderer>().material = materials[roleNumber];
        if(roleNumber == 2)
        {
            speed = 20f;
            rb.useGravity = false;
            GetComponent<BoxCollider>().enabled = false;
        }
        else
        {
            speed = 7f;
        }
        if(roleNumber == 1)
        {
            for(int i = 0; i < 6; i ++)
            {
                foreach (var objCol in prisonObj[i].GetComponents<Collider>())
                {
                    foreach (var playerCol in GetComponents<Collider>())
                    {
                        Physics.IgnoreCollision(objCol, playerCol);
                    }
                }   
            }
            speed = 7.5f;
        }
        jumpForce = 10f;
        jumpCount = 0;
        jumpCountLimit = 1;
        stateNumber = 0;
        missionSub = -1;
        canMove = true;
        variableManager = ManagerObj.GetComponent<VariableManager>();
    }

    void Update()
    {
        if(variableManager.prisonBreak) stateNumber = 0;
        cameraController = cameraObj.GetComponent<CameraController>();
        Vector3 direction = transform.forward * z + transform.right * x;
        rb.velocity = new Vector3(direction.x * speed, rb.velocity.y, direction.z * speed);
        if(canMove)
        {
            x = Input.GetAxis("Horizontal"); // A/D, ←/→
            z = Input.GetAxis("Vertical");   // W/S, ↑/↓   
        }
        else
        {
            x = 0;
            z = 0;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (roleNumber != 2 && isStand)
            {
                if (jumpCount < jumpCountLimit)
                {
                    myCollider.sharedMaterial = noFriction;

                    Vector3 vel = rb.velocity;
                    vel.y = 0;
                    rb.velocity = vel;

                    RaycastHit hit;
                    Vector3 pushDirection = Vector3.zero;

                    Vector3 rayOrigin = transform.position + Vector3.down * 0.5f;

                    if (Physics.Raycast(rayOrigin, transform.forward, out hit, 0.7f))
                    {
                        if (hit.collider.CompareTag("Stage"))
                        {
                            // ★ hit.normal は「壁が向いている方向」
                            pushDirection = hit.normal;
                        }
                    }

                    if (pushDirection != Vector3.zero)
                    {
                        rb.AddForce(pushDirection * 20f, ForceMode.Impulse);
                    }

                    rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

                    jumpCount++;
                    isStand = false;
                }
            }
        }
        if(Input.GetKey(KeyCode.Space))
        {
            if(roleNumber == 2)
            {
                transform.position += Vector3.up * 0.8f;
            }
        }
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            if(roleNumber == 0)
            {
                if(animator.GetBool("isStandAnimation"))
                {
                    animator.SetBool("isStandAnimation", false);
                    speed = 3f;
                }
                else
                {
                    animator.SetBool("isStandAnimation", true);
                    speed = 7f;
                }
            }
        }
        if(Input.GetKey(KeyCode.LeftShift))
        {
            if(roleNumber == 2)
            {
                transform.position += Vector3.down * 0.8f;
            }
        }
        if(Input.GetMouseButton(1))
        {
            if(roleNumber == 2 && variableManager.canMission)
            {
                if(Input.GetKey(KeyCode.Alpha1))
                {
                    missionSub = 0;
                }
            }
        }
        if(missionSub != -1 && variableManager.missionSubNumber == -1 && roleNumber == 2)
        {
            if(missionSub == 0)
            {
                variableManager.textString = "Push any ArrowKey to set DARUMA";
                if(Input.GetKey(KeyCode.RightArrow))
                {
                    variableManager.missionNumber = 0;
                    variableManager.missionSubNumber = 0;
                    variableManager.canMission = false;
                    variableManager.missionStart = true;
                    variableManager.textString = " ";
                    missionSub = -1;
                }
                if(Input.GetKey(KeyCode.LeftArrow))
                {
                    variableManager.missionNumber = 0;
                    variableManager.missionSubNumber = 1;
                    variableManager.canMission = false;
                    variableManager.missionStart = true;
                    variableManager.textString = " ";
                    missionSub = -1;
                }
                if(Input.GetKey(KeyCode.UpArrow))
                {
                    variableManager.missionNumber = 0;
                    variableManager.missionSubNumber = 2;
                    variableManager.canMission = false;
                    variableManager.missionStart = true;
                    variableManager.textString = " ";
                    missionSub = -1;
                }
                if(Input.GetKey(KeyCode.DownArrow))
                {
                    variableManager.missionNumber = 0;
                    variableManager.missionSubNumber = 3;
                    variableManager.canMission = false;
                    variableManager.missionStart = true;
                    variableManager.textString = " ";
                    missionSub = -1;
                }
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.CompareTag("Stage"))
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                // 上向きの法線（地面がプレイヤーを押し上げている状態）かチェック
                // 0.5f くらいにすると、ある程度の坂道も地面として認められます
                if (contact.normal.y > 0.5f)
                {
                    isStand = true;
                    jumpCount = 0;
                    myCollider.sharedMaterial = defaultFriction;
                    return; // 地面が見つかったので終了
                }
            }
        }
        if(collision.gameObject.CompareTag("Player"))
        {
            collisionObj = collision.gameObject;
            enemyPlayerController = collisionObj.GetComponent<EnemyPlayerController>();
            if(roleNumber == 0 && enemyPlayerController.roleNumber == 1)
            {
                stateNumber = 1;
                transform.position = prisonTransform.transform.position;
                variableManager.prisonBreak = false;
            }
        }
        if(collision.gameObject.CompareTag("Prison"))
        {
            if(roleNumber == 0 && stateNumber == 0)
            {
                variableManager.prisonBreak = true;
            }
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.tag == "Gem")
        {
            variableManager.gemList[collider.gameObject.GetComponent<GemSprict>().gemNumber] = 1;
            if(!variableManager.gemList.Contains(0))
            {
                variableManager.isGoalOpen = true;
            }
            collider.gameObject.GetComponent<GemSprict>().effect = true;
        }
        if(collider.gameObject.tag == "daruma")
        {
            variableManager.clearMission = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag == "Stage")
        {
            isStand = false;
            myCollider.sharedMaterial = noFriction;
        }
    }
}
