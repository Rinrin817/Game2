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
    [SerializeField] GameObject[] blenderObj;
    [SerializeField] Collider myCollider;
    [SerializeField] PhysicMaterial defaultFriction;
    [SerializeField] PhysicMaterial noFriction;
    [SerializeField] Transform[] darumaTransform;
    [SerializeField] Transform[] prisonTransformArray;
    [SerializeField] Transform[] goalTransformArray;
    [SerializeField] GameObject randomObj;
    [SerializeField] AudioClip goodAction;

    public float mouseX;
    public float mouseY;
    public float speed;
    public float jumpForce;
    public int roleNumber;
    public int stateNumber;
    public bool isStand;
    public bool isFalling;
    public bool canMove;
    public string[] pushedKey;
    public enum myState {Idle, Steal, Goal, Chasing, Escape, Mission, Rescue};
    public myState _state = myState.Idle;
    public GameObject targetObj;
    public GameObject[] playersObj;
    public Transform[] playersTransform;

    Animator animator;
    AudioSource audioSource;
    Transform prisonTransform;
    Transform goalTransform;
    CameraController cameraController;
    Vector3 lastDestination;
    Vector3 nowDestination;
    PlayerController playerController;
    EnemyPlayerController enemyPlayerController;
    Vector3[] cachedCorners;
    int cornerIndex = 1;
    Quaternion targetRot;
    Vector3 targetDir;
    VariableManager variableManager;
    GameObject collisionObj;
    NavMeshAgent agent;
    Transform targetTransform;
    int jumpCount;
    int jumpCountLimit;
    int missionSub;
    int random;
    int random2;
    float randomDis;
    float x;
    float z;
    float distance;
    float obstacleCheckDistance = 1.0f;
    float obstacleCheckHeight = 3f;
    float lookTimer;
    float headAngle;
    float lookSpeed;
    float timer;
    float chaseTimer = 16;
    float[] transformTimer = new float[5];
    bool bigJump;
    bool wishJump;
    bool hasExtraInput;
    int wishMissionNumber;
    Vector3 cachedCorner;
    bool hasCachedCorner = false;
    Quaternion cachedRot;
    bool hasCachedRot = false;
    bool darumaTransformSet;

    void Start()
    {
        prisonTransform = prisonTransformArray[VariableManager.stageNumber];
        goalTransform = goalTransformArray[VariableManager.stageNumber];
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
            randomObj.transform.position += new Vector3(0, 50, 0);
            randomObj.GetComponent<randomTransformSprict>().speed = 200f;
        }
        else
        {
            speed = 7f;
            randomObj.GetComponent<randomTransformSprict>().speed = 50f;
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
        wishJump = false;
        jumpCountLimit = 1;
        stateNumber = 0;
        missionSub = -1;
        canMove = true;
        darumaTransformSet = false;
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
        agent.nextPosition = transform.position;
        Vector3 agentPos = agent.nextPosition;
        Vector3 realPos = transform.position;
        float dist = Vector3.Distance(agentPos, realPos);
        if (dist >= 3.0f)
        {
            agent.Warp(transform.position);
        }
        CheckAndAutoJump();
        if (canMove)
        {
            x = 0; z = 0;
            if (pushedKey.Contains("D")) x += 1;
            if (pushedKey.Contains("A")) x -= 1;
            if (pushedKey.Contains("W")) z += 1;
            if (pushedKey.Contains("S")) z -= 1;
            if(animator != null)
            {
                if (!pushedKey.Contains("D") && !pushedKey.Contains("A") && !pushedKey.Contains("W") && !pushedKey.Contains("S")) animator.SetBool("isRunAnimation", false);
                else animator.SetBool("isRunAnimation", true);   
            }
        }
        else
        {
            if(animator != null) animator.SetBool("isRunAnimation", false);
            x = 0; z = 0;
        }
        Vector3 direction = transform.forward * z + transform.right * x;
        rb.velocity = new Vector3(direction.x * speed, rb.velocity.y, direction.z * speed);
        if (_state == myState.Idle && roleNumber == 1)
        {
            chaseTimer += Time.deltaTime;
            if(targetObj != null)
            {
                PlayerController playerController = targetObj.GetComponent<PlayerController>();
                enemyPlayerController = targetObj.GetComponent<EnemyPlayerController>();   
            }
            if((playerController != null && playerController.stateNumber == 1) || (enemyPlayerController != null && enemyPlayerController.stateNumber == 1))
            {
                chaseTimer = 20f;
                targetObj = null;
            }
            if(chaseTimer <= 15f)
            {
                if(targetObj != null) randomObj.transform.position = targetObj.transform.position;
                chaseTimer = 0;
            }
            else
            {
                lookSpeed = 5f;
                timer += Time.deltaTime;
                if (timer < 5) lookSpeed = 0.1f;

                lookTimer += Time.deltaTime;
                headAngle = Mathf.Sin(lookTimer * lookSpeed) * 60f;
            }
        }
        else if (_state == myState.Chasing)
        {
            timer = 0;
        }
        if(roleNumber == 2)
        {
            for(int i = 0; i < 5; i ++)
            {
                transformTimer[i] += Time.deltaTime;
            }
        }
        if(pushedKey.Contains("Space"))
        {
            // Debug.Log(gameObject.name + "space" + isStand.ToString());
            if(roleNumber == 2)
            {
                transform.position += Vector3.up * 0.6f;
            }
            else if(isStand && canMove)
            {
                // Debug.Log(gameObject.name + "space2");
                if(jumpCount < jumpCountLimit)
                {
                    // Debug.Log(gameObject.name + "space3");
                    myCollider.sharedMaterial = noFriction;
                    if(animator != null) animator.SetBool("isJumpAnimation", true);

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
                transform.position += Vector3.down * 0.6f;
            }
            if(roleNumber == 0)
            {
                if(animator.GetBool("isStandAnimation"))
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
        if(pushedKey.Contains("Mouse1"))
        {
            if(roleNumber == 2 && variableManager.canMission)
            {
                if(pushedKey.Contains("1"))
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
        if (!hasExtraInput)
        {
            for(int i = 0; i < pushedKey.Length; i++)
                pushedKey[i] = "";
        }
        hasExtraInput = false;
        
        switch (_state)
        {
            case myState.Idle:
                if(roleNumber != 2 && variableManager.missionNumber != -1)
                {
                    darumaTransformSet = false;
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
                        float distA = Vector3.Distance(agent.transform.position, prisonTransform.position);
                        float distB = 10000f;
                        if(GetNearestStrongBox() != null) distB = Vector3.Distance(agent.transform.position, GetNearestStrongBox().transform.position);
                        if(distA < distB || distB > 100f)
                        {
                            _state = myState.Rescue;
                            break;   
                        }
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
                    int nullCount = 0;
                    for(int i = 0; i < 5; i ++)
                    {
                        if(transformTimer[i] != null && transformTimer[i] > 20f)
                        {
                            playersTransform[i] = null;
                        }
                        if(playersTransform[i] == null)
                        {
                            nullCount ++;
                            if(IsTargetInSight(playersObj[i]))
                            {
                                playersTransform[i] = playersObj[i].transform;
                                transformTimer[i] = 0f;
                            }
                            if(playersObj[i].transform == gameObject.transform)
                            {
                                playersTransform[i] = playersObj[i].transform;
                            }
                        }
                    }
                    Vector3 dir = randomObj.transform.position - transform.position;
                    dir.y = 0;
                    if (dir.sqrMagnitude < 0.001f) return;
                    // 回転も入力も同じ基準にする
                    Vector3 localDir = transform.InverseTransformDirection(dir);
                    // 回転
                    Quaternion targetRot = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation,
                        targetRot,
                        540f * Time.deltaTime // 少し強める
                    );
                    if (localDir.z > 0.1f) PushKey(2);
                    if (localDir.x > 0.3f) PushKey(0);
                    if (localDir.x < -0.3f) PushKey(1);
                    if (localDir.z <= 0.1f && localDir.x <= 0.3f && localDir.x >= -0.3f) PushKey(3);
                    
                    if (randomObj.transform.position.y > transform.position.y) PushKey(4);
                    else if(transform.position.y - randomObj.transform.position.y >= 10) PushKey(5);
                    if(nullCount == 0 && variableManager.canMission)
                    {
                        darumaTransformSet = false;
                        _state = myState.Mission;
                        break;
                    }
                }
                break;

            case myState.Steal:
                if(roleNumber != 2 && variableManager.missionNumber != -1)
                {
                    darumaTransformSet = false;
                    _state = myState.Mission;
                    break;
                }
                if(!variableManager.gemList.Contains(0))
                {
                    _state = myState.Goal;
                    break;
                }
                if(GetNearest1Player() && distance <= 150 && IsTargetInSight(GetNearest1Player()))
                {
                    float distB = Vector3.Distance(agent.transform.position, GetNearestStrongBox().transform.position);
                    if(distB > 10f)
                    {
                        _state = myState.Rescue;
                        break;   
                    }
                }
                if(GetNearest1Player() && distance <= 100 && IsTargetInSight(GetNearest1Player()))
                {
                    _state = myState.Escape;
                    break;
                }
                if(!variableManager.prisonBreak && stateNumber == 0)
                {
                    float distA = Vector3.Distance(agent.transform.position, prisonTransform.position);
                    float distB = 10000f;
                    if(GetNearestStrongBox() != null) distB = Vector3.Distance(agent.transform.position, GetNearestStrongBox().transform.position);
                    if(distA < distB || distB > 100f)
                    {
                        _state = myState.Rescue;
                        break;   
                    }
                }
                if(GetNearestStrongBox())
                {
                    GameObject targetStrongBox = GetNearestStrongBox();
                    if(distance <= 100)
                    {
                        PushKey(6);
                    }
                    MoveToID(targetStrongBox.transform.position);
                    break;
                }
                _state = myState.Idle;
                break; 

            case myState.Goal:
                if(GetNearest1Player() && distance <= 100 && IsTargetInSight(GetNearest1Player()))
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
                    if(variableManager.missionNumber != -1 && Vector3.Distance(agent.transform.position, target.transform.position) > 10f)
                    {
                        darumaTransformSet = false;
                        _state = myState.Mission;
                        break;
                    }
                    else
                    {
                        MoveToID(target.transform.position);
                        targetObj = target;
                        randomObj.transform.position = targetObj.transform.position;   
                    }
                }
                else
                {
                    // 見失ったらその場で止まるか、パトロールに切り替える
                    if(_state == myState.Chasing) _state = myState.Idle;
                    chaseTimer = 0;
                }
                break;

            case myState.Escape:
                if(roleNumber != 2 && variableManager.missionNumber != -1)
                {
                    darumaTransformSet = false;
                    _state = myState.Mission;
                }
                GameObject police = GetNearest1Player();
                if(animator != null && !animator.GetBool("isStandAnimation"))
                {
                    if(animator != null) animator.SetBool("isStandAnimation", true);
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
                if(roleNumber == 2)
                {
                    int nullCount = 0;
                    for(int i = 0; i < 5; i ++)
                    {
                        if(playersTransform[i] == null)
                        {
                            nullCount ++;
                            if(IsTargetInSight(playersObj[i]))
                            {
                                playersTransform[i] = playersObj[i].transform;
                            }
                            if(playersObj[i].transform == gameObject.transform)
                            {
                                playersTransform[i] = playersObj[i].transform;
                            }
                        }
                    }
                    if(nullCount != 0)
                    {
                        _state = myState.Idle;
                        break;
                    }
                    if(!variableManager.canMission)
                    {
                        _state = myState.Idle;
                        break;
                    }
                    if(variableManager.canMission)
                    {
                        PushKey(6);
                        PushKey(7);
                        wishMissionNumber = 0;
                    }
                    if(wishMissionNumber == 0)
                    {
                        int right = 0, left = 0, up = 0, down = 0;

                        for (int i = 0; i < 5; i++)
                        {
                            if (playersTransform[i] == null) continue;
                            if (playersTransform[i].position.x > 0) right++;
                            else left++;

                            if (playersTransform[i].position.z > 0) up++;
                            else down++;
                        }

                        int averageDirection;

                        if (right > left && right > up && right > down) averageDirection = 0;
                        else if (left > up && left > down) averageDirection = 1;
                        else if (up > down) averageDirection = 2;
                        else averageDirection = 3;
                        
                        if(averageDirection == 0) //右
                        {
                            PushKey(9); //左
                        }
                        if(averageDirection == 1) //左
                        {
                            PushKey(8); //右
                        }
                        if(averageDirection == 2) //上
                        {
                            PushKey(11); //下
                        }
                        if(averageDirection == 3) //下
                        {
                            PushKey(10); //上
                        }
                    }
                    break;
                }
                if(variableManager.missionNumber == 0)
                {
                    //Debug.Log(gameObject.name + "Go to DARUMA");
                    //Debug.Log((darumaTransform.position).ToString());
                    float minDist = 10000f;
                    int number = 0;
                    if(darumaTransformSet)
                    {
                        for(int i = 0; i < darumaTransform.Length; i ++)
                        {
                            if(minDist - randomDis > Vector3.Distance(agent.transform.position, darumaTransform[i].transform.position))
                            {
                                minDist = Vector3.Distance(agent.transform.position, darumaTransform[i].transform.position);
                                number = i;
                            }
                        }   
                    }
                    else
                    {
                        darumaTransformSet = true;
                        for(int i = 0; i < darumaTransform.Length; i ++)
                        {
                            if(minDist > Vector3.Distance(agent.transform.position, darumaTransform[i].transform.position))
                            {
                                minDist = Vector3.Distance(agent.transform.position, darumaTransform[i].transform.position);
                                number = i;
                            }
                        }
                        randomDis = Random.Range(1f, 100f);
                    }
                    MoveToID(darumaTransform[number].position);
                }
                if(roleNumber == 0 && GetNearest1Player() && distance <= 100f && IsTargetInSight(GetNearest1Player()))
                {
                    _state = myState.Escape;
                    break;
                }
                if(variableManager.missionNumber == -1)
                {
                    darumaTransformSet = false;
                    _state = myState.Idle;
                    break;
                }
                break;

            case myState.Rescue:
                if(GetNearest1Player() && distance <= 200 && IsTargetInSight(GetNearest1Player()))
                {
                    _state = myState.Escape;
                    break;
                }
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

    Vector3 GetSteeringDirection(NavMeshPath path)
    {
        if (path.corners == null || path.corners.Length < 2)
            return Vector3.zero;

        // 一番近い次のコーナーを探す
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            float dist = Vector3.Distance(transform.position, path.corners[i]);

            if (dist > 0.5f)
            {
                return path.corners[i] - transform.position;
            }
        }

        return path.corners[1] - transform.position;
    }

    void MoveToID(Vector3 destination)
    {
        if (agent == null) return;
        nowDestination = destination;

        if (!isStand || isFalling)
        {
            if(roleNumber != 2)
            {
                LookDestinationPath(90f);
            }
            PushKey(2);
            return;
        }

        if (!agent.isOnNavMesh) return;
        if (hasExtraInput)
        {
            LookDestinationPath(90);
            return;
        }
        if (IsDropRequired(destination))
        {
            StartDrop();
            PushKey(2);
            return;
        }

        NavMeshHit hit;
        Vector3 validDestination = destination;

        if (NavMesh.SamplePosition(destination, out hit, 5.0f, NavMesh.AllAreas))
        {
            validDestination = hit.position;
        }

        // ルート比較
        NavMeshPath newPath = new NavMeshPath();
        bool canCalc = NavMesh.CalculatePath(transform.position, validDestination, NavMesh.AllAreas, newPath);

        bool shouldSet = false;

        if (canCalc && newPath.status != NavMeshPathStatus.PathInvalid)
        {
            float newLen = GetPathLength(newPath);
            float currentLen = GetPathLength(agent.path);

            float threshold = 10.0f;

            if (!agent.hasPath || newLen < currentLen - threshold || agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                shouldSet = true;
            }
        }
        else
        {
            if (!agent.hasPath)
            {
                shouldSet = true;
            }
        }

        if (shouldSet || Vector3.Distance(agent.destination, validDestination) > 0.5f)
        {
            agent.SetDestination(validDestination);
            lastDestination = validDestination;
        }

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid && !agent.pathPending)
        {
            agent.SetDestination(validDestination);
        }

        NavMeshPath path = agent.path;
        if (path.corners != null && path.corners.Length >= 2)
        {
            targetDir = path.corners[1] - transform.position;
        }
        else
        {
            targetDir = validDestination - transform.position;
        }

        targetDir.y = 0f;
        targetDir.Normalize();
        targetRot = Quaternion.LookRotation(targetDir);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            180f * Time.deltaTime
        );

        Vector3 localDir = transform.InverseTransformDirection(targetDir);
        if (isStand)
        {
            if (localDir.z > 0.1f) PushKey(2);
            if (localDir.x > 0.3f) PushKey(0);
            if (localDir.x < -0.3f) PushKey(1);
            if (localDir.z <= 0.1f && localDir.x <= 0.3f && localDir.x >= -0.3f) PushKey(3);
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
            if(playerController != null && (playerController.roleNumber == 2 || playerController.roleNumber == 1 || playerController.stateNumber == 1)) continue;
            enemyPlayerController = p.gameObject.GetComponent<EnemyPlayerController>();
            if(enemyPlayerController != null && (enemyPlayerController.roleNumber == 2 || enemyPlayerController.roleNumber == 1 || enemyPlayerController.stateNumber == 1)) continue;
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
        GameObject nearestGem= null;
        float minDistance = float.MaxValue;
        Vector3 currentPos = transform.position;

        foreach (GameObject b in boxs)
        {
            if (variableManager.gemList[b.GetComponent<StrongBoxScript>().boxNumber] == 1) continue;

            float dist = (b.transform.position - currentPos).sqrMagnitude;
            
            if (dist < minDistance)
            {
                minDistance = dist;
                foreach (Transform child in b.transform)
                {
                    if (child.CompareTag("Gem"))
                    {
                        nearestGem = child.gameObject;
                    }
                }
            }
        }

        distance = minDistance; 

        return nearestGem;
    }

    /*
    void CheckAndAutoJump()
    {
        if (!isStand) return;
        if (jumpCount >= jumpCountLimit) return;

        if(wishJump)
        {
            PushKey(4);
            PushKey(2);
            return;
        }

        Vector3 origin = transform.position + Vector3.up * 0.3f + -transform.forward * 1f; 
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
                float heightDiff = h.collider.gameObject.transform.localScale.y / 2 + h.collider.gameObject.transform.position.y - (transform.position.y - transform.localScale.y / 2);

                if (heightDiff > 0f && heightDiff <= obstacleCheckHeight)
                {
                    origin = transform.position + Vector3.up * 2f + -transform.forward * 1f;
                    RaycastHit[] hits2 = Physics.SphereCastAll(origin, radius, dir, distance);
                    foreach(var h2 in hits2)
                    {
                        if (h2.collider.gameObject == gameObject) continue;
                        if (h2.collider.CompareTag("Stage"))
                        {
                            return;
                        }
                    }
                    PushKey(4);
                    return;
                }
            }
        }
        
        origin = transform.position + Vector3.up * 1f + -transform.forward * 2.5f; 
        hits = Physics.SphereCastAll(origin, radius, dir, distance);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var h in hits)
        {
            if (h.collider.gameObject == gameObject) continue;

            if (h.collider.CompareTag("Stage"))
            {
                float heightDiff = h.collider.gameObject.transform.localScale.y / 2 + h.collider.gameObject.transform.position.y - (transform.position.y - transform.localScale.y / 2);

                if (heightDiff > 0f && heightDiff <= obstacleCheckHeight)
                {
                    origin = transform.position + Vector3.up * 2f + -transform.forward * 1f;
                    RaycastHit[] hits2 = Physics.SphereCastAll(origin, radius, dir, distance);
                    foreach(var h2 in hits2)
                    {
                        if (h2.collider.gameObject == gameObject) continue;
                        if (h2.collider.CompareTag("Stage"))
                        {
                            return;
                        }
                    }
                    PushKey(4);
                    return;
                }
            }
        }
    }
    */

void CheckAndAutoJump()
{
    if (!isStand) return;
    if (jumpCount >= jumpCountLimit) return;

    if (wishJump)
    {
        PushKey(4);
        PushKey(2);
        return;
    }

    Vector3 origin =
        transform.position +
        Vector3.up * 0.1f +
        transform.forward * 0.2f;

    Vector3 dir = transform.forward;

    float radius = 0.45f;
    float distance = obstacleCheckDistance + 2f;

    RaycastHit[] hits =
        Physics.SphereCastAll(origin, radius, dir, distance);

    System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

    foreach (var h in hits)
    {
        if (h.collider.gameObject == gameObject) continue;
        if (!h.collider.CompareTag("Stage")) continue;

        // =========================
        // 接触地点の高さを見る
        // =========================
        float hitHeight = h.point.y;

        float myBottom =
            transform.position.y -
            transform.localScale.y / 2f;

        float heightDiff = hitHeight - myBottom;

        // Debug.Log(heightDiff);

        if (heightDiff > 0.02f &&
            heightDiff <= obstacleCheckHeight)
        {
            // =========================
            // 頭上チェック
            // =========================
            Vector3 upperOrigin =
                transform.position +
                Vector3.up * 1.3f;

            RaycastHit[] upperHits =
                Physics.SphereCastAll(
                    upperOrigin,
                    0.35f,
                    dir,
                    1.2f
                );

            bool blocked = false;

            foreach (var uh in upperHits)
            {
                if (uh.collider.gameObject == gameObject)
                    continue;

                if (uh.collider.CompareTag("Stage"))
                {
                    blocked = true;
                    break;
                }
            }

            if (blocked) return;

            PushKey(4);
            return;
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
        if(pushKeyNumber == 7)
        {
            pushedKey[pushKeyNumber] = "1";
        }
        if(pushKeyNumber == 8)
        {
            pushedKey[pushKeyNumber] = "RArrow";
        }
        if(pushKeyNumber == 9)
        {
            pushedKey[pushKeyNumber] = "LArrow";
        }
        if(pushKeyNumber == 10)
        {
            pushedKey[pushKeyNumber] = "UArrow";
        }
        if(pushKeyNumber == 11)
        {
            pushedKey[pushKeyNumber] = "DArrow";
        }
    }

    bool IsDropRequired(Vector3 destination)
    {
        float heightDiff = transform.position.y - destination.y;
        if (heightDiff <= 1.5f) return false;

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 forward = transform.forward;

        // 前に壁があるか
        if (Physics.Raycast(origin, forward, 1.0f))
        {
            // 壁なら落ちない
            return false;
        }

        // 壁が無くて高さ差あり → 落下
        return true;
    }

    void StartDrop()
    {
        isFalling = true;
    }

    float GetPathLength(NavMeshPath path)
    {
        if (path == null || path.corners == null || path.corners.Length < 2)
            return float.MaxValue;

        float length = 0f;
        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }
        return length;
    }
    /*
    void LookDestinationPath(float rotSpeed)
    {
        Vector3 destination = nowDestination;

        // 地上のみNavMesh更新
        if (isStand)
        {
            agent.SetDestination(destination);
            if (agent.hasPath)
            {
                cachedCorners = agent.path.corners;
                cornerIndex = 1;
            }
        }

        // 方向(targetDir)の決定
        if (!isStand && cachedCorners != null && cornerIndex < cachedCorners.Length)
        {
            Vector3 targetCorner = cachedCorners[cornerIndex];
            targetCorner.y = transform.position.y; // 高さを固定

            if (Vector3.Distance(transform.position, targetCorner) < 1.0f)
            {
                cornerIndex++;
            }
            targetDir = targetCorner - transform.position;
        }
        else if (agent.hasPath && agent.path.corners.Length >= 2)
        {
            targetDir = agent.path.corners[1] - transform.position;
        }

        // 回転の実行
        targetDir.y = 0;
        if (targetDir.sqrMagnitude > 0.01f)
        {
            // ★ここで計算した方向を回転値に変換する
            Quaternion targetRot = Quaternion.LookRotation(targetDir);

            if (Quaternion.Angle(transform.rotation, targetRot) > 1f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, targetRot, rotSpeed * Time.deltaTime);
            }
            else
            {
                transform.rotation = targetRot;
            }
        }
    }
    */
    
    void LookDestinationPath(float rotSpeed)
    {
        NavMeshHit hit;
        Vector3 validDestination = nowDestination;

        if (NavMesh.SamplePosition(nowDestination, out hit, 5.0f, NavMesh.AllAreas))
        {
            validDestination = hit.position;
        }

        // ルート比較
        NavMeshPath newPath = new NavMeshPath();
        bool canCalc = NavMesh.CalculatePath(transform.position, validDestination, NavMesh.AllAreas, newPath);

        bool shouldSet = false;

        if (canCalc && newPath.status != NavMeshPathStatus.PathInvalid)
        {
            float newLen = GetPathLength(newPath);
            float currentLen = GetPathLength(agent.path);

            float threshold = 50.0f;

            if (!agent.hasPath || newLen < currentLen - threshold || agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                shouldSet = true;
            }
        }
        else
        {
            if (!agent.hasPath)
            {
                shouldSet = true;
            }
        }

        if (shouldSet || Vector3.Distance(agent.destination, validDestination) > 0.5f)
        {
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.SetDestination(validDestination);
            }
            lastDestination = validDestination;
        }

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid && !agent.pathPending)
        {
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.SetDestination(validDestination);
            }
        }

        NavMeshPath path = agent.path;
        if (path.corners != null && path.corners.Length >= 2)
        {
            targetDir = path.corners[1] - transform.position;
        }
        else
        {
            targetDir = validDestination - transform.position;
        }

        targetDir.y = 0;
        if (targetDir.sqrMagnitude > 0.01f)
        {
            // ★ここで計算した方向を回転値に変換する
            Quaternion targetRot = Quaternion.LookRotation(targetDir);

            if (Quaternion.Angle(transform.rotation, targetRot) > 50f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, targetRot, rotSpeed * Time.deltaTime);
            }
            else
            {
                transform.rotation = targetRot;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Stage"))
        {
            Vector3 agentPos = agent.nextPosition;
            Vector3 realPos = transform.position;
            float dist = Vector3.Distance(agentPos, realPos);
            if (dist >= 1.0f)
            {
                agent.Warp(transform.position);
            }
            if(roleNumber != 2)
            {
                LookDestinationPath(90f);
            }
            if (isFalling)
            {
                isFalling = false;
                NavMeshHit hit;
                if (!agent.isOnNavMesh) return;
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.CompareTag("Stage"))
        {
            if(roleNumber != 2)
            {
                LookDestinationPath(90f);
            }
            if(animator != null) animator.SetBool("isJumpAnimation", false);
            bigJump = false;
            isFalling = false;
            if (!agent.isOnNavMesh) return;
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
            Vector3 escapeDir = Vector3.zero;

            foreach (var contact in collision.contacts)
            {
                // 壁の法線（壁が向いている方向）
                Vector3 normal = contact.normal;

                // プレイヤーを押し返す方向（反対）
                escapeDir += normal;
            }

            escapeDir.y = 0f;
            escapeDir.Normalize();

            if (escapeDir.sqrMagnitude > 0.01f)
            {
                Vector3 local = transform.InverseTransformDirection(escapeDir);

                Debug.Log(gameObject.name + "ExtraInput");
                // どの方向キーを押すか決める
                if(Random.Range(0, 5) == 0) PushKey(4);
                if (local.z > 0.1f) PushKey(2);   // W
                else if (local.z < -0.1f) PushKey(3); // S
                else if (local.x > 0.1f) PushKey(0);  // D
                else if (local.x < -0.1f) PushKey(1); // A
                LookDestinationPath(360f);
                hasExtraInput = true;
            }
        }
        if(collision.gameObject.CompareTag("Player"))
        {
            collisionObj = collision.gameObject;
            playerController = collisionObj.GetComponent<PlayerController>();
            enemyPlayerController = collisionObj.GetComponent<EnemyPlayerController>();
            if(enemyPlayerController != null && enemyPlayerController.roleNumber == 1)
            {
                if(roleNumber == 0 && stateNumber == 0)
                {
                    stateNumber = 1;
                    transform.position = prisonTransform.transform.position;
                    variableManager.prisonBreak = false;
                }                
            }
            if(playerController != null && playerController.roleNumber == 1)
            {
                if(roleNumber == 0 && stateNumber == 0)
                {
                    audioSource.PlayOneShot(goodAction);
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
        if(collider.gameObject.tag == "Jump")
        {
            if(roleNumber != 2)
            {
                LookDestinationPath(360f);
                if(!bigJump)
                {
                    bigJump = true;
                    Vector3 vel = rb.velocity;
                    vel.y = 0;
                    rb.velocity = vel;
                    rb.AddForce(Vector3.up * jumpForce * 1.5f, ForceMode.Impulse);
                    isStand = false;
                    myCollider.sharedMaterial = noFriction;
                    if(animator != null) animator.SetBool("isJumpAnimation", true);
                }
            }
        }
        if(collider.gameObject.tag == "Jump2")
        {
            if(roleNumber != 2)
            {
                LookDestinationPath(360f);
                if(!bigJump)
                {
                    bigJump = true;
                    Vector3 vel = rb.velocity;
                    vel.y = 0;
                    rb.velocity = vel;
                    rb.AddForce(Vector3.up * jumpForce * 2.2f, ForceMode.Impulse);
                    isStand = false;
                    myCollider.sharedMaterial = noFriction;   
                    if(animator != null) animator.SetBool("isJumpAnimation", true);
                }
            }
        }
        if(collider.gameObject.tag == "haveJump")
        {
            if(roleNumber != 2)
            {
                LookDestinationPath(360f);
            }
            wishJump = true;
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

    void OnTriggerExit(Collider collider)
    {
        if(collider.gameObject.tag == "haveJump")
        {
            wishJump = false;
        }
    }

    void OnDrawGizmos()
    {
        if (agent == null || agent.path == null) return;

        var path = agent.path;

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
        }
    }
}
