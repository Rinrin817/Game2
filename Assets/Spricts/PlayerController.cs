using System.Collections;
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
    [SerializeField] GameObject[] blenderObj;
    [SerializeField] Collider myCollider;
    [SerializeField] PhysicMaterial defaultFriction;
    [SerializeField] PhysicMaterial noFriction;
    [SerializeField] Transform prisonTransform;
    [SerializeField] AudioClip goodAction;
    [SerializeField] AudioClip goodAction2;
    [SerializeField] AudioClip jump;
    [SerializeField] AudioClip bigJumpAudio;
    [SerializeField] AudioClip badAction;
    public float speed;
    public float jumpForce;
    public int roleNumber;
    public int stateNumber;
    public bool canMove;
    CameraController cameraController;
    EnemyPlayerController enemyPlayerController;
    VariableManager variableManager;
    GameObject collisionObj;
    AudioSource audioSource;
    Animator animator;
    int jumpCount;
    int jumpCountLimit;
    int cameraNumber;
    public bool isStand;
    public bool bigJump;
    bool runAnimationBool;
    int missionSub;
    float x;
    float z;

    void Start()
    {
        cameraNumber = -1;
        audioSource = GetComponent<AudioSource>();
        GetComponent<Renderer>().material = materials[roleNumber];
        if(roleNumber == 0)
        {
            gameObject.layer = 6;
            animator = Instantiate(blenderObj[0], gameObject.transform.position + new Vector3(0, 0.1f, 0), Quaternion.identity, gameObject.transform).GetComponent<Animator>();
        }
        if(roleNumber == 1)
        {
            gameObject.layer = 7;
            animator = Instantiate(blenderObj[1], gameObject.transform.position + new Vector3(0, 0.1f, 0), Quaternion.identity, gameObject.transform).GetComponent<Animator>();
        }
        if(roleNumber == 2)
        {
            gameObject.layer = 8;
        }
        if(roleNumber == 2)
        {
            speed = 18f;
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
        jumpForce = 12f;
        jumpCount = 0;
        jumpCountLimit = 1;
        stateNumber = 0;
        missionSub = -1;
        canMove = true;
        runAnimationBool = false;
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
            x = Input.GetAxis("Horizontal");
            z = Input.GetAxis("Vertical");

            // 停止しきい値（走り中だけ緩める）
            float stopThreshold = runAnimationBool ? 0.2f : 0.5f;

            // ★追加：このフレームで変更したか
            bool changed = false;

            if(animator != null && !animator.GetBool("isRunAnimation"))
            {
                if(runAnimationBool)
                {
                    if(x > 0.7f || z > 0.7f || x < -0.7f || z < -0.7f)
                    {
                        if(animator != null) animator.SetBool("isRunAnimation", true);
                        runAnimationBool = false;
                        changed = true;
                    }
                }
                else
                {
                    // ★修正：停止条件(0.5)と重ならないようにする
                    if(x > 0.5f || z > 0.5f || x < -0.5f || z < -0.5f)
                    {
                        if(animator != null) animator.SetBool("isRunAnimation", true);
                        runAnimationBool = true;
                        changed = true;
                    }
                }
            }

            // ★同一フレームでの再変更を防ぐ
            if(animator != null && animator.GetBool("isRunAnimation") && !changed)
            {
                if(runAnimationBool)
                {
                    if(x > 0.7f || z > 0.7f || x < -0.7f || z < -0.7f)
                    {
                        runAnimationBool = false;
                    }

                    if(x < stopThreshold && z < stopThreshold && x > -stopThreshold && z > -stopThreshold)
                    {
                        runAnimationBool = false;
                        if(animator != null) animator.SetBool("isRunAnimation", false);
                    }
                }
                else
                {
                    if(x < stopThreshold && z < stopThreshold && x > -stopThreshold && z > -stopThreshold)
                    {
                        if(animator != null) animator.SetBool("isRunAnimation", false);
                    }   
                }
            }
        }
        else
        {
            x = 0;
            z = 0;
            if(animator != null) animator.SetBool("isRunAnimation", false);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (roleNumber != 2 && isStand && canMove)
            {
                if (jumpCount < jumpCountLimit)
                {
                    audioSource.PlayOneShot(jump, 0.7f);
                    if(animator != null) animator.SetBool("isJumpAnimation", true);

                    myCollider.sharedMaterial = noFriction;

                    Vector3 vel = rb.velocity;
                    if (!bigJump) vel.y = 0;
                    rb.velocity = vel;

                    RaycastHit hit;
                    Vector3 pushDirection = Vector3.zero;
                    Vector3 rayOrigin = transform.position + Vector3.down * 0.5f;

                    if (Physics.Raycast(rayOrigin, transform.forward, out hit, 0.7f))
                    {
                        if (hit.collider.CompareTag("Stage"))
                        {
                            pushDirection = hit.normal;
                        }
                    }

                    if (pushDirection != Vector3.zero)
                    {
                        rb.AddForce(pushDirection * 25f, ForceMode.Impulse);
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
                transform.position += Vector3.up * 0.6f;
            }
        }
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            if(roleNumber == 0)
            {
                if(animator != null && animator.GetBool("isStandAnimation"))
                {
                    if(animator != null) animator.SetBool("isStandAnimation", false);
                    speed = 3f;
                }
                else
                {
                    if(animator != null) animator.SetBool("isStandAnimation", true);
                    speed = 7f;
                }
            }
        }
        if(Input.GetKey(KeyCode.LeftShift))
        {
            if(roleNumber == 2)
            {
                transform.position += Vector3.down * 0.6f;
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
        if(roleNumber == 2)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Player")
                .Where(obj => obj != gameObject)
                .OrderBy(obj => {
                    // 名前から "EnemyPlayer" を除外して数字部分だけにする
                    string numStr = obj.name.Replace("EnemyPlayer", "");
                    
                    // 数値に変換して、その値で並べ替える（変換できない場合は大きな値にするなどの安全策）
                    int num;
                    return int.TryParse(numStr, out num) ? num : int.MaxValue;
                })
                .ToArray();
            if(cameraNumber != -1 && cameraNumber < enemies.Length)
            {
                transform.position = enemies[cameraNumber].transform.position;

                if(Input.GetKey(KeyCode.UpArrow))
                {
                    transform.rotation = enemies[cameraNumber].transform.rotation;
                }

                if(Input.GetKey(KeyCode.DownArrow))
                {
                    transform.rotation = enemies[cameraNumber].transform.rotation * Quaternion.Euler(0, 180, 0);
                }
            }
            else
            {
                cameraNumber = -1;
            }
            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                if(Input.GetKey(KeyCode.LeftShift) && enemies.Length > 9)
                {
                    if(cameraNumber == 9)
                    {
                        cameraNumber = -1;
                    }
                    else
                    {
                        cameraNumber = 9;
                    }
                }
                else if(enemies.Length > 0)
                {
                    if(cameraNumber == 0)
                    {
                        cameraNumber = -1;
                    }
                    else
                    {
                        cameraNumber = 0;
                    }
                }
            }
            if(Input.GetKeyDown(KeyCode.Alpha2))
            {
                if(Input.GetKey(KeyCode.LeftShift) && enemies.Length > 10)
                {
                    if(cameraNumber == 10)
                    {
                        cameraNumber = -1;
                    }
                    else
                    {
                        cameraNumber = 10;
                    }
                }
                else if(enemies.Length > 1)
                {
                    if(cameraNumber == 1)
                    {
                        cameraNumber = -1;
                    }
                    else if(enemies.Length > 1)
                    {
                        cameraNumber = 1;
                    }
                }
            }
            if(Input.GetKeyDown(KeyCode.Alpha3))
            {
                if(Input.GetKey(KeyCode.LeftShift) && enemies.Length > 11)
                {
                    if(cameraNumber == 11)
                    {
                        cameraNumber = -1;
                    }
                    else
                    {
                        cameraNumber = 11;
                    }
                }
                else if(enemies.Length > 2)
                {
                    if(cameraNumber == 2)
                    {
                        cameraNumber = -1;
                    }
                    else if(enemies.Length > 2)
                    {
                        cameraNumber = 2;
                    }
                }
            }
            if(Input.GetKeyDown(KeyCode.Alpha4))
            {
                if(cameraNumber == 3)
                {
                    cameraNumber = -1;
                }
                else if(enemies.Length > 3)
                {
                    cameraNumber = 3;
                }
            }
            if(Input.GetKeyDown(KeyCode.Alpha5))
            {
                if(cameraNumber == 4)
                {
                    cameraNumber = -1;
                }
                else if(enemies.Length > 4)
                {
                    cameraNumber = 4;
                }
            }
            if(Input.GetKeyDown(KeyCode.Alpha6))
            {
                if(cameraNumber == 5)
                {
                    cameraNumber = -1;
                }
                else if(enemies.Length > 5)
                {
                    cameraNumber = 5;
                }
            }

            if(Input.GetKeyDown(KeyCode.Alpha7))
            {
                if(cameraNumber == 6)
                {
                    cameraNumber = -1;
                }
                else if(enemies.Length > 6)
                {
                    cameraNumber = 6;
                }
            }

            if(Input.GetKeyDown(KeyCode.Alpha8))
            {
                if(cameraNumber == 7)
                {
                    cameraNumber = -1;
                }
                else if(enemies.Length > 7)
                {
                    cameraNumber = 7;
                }
            }

            if(Input.GetKeyDown(KeyCode.Alpha9))
            {
                if(cameraNumber == 8)
                {
                    cameraNumber = -1;
                }
                else if(enemies.Length > 8)
                {
                    cameraNumber = 8;
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
                    bigJump = false;
                    jumpCount = 0;
                    myCollider.sharedMaterial = defaultFriction;
                    if(animator != null) animator.SetBool("isJumpAnimation", false);
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
                audioSource.PlayOneShot(badAction);
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
            audioSource.PlayOneShot(goodAction);
            variableManager.gemList[collider.gameObject.GetComponent<GemSprict>().gemNumber] = 1;
            if(!variableManager.gemList.Contains(0))
            {
                variableManager.isGoalOpen = true;
            }
            collider.gameObject.GetComponent<GemSprict>().effect = true;
        }
        if(collider.gameObject.tag == "daruma")
        {
            audioSource.PlayOneShot(goodAction2);
            variableManager.clearMission = true;
        }
        if(collider.gameObject.tag == "Jump")
        {
            if(roleNumber != 2)
            {
                if(!bigJump)
                {
                    audioSource.PlayOneShot(bigJumpAudio);
                    if(animator != null) animator.SetBool("isJumpAnimation", true);
                    bigJump = true;
                    Vector3 vel = rb.velocity;
                    vel.y = 0;
                    rb.velocity = vel;
                    rb.AddForce(Vector3.up * jumpForce * 1.5f, ForceMode.Impulse);
                    isStand = false;
                    myCollider.sharedMaterial = noFriction;   
                }
            }
        }
        if(collider.gameObject.tag == "Jump2")
        {
            if(roleNumber != 2)
            {
                if(!bigJump)
                {
                    audioSource.PlayOneShot(bigJumpAudio);
                    if(animator != null) animator.SetBool("isJumpAnimation", true);
                    bigJump = true;
                    Vector3 vel = rb.velocity;
                    vel.y = 0;
                    rb.velocity = vel;
                    rb.AddForce(Vector3.up * jumpForce * 2.2f, ForceMode.Impulse);
                    isStand = false;
                    myCollider.sharedMaterial = noFriction;   
                }
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag == "Stage")
        {
            isStand = false;
            myCollider.sharedMaterial = noFriction;
            if(animator != null) animator.SetBool("isJumpAnimation", true);
        }
    }
}
