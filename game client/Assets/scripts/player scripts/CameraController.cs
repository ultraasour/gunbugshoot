using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float viewmod; //modifies the view distance of the camera

    public Camera self;
    public GameObject player;
    private Vector3 nextposition;

    // Start is called before the first frame update
    void Awake()
    {
        viewmod = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(player != null)
        {
            nextposition = player.transform.position; //above player
            nextposition.y += (100f * (1f + viewmod)); //move camera up

            self.transform.position = nextposition;
        }
    }

    public void UpdatePlayer(GameObject _player)
    {
        player = _player;
    }

    public void UpdateSelf(Camera _self)
    {
        self = _self;
    }
}
