using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

public class EnemyPlayerController : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] Material[] materials;
    [SerializeField] GameObject ManagerObj;
    [SerializeField] GameObject[] prisonObj;
    [SerializeField] Animator animator;
    [SerializeField] Collider myCollider;
    [SerializeField] PhysicMaterial defaultFriction;
    [SerializeField] PhysicMaterial noFriction;
    [SerializeField] Transform darumaTransform;
    [SerializeField] Transform prisonTransform;
    [SerializeField] Transform goalTransform;
    [SerializeField] GameObject randomObj;

    public float mouseX;
    public float mouseY;
    public float speed;
    public float jumpForce;
    public int roleNumber;
    public int stateNumber;
    public bool canMove;
    public string[] pushedKey;
    public enum myState {Idle, Steal, Goal, Chasing, Escape, Mission, Rescue};
    public myState _state = myState.Idle;
    public GameObject targetObj;
    CameraController cameraController;
    PlayerController playerController;
    EnemyPlayerController enemyPlayerController;
    VariableManager variableManager;
    GameObject collisionObj;
    NavMeshAgent agent;
    Transform targetTransform;
    public int jumpCount;
    int jumpCountLimit;
    public bool isStand;
    int missionSub;
    int random;
    int random2;
    float x;
    float z;
    float distance;
    float obstacleCheckDistance = 2.0f;
    float obstacleCheckHeight = 1.5f;
    float lookTimer;
    float headAngle;
    float lookSpeed;
    float timer;
    float chaseTimer = 16;
    bool bigJump;

    void Start()
    {
        GetComponent<Renderer>().material = materials[roleNumber];
        if(roleNumber == 2)
        {
            speed = 20f;
            rb.useGravity = false;
            GetComponent<BoxCollider>().enabled = false;
            randomObj.transform.position += new Vector3(0, 50, 0);
            randomObj.GetComponent<randomTransformSprict>().speed = 100f;
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
        agent = GetComponent<NavMeshAgent>();
        if(agent != null) {
            agent.updatePosition = false; 
            agent.updateRotation = false;
        }
        // 警察（roleNumber == 1）の場合、"DoroOnly" エリアを通れなくする
        if (roleNumber == 1)
        {
            SetAreaMask("DoroOnly", false);
        }
        else
        {
            // 泥棒はすべて通れるようにする
            agent.areaMask = NavMesh.AllAreas;
        }
    }

    void SetAreaMask(string areaName, bool canPass)
    {
        // エリア名からインデックス（番号）を取得
        int areaIndex = NavMesh.GetAreaFromName(areaName);
        
        if (canPass)
        {
            // そのエリアを通れるようにする（ビットを立てる）
            agent.areaMask |= (1 << areaIndex);
        }
        else
        {
            // そのエリアを通れないようにする（ビットを降ろす）
            agent.areaMask &= ~(1 << areaIndex);
        }
    }

    void Update()
    {
        if(variableManager.prisonBreak) stateNumber = 0;

        ConsiderMove();

        if (canMove)
        {
            x = 0; z = 0;
            if (pushedKey.Contains("D")) x += 1;
            if (pushedKey.Contains("A")) x -= 1;
            if (pushedKey.Contains("W")) z += 1;
            if (pushedKey.Contains("S")) z -= 1;
        }
        else
        {
            x = 0; z = 0;
        }
        CheckAndAutoJump();
        Vector3 direction = transform.forward * z + transform.right * x;
        rb.velocity = new Vector3(direction.x * speed, rb.velocity.y, direction.z * speed);
        if (_state == myState.Idle && roleNumber == 1)
        {
            chaseTimer += Time.deltaTime;
            if(chaseTimer <= 15f)
            {
                if(targetObj != null) randomObj.transform.position = targetObj.transform.position;
            }
            else
            {
                agent.updateRotation = false;
                lookSpeed = 5f;
                timer += Time.deltaTime;
                if (timer < 5) lookSpeed = 0.1f;

                lookTimer += Time.deltaTime;
                headAngle = Mathf.Sin(lookTimer * lookSpeed) * 60f;

                Vector3 moveDir = agent.desiredVelocity;
                if (moveDir.magnitude > 0.1f)
                {
                    Quaternion baseRotation = Quaternion.LookRotation(moveDir);
                    Quaternion finalRotation = baseRotation * Quaternion.Euler(0, headAngle, 0);
                    Vector3 rot = finalRotation.eulerAngles;
                    transform.rotation = Quaternion.Euler(0, rot.y, 0);
                }   
            }
        }
        else if (_state == myState.Chasing)
        {
            agent.updateRotation = false;
            GameObject target = GetNearest0Player();
            if (target != null)
            {
                Vector3 targetDir = (target.transform.position - transform.position).normalized;
                targetDir.y = 0;
                
                if (targetDir != Vector3.zero)
                {
                    Quaternion lookRot = Quaternion.LookRotation(targetDir);
                    Quaternion slerpedRot = Quaternion.Slerp(transform.rotation, lookRot, 0.1f);
                    Vector3 rot = slerpedRot.eulerAngles;
                    transform.rotation = Quaternion.Euler(0, rot.y, 0);
                }
            }
            timer = 0;
        }
        else
        {
            if (agent.desiredVelocity.magnitude > 0.5f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(agent.desiredVelocity), 0.1f);
            }
        }
        if(pushedKey.Contains("Space"))
        {
            Debug.Log(gameObject.name + "space" + isStand.ToString());
            if(roleNumber == 2)
            {
                transform.position += Vector3.up * 0.8f;
            }
            else if(isStand)
            {
                Debug.Log(gameObject.name + "space2");
                if(jumpCount < jumpCountLimit)
                {
                    Debug.Log(gameObject.name + "space3");
                    myCollider.sharedMaterial = noFriction;

                    Vector3 vel = rb.velocity;
                    if(!bigJump) vel.y = 0;
                    rb.velocity = vel;

                    RaycastHit hit;
                    Vector3 pushDirection = Vector3.zero;

                    Vector3 rayOrigin = transform.position + Vector3.down * 0.6f;

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
                        rb.AddForce(pushDirection * 25f, ForceMode.Impulse);
                    }

                    rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

                    jumpCount++;
                    isStand = false;
                    pushedKey[4] = "";
                }
            }
        }
        if(pushedKey.Contains("LShift"))
        {
            if(roleNumber == 2)
            {
                transform.position += Vector3.down * 0.8f;
            }
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
        if(pushedKey.Contains("Mouse1"))
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
                if(pushedKey.Contains("RArrow"))
                {
                    variableManager.missionNumber = 0;
                    variableManager.missionSubNumber = 0;
                    variableManager.canMission = false;
                    variableManager.missionStart = true;
                    variableManager.textString = " ";
                    missionSub = -1;
                }
                if(pushedKey.Contains("LArrow"))
                {
                    variableManager.missionNumber = 0;
                    variableManager.missionSubNumber = 1;
                    variableManager.canMission = false;
                    variableManager.missionStart = true;
                    variableManager.textString = " ";
                    missionSub = -1;
                }
                if(pushedKey.Contains("UArrow"))
                {
                    variableManager.missionNumber = 0;
                    variableManager.missionSubNumber = 2;
                    variableManager.canMission = false;
                    variableManager.missionStart = true;
                    variableManager.textString = " ";
                    missionSub = -1;
                }
                if(pushedKey.Contains("DArrow"))
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

    void ConsiderMove()
    {
        for(int i = 0; i < pushedKey.Length; i ++)
        {
            pushedKey[i] = "";
        }
        
        switch (_state)
        {
            case myState.Idle:
                if(roleNumber != 2 && variableManager.missionNumber != -1)
                {
                    _state = myState.Mission;
                }
                if(roleNumber == 0)
                {
                    if(GetNearest1Player() != null && distance <= 100 && IsTargetInSight(GetNearest1Player()))
                    {
                        _state = myState.Escape;
                        break;
                    }
                    if(!variableManager.prisonBreak && stateNumber == 0)
                    {
                        _state = myState.Rescue;
                        break;
                    }
                    _state = myState.Steal;
                    break;                  
                }
                if(roleNumber == 1)
                {
                    if(IsTargetInSight(GetNearest0Player()))
                    {
                        _state = myState.Chasing;   
                    }
                    else
                    {
                        MoveToID(randomObj.transform.position);
                    }
                    break;
                }
                if(roleNumber == 2)
                {
                    MoveToID(randomObj.transform.position);
                    if(randomObj.transform.position.y >= transform.position.y) PushKey(4);
                    else PushKey(5);
                }
                break;

            case myState.Steal:
                if(!variableManager.gemList.Contains(0))
                {
                    _state = myState.Goal;
                    break;
                }
                if(GetNearest1Player() && distance <= 100 && IsTargetInSight(GetNearest1Player()))
                {
                    _state = myState.Escape;
                    break;
                }
                if(!variableManager.prisonBreak && stateNumber == 0)
                {
                    _state = myState.Rescue;
                    break;
                }
                if(GetNearestStrongBox())
                {
                    GameObject targetStrongBox = GetNearestStrongBox();
                    if(distance <= 100)
                    {
                        PushKey(6);
                    }
                    MoveToID(targetStrongBox.transform.position);
                }
                break;

            case myState.Goal:
                if(GetNearest1Player() && distance <= 50 && IsTargetInSight(GetNearest1Player()))
                {
                    _state = myState.Escape;
                    break;
                }
                MoveToID(goalTransform.position);
                break;

            case myState.Chasing:
                GameObject target = GetNearest0Player();
                if (target != null && IsTargetInSight(target)) 
                {
                    MoveToID(target.transform.position);
                    targetObj = target;
                    randomObj.transform.position = targetObj.transform.position;
                }
                else
                {
                    // 見失ったらその場で止まるか、パトロールに切り替える
                    if(_state == myState.Chasing) _state = myState.Idle;
                    chaseTimer = 0;
                }
                break;

            case myState.Escape:
                GameObject police = GetNearest1Player();
                if(!animator.GetBool("isStandAnimation"))
                {
                    animator.SetBool("isStandAnimation", true);
                    speed = 7f;
                }
                if(distance > 100)
                {
                    _state = myState.Idle;
                    break;
                }
                if (police != null)
                {
                    // 1. 基本は警察と反対方向
                    Vector3 runDirection = (transform.position - police.transform.position).normalized;
                    Vector3 escapeTarget = transform.position + runDirection * 8f; // 少し遠めを目標にする

                    // 2. 逃げ先が「角」や「壁の外」でないかチェック
                    UnityEngine.AI.NavMeshHit hit;
                    // ターゲット地点の半径5m以内で、NavMesh上の有効な場所を探す
                    if (UnityEngine.AI.NavMesh.SamplePosition(escapeTarget, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        escapeTarget = hit.position;
                    }
                    else
                    {
                        // 3. もし真後ろが行き止まりなら、左右45度方向に逃げ道がないか試す
                        Vector3 leftPath = Quaternion.Euler(0, 45, 0) * runDirection;
                        Vector3 rightPath = Quaternion.Euler(0, -45, 0) * runDirection;
                        
                        // 左斜め後ろをチェック
                        if (UnityEngine.AI.NavMesh.SamplePosition(transform.position + leftPath * 8f, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                        {
                            escapeTarget = hit.position;
                        }
                        // 右斜め後ろをチェック
                        else if (UnityEngine.AI.NavMesh.SamplePosition(transform.position + rightPath * 8f, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                        {
                            escapeTarget = hit.position;
                        }
                    }

                    MoveToID(escapeTarget);
                }
                break;

            case myState.Mission:
                if(variableManager.missionNumber == 0)
                {
                    MoveToID(darumaTransform.position);
                }
                break;
            case myState.Rescue:
                if(variableManager.prisonBreak || stateNumber == 1)
                {
                    _state = myState.Idle;
                    break;
                }
                MoveToID(prisonTransform.position);
                break;
            default:
                break;
        }
    }

    void MoveToID(Vector3 destination)
    {
        if (agent == null) return;

        agent.SetDestination(destination);
        Vector3 dir = destination - transform.position;
        dir.y = 0;

        if (dir.magnitude > 0.1f)
        {
            // 向きを安定させる
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 0.15f);

            // ローカル方向に変換
            Vector3 localDir = transform.InverseTransformDirection(dir.normalized);

            // 前後
            if (localDir.z > 0.3f) PushKey(2);      // W
            else if (localDir.z < -0.3f) PushKey(3); // S

            // 左右
            if (localDir.x > 0.3f) PushKey(0);      // D
            else if (localDir.x < -0.3f) PushKey(1); // A
        }
    }

    GameObject GetNearest0Player()
    {
        // "Player"タグが付いているすべてのオブジェクトを取得
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject nearestPlayer = null;
        float minDistance = float.MaxValue;
        Vector3 currentPos = transform.position;

        foreach (GameObject p in players)
        {
            if (p == gameObject) continue;
            playerController = p.gameObject.GetComponent<PlayerController>();
            if(playerController != null && (playerController.roleNumber == 2 || playerController.stateNumber == 1)) continue;
            enemyPlayerController = p.gameObject.GetComponent<EnemyPlayerController>();
            if(enemyPlayerController != null && (enemyPlayerController.roleNumber == 2 || enemyPlayerController.stateNumber == 1)) continue;
            // 距離を計算（平方根の計算を避けるためsqrMagnitudeを使うと高速です）
            float dist = (p.transform.position - currentPos).sqrMagnitude;
            
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestPlayer = p;
            }
        }

        return nearestPlayer;
    }

    GameObject GetNearest1Player()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject nearest1Player = null;
        float minDistance = float.MaxValue;
        Vector3 currentPos = transform.position;

        foreach (GameObject p in players)
        {
            if (p == gameObject) continue;

            // 相手のコンポーネントを取得
            PlayerController pc = p.GetComponent<PlayerController>();
            EnemyPlayerController epc = p.GetComponent<EnemyPlayerController>();

            // 警察(roleNumber 1)かどうかを判定
            bool isPolice = (pc != null && pc.roleNumber == 1) || (epc != null && epc.roleNumber == 1);

            // 警察じゃないなら無視
            if (!isPolice) continue;

            float dist = (p.transform.position - currentPos).sqrMagnitude;
            
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest1Player = p;
            }
        }

        // 全員チェックし終わったあと、一番近い警察との距離を代入
        distance = minDistance; 

        // デバッグログ：見つからなかった場合は "None" と出す
        // Debug.Log($"{gameObject.name} nearest police distance: {distance} (Target: {(nearest1Player != null ? nearest1Player.name : "None")})");

        return nearest1Player;
    }

    GameObject GetNearestStrongBox()
    {
        GameObject[] boxs = GameObject.FindGameObjectsWithTag("StrongBox");
        GameObject nearestStorongBox = null;
        float minDistance = float.MaxValue;
        Vector3 currentPos = transform.position;

        foreach (GameObject b in boxs)
        {
            if (variableManager.gemList[b.GetComponent<StrongBoxScript>().boxNumber] == 1) continue;

            float dist = (b.transform.position - currentPos).sqrMagnitude;
            
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestStorongBox = b;
            }
        }

        // 全員チェックし終わったあと、一番近い警察との距離を代入
        distance = minDistance; 

        return nearestStorongBox;
    }

    void CheckAndAutoJump()
    {
        if (!isStand) return;
        if (jumpCount >= jumpCountLimit) return;

        // originの高さのみ 0.5f から 0.2f に変更。これで低い段差を正面から捉えます
        Vector3 origin = transform.position + Vector3.up * 0.2f + -transform.forward * 2.5f; 
        Vector3 dir = transform.forward;
        float radius = 0.5f; 
        float distance = obstacleCheckDistance + 2.5f;

        RaycastHit[] hits = Physics.SphereCastAll(origin, radius, dir, distance);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var h in hits)
        {
            if (h.collider.gameObject == gameObject) continue;

            if (h.collider.CompareTag("Stage"))
            {
                float heightDiff = h.point.y - transform.position.y;

                // 条件を最初のもの(>-5f)に完全に戻しました。
                if (heightDiff > -5f && heightDiff <= obstacleCheckHeight)
                {
                    PushKey(4);
                    return;
                }
            }
        }
    }

    bool IsTargetInSight(GameObject target)
    {
        if (target == null) return false;

        Vector3 targetPos = target.transform.position;
        Vector3 myPos = transform.position + Vector3.up * 1.5f + Vector3.back * 1f;
        Vector3 directionToTarget = (targetPos - myPos).normalized;

        float distanceToTarget = Vector3.Distance(myPos, targetPos);
        if (distanceToTarget > 1000f) return false;

        float angle = Vector3.Angle(transform.forward, directionToTarget);
        if (angle > 50f) return false;

        // 全てのヒット情報を取得（自分も他人も含めて全部突き抜ける）
        RaycastHit[] hits = Physics.RaycastAll(myPos, directionToTarget, distanceToTarget);
        
        // ヒットしたものを距離が近い順に並び替える
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            // 自分自身に当たった場合は無視して次のヒットへ（突き抜ける処理）
            if (hit.collider.gameObject == gameObject) continue;

            // 自分以外で最初に当たったのがターゲットなら「見える」
            if (hit.collider.gameObject == target)
            {
                return true;
            }
            else
            {
                // ターゲットより手前に自分以外の何か（壁など）があったら遮蔽されている
                return false;
            }
        }

        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 myPos = transform.position + Vector3.up * 1.5f;
        
        // 前方の視界限界線を書く
        Vector3 leftRayDirection = Quaternion.AngleAxis(-30, Vector3.up) * transform.forward;
        Vector3 rightRayDirection = Quaternion.AngleAxis(30, Vector3.up) * transform.forward;

        Gizmos.DrawRay(myPos, leftRayDirection * 60f);
        Gizmos.DrawRay(myPos, rightRayDirection * 60f);
        Gizmos.DrawLine(myPos + leftRayDirection * 60f, myPos + rightRayDirection * 60f);
    }

    void PushKey(int pushKeyNumber)
    {
        if(pushKeyNumber == 0)
        {
            pushedKey[pushKeyNumber] = "D";
        }
        if(pushKeyNumber == 1)
        {
            pushedKey[pushKeyNumber] = "A";
        }
        if(pushKeyNumber == 2)
        {
            pushedKey[pushKeyNumber] = "W";
        }
        if(pushKeyNumber == 3)
        {
            pushedKey[pushKeyNumber] = "S";
        }
        if(pushKeyNumber == 4)
        {
            pushedKey[pushKeyNumber] = "Space";
        }
        if(pushKeyNumber == 5)
        {
            pushedKey[pushKeyNumber] = "LShift";
        }
        if(pushKeyNumber == 6)
        {
            pushedKey[pushKeyNumber] = "Mouse1";
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.CompareTag("Stage"))
        {
            bigJump = false;
            foreach (ContactPoint contact in collision.contacts)
            {
                // 上向きの法線（地面がプレイヤーを押し上げている状態）かチェック
                // 0.6f くらいにすると、ある程度の坂道も地面として認められます
                if (contact.normal.y > 0.6f)
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
            playerController = collisionObj.GetComponent<PlayerController>();
            enemyPlayerController = collisionObj.GetComponent<EnemyPlayerController>();
            if((enemyPlayerController != null && enemyPlayerController.roleNumber == 1) || (playerController != null && playerController.roleNumber == 1))
            {
                if(roleNumber == 0)
                {
                    stateNumber = 1;
                    transform.position = prisonTransform.transform.position;
                    variableManager.prisonBreak = false;
                }                
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
        if(collider.gameObject.CompareTag("Jump"))
        {
            if(roleNumber != 2)
            {
                if(!bigJump)
                {
                    bigJump = true;
                    rb.AddForce(Vector3.up * jumpForce * 3.2f, ForceMode.Impulse);
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
        }
    }
}
