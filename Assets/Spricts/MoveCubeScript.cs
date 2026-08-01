using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCubeScript : MonoBehaviour
{
    [SerializeField] float movingLimit;
    [SerializeField] float speed;
    [SerializeField] int XorYorZ;
    float middle;
    bool moveDirection;
    // Start is called before the first frame update
    void Start()
    {
        moveDirection = false;
        if(XorYorZ == 0) middle = transform.position.x;
        if(XorYorZ == 1) middle = transform.position.y;
        if(XorYorZ == 2) middle = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        if(XorYorZ == 0)
        {
            if(transform.position.x > middle + (movingLimit / 2))
            {
                moveDirection = true;
            }   
            else if(transform.position.x < middle - (movingLimit / 2))
            {
                moveDirection = false;
            } 
            if(!moveDirection)
            {
                transform.position += new Vector3(speed * Time.deltaTime, 0, 0);
            }
            if(moveDirection)
            {
                transform.position -= new Vector3(speed * Time.deltaTime, 0, 0);
            }
        }
    }
}
