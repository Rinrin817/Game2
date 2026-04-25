using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class randomTransformSprict : MonoBehaviour
{
    public float speed;
    public float range; // 限界値（50）
    
    private Vector3 moveDirection;
    private float changeTimer;

    void Start()
    {
        // 最初にランダムな方向を決める
        SetRandomDirection();
        speed = 20f;
    }

    void Update()
    {
        // 1. 座標をチェックして、範囲を超えていたら反射させる
        CheckBounds();

        // 2. 移動実行
        transform.position += moveDirection * speed * Time.deltaTime;
        // 3. 一定時間経ったらたまに方向を変える（ずっと同じ方向だと退屈なため）
        changeTimer += Time.deltaTime;
        if (changeTimer > 1f)
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
        Quaternion rotation = Quaternion.Euler(0, Random.Range(-30f, 30f), 0);
        moveDirection = (rotation * baseDir).normalized;
    }
    
    void SetRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        Vector3 vector3 = transform.position + new Vector3(0,  Random.Range(-10f, 10f), 0);
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

        if (pos.y > range)
        {
            pos.y -= 50;
        }
        else if (pos.z < -range)
        {
            pos.y += 50;
        }

        transform.position = pos;
    }
}
