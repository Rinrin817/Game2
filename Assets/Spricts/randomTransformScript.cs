using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class randomTransformScript : MonoBehaviour
{
    public float speed;
    public float range; // 限界値（50）
    
    private Vector3 moveDirection;
    private float changeTimer;
    Vector3 vector3;

    void Start()
    {
        SetRandomDirection();
    }

    void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
        CheckBounds();
        changeTimer += Time.deltaTime;
        if (changeTimer > 0.5f)
        {
            SetRotateDirection();
            changeTimer = 0;
        }
        if (changeTimer > 5f)
        {
            SetRandomDirection();
            changeTimer = 0;
        }
    }

    void SetRotateDirection()
    {
        // もし動いていないなら、向いている正面を基準にする
        Vector3 baseDir = moveDirection == Vector3.zero ? transform.forward : moveDirection;
        Quaternion rotation = Quaternion.Euler(0, Random.Range(-0.1f, 0.1f), 0);
        moveDirection = (rotation * baseDir).normalized;
    }
    
    void SetRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        vector3 = new Vector3(0,  Random.Range(-2f, 2f), 0);
        moveDirection = new Vector3(Mathf.Sin(angle) * 2f, vector3.y, Mathf.Cos(angle) * 2f).normalized;
    }

    void CheckBounds()
    {
        Vector3 pos = transform.position;

        // X軸の判定
        if (pos.x > range)
        {
            pos.x = range;
            moveDirection.x = -Mathf.Abs(moveDirection.x); // 強制的に左（マイナス）へ
        }
        else if (pos.x < -range)
        {
            pos.x = -range;
            moveDirection.x = Mathf.Abs(moveDirection.x);  // 強制的に右（プラス）へ
        }

        // Z軸の判定
        if (pos.z > range)
        {
            pos.z = range;
            moveDirection.z = -Mathf.Abs(moveDirection.z); // 強制的に手前（マイナス）へ
        }
        else if (pos.z < -range)
        {
            pos.z = -range;
            moveDirection.z = Mathf.Abs(moveDirection.z);  // 強制的に奥（プラス）へ
        }

        if (pos.y > range / 2)
        {
            pos.y = range / 2;
            moveDirection.y = -Mathf.Abs(moveDirection.y);
        }
        else if (pos.y < -5)
        {
            pos.y = -5;
            moveDirection.y = Mathf.Abs(moveDirection.y);
        }

        transform.position = pos;
    }
}
