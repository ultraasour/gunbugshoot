using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_horde : enemyhealth
{
    //  script works by lerping between the most recently sent position and the last sent position and rotating towards our desired rotation
    private Vector3 nextposition;
    private Vector3 lastposition;
    public float desiredrotation;

    //  helps keep track of how far we are in the process of lerping between positions, may desync from server a bit but shouldn't be too big an issue
    private int moveiterations; 

    public bool mc; //used for debugging, when clicked it will return information about the specific enemy

    void Awake()
    {
       mc = false;
    }

    void FixedUpdate()
    {
        //rotate towards our desired rotation
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0f, desiredrotation, 0f), 20f);

        //lerp between positions
        transform.position = Vector3.Lerp(lastposition, nextposition, moveiterations * .2f);

        moveiterations += 1;
    }

    //  called by the AI manager when it receives new positions, only a fifth of the enemeis will receive this each call due to rotating updates,
    //  which is what the lerping is for
    public void UpdateMovement(Vector3 _nextpos)
    {
        moveiterations = 0;
        lastposition = nextposition;
        nextposition = _nextpos;
    }

    public override void Die()
    {
        transform.position = new Vector3(0f, 200f, 0f);
        nextposition = transform.position;
        lastposition = transform.position;

        currenthealth = 0f;

        active = false;
    }
}
