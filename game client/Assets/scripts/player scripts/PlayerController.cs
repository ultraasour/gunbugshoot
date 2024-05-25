using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region modifiers
    public float walkspeedmod; //modifies basic walking speed
    public float aimwalkmod; //modifies walking speed while aiming
    public float jogspeedmod; //modifies how much faster you move when jogging
    public float sprintspeedmod; //modifies how much faster you move when sprinting
    public float dashcooldownmod; //modifies how quickly your dash cools down
    public float dashpowermod; //modifies the speed and distance of your dash
    #endregion

    #region weapons
    public List<weapon> equiptweapons;
    public List<int> loadedammo = new List<int>();
    public List<int> ammopools = new List<int>();

    private bool reloading;
    public float reloadtimer;

    public int selectedweapon;

    private float firedelay;

    #endregion

    public Camera playercam;

    private float sprinttimer = 0f;
    private float momentumx = 0f;
    private float momentumz = 0f;
    private Vector3 nextposition;

    public Vector3 aimpoint;
    private LayerMask aimlayer;

    //movement collision/collide and slide
    private int maxbounces = 5;
    private float skinwidth = .015f;
    private LayerMask collisionlayer;
    Bounds bounds;

    public GameObject firepoint;

    public Camera camera_;

    private void Awake()
    {
        foreach(weapon _w in equiptweapons)
        {
            ammopools.Add(_w.magazine * _w.sparemags);
            loadedammo.Add(_w.magazine);
        }
        reloadtimer = -1f;

        selectedweapon = 0;

        playercam = Instantiate(camera_);
        playercam.GetComponent<CameraController>().UpdatePlayer(gameObject);
        playercam.GetComponent<CameraController>().UpdateSelf(playercam);
        
        bounds = this.gameObject.GetComponent<Collider>().bounds;
        bounds.Expand(-2 * skinwidth);

        aimlayer = 1 << 6; //set out look direction raycast equal to 6, the layer for our aim collider
    }

    void FixedUpdate()
    {
        SendPlayerTransform();
    }

    // Update is called once per frame
    void Update()
    {
        if(firedelay >= 0f)
        {
            firedelay -= Time.deltaTime;
        }
        if(reloadtimer >= 0f)
        {
            reloadtimer -= Time.deltaTime;
            if(reloadtimer <= 0f)
            {
                loadedammo[selectedweapon] += reloadscript.instance.ReloadFinish((int)equiptweapons[selectedweapon].reload, gameObject);
            }
        }
        // ------------- movement direction -------------

        momentumx = 0f;
        momentumz = 0f;
        nextposition = new Vector3(0f , 0f , 0f);

        if (Input.GetKey("w"))
        {
            momentumz += 1f;
        }
        if (Input.GetKey("s"))
        {
            momentumz -= 1f;
        }
        if (Input.GetKey("d"))
        {
            momentumx += 1f;
        }
        if (Input.GetKey("a"))
        {
            momentumx -= 1f ;

        }

        //get initial basic direction
        nextposition.z += momentumz; 
        nextposition.x += momentumx;

        //normalize direction
        nextposition.Normalize();
        nextposition *= Time.deltaTime;

        //check for jogging, don't start the sprint timer if player is standing still
        if (Input.GetKey("left shift") && (momentumx !=0 || momentumz !=0))
        {
            Facing(nextposition); //face in the direction of sprinting

            if(sprinttimer < 3) //jogging speed
            {
                nextposition *= 15f;
                sprinttimer += Time.deltaTime;
            }
            else //pop into sprinting after running for more than 3 seconds
            {
                nextposition *= 20f;
            }
        }
        else
        {
            sprinttimer = 0f;
            nextposition *= 10f;
            Facing();
            if(Input.GetButtonDown("reload"))
            {
                Reload();
            }
            else if(Input.GetButtonDown("Fire1"))
            {
                FireKeyDown();
                
            }
            else if(equiptweapons[selectedweapon].auto)
            {
                if(Input.GetButton("Fire1"))
                {
                    if(firedelay <= 0f)
                    {
                        FireKeyDown();
                    }
                }
            }
        }

        gameObject.transform.position += CollideAndSlide(nextposition, transform.position, 1);
    }

    private void Facing()
    {
        Ray look = playercam.ScreenPointToRay(Input.mousePosition); //ray cast from mouse/camera

        if (Physics.Raycast(look, out RaycastHit raycasthit, 999f, aimlayer)) //find where the raycast intersects with aim collider
        {
            aimpoint = raycasthit.point;
        }

        //face the direction of movement when running
        /*if (Input.GetKey("left shift") && (momentumx != 0 || momentumz != 0))
        {
            aimpoint = transform.position + nextposition;
        }*/

        Vector3 lookdir = aimpoint - transform.position; //find the direction by finding the difference of the aimpoint and character position
        transform.rotation = Quaternion.LookRotation(lookdir); //set our rotation so we face the aim point
        transform.rotation *= Quaternion.Euler(0f, -90f, 0f);

        return;
    }

    private void Facing(Vector3 _nextpos)
    {
        aimpoint = transform.position + _nextpos;

        Vector3 lookdir = aimpoint - transform.position; //find the direction by finding the difference of the aimpoint and character position
        transform.rotation = Quaternion.LookRotation(lookdir); //set our rotation so we face the aim point
        transform.rotation *= Quaternion.Euler(0f, -90f, 0f); //rotate 90 degress because this bitch is stupid

        return;
    }

    private Vector3 CollideAndSlide(Vector3 vel, Vector3 pos, int depth)
    {
        if(depth >= maxbounces)
        {
            return Vector3.zero; //dont go over max bounces
        }

        float dist = vel.magnitude + skinwidth;

        RaycastHit hit;
        if (Physics.SphereCast(pos, bounds.extents.x, vel.normalized, out hit, dist, collisionlayer))
        {
            Vector3 snaptosurface = vel.normalized * (hit.distance - skinwidth);
            Vector3 leftover = vel - snaptosurface;

            //normalization
            float mag = leftover.magnitude;
            leftover = Vector3.ProjectOnPlane(leftover, hit.normal).normalized;
            leftover *= mag;

            return snaptosurface + CollideAndSlide(leftover, pos + snaptosurface, depth + 1);
        }

        return vel;
    }

    private void SendPlayerTransform()
    {
        ClientSend.PlayerTransform(transform.position, transform.rotation);
    }

    private void FireKeyDown()
    {
        if(firedelay <= 0f && loadedammo[selectedweapon] > 0)
        {
            reloadtimer = -.1f;
            loadedammo[selectedweapon] -= 1;
            firedelay = firescript.instance.FireDown((int)equiptweapons[selectedweapon].fire, gameObject);
        }
    }
    public void Reload()
    {
        if(ammopools[selectedweapon] > 0 && loadedammo[selectedweapon] < equiptweapons[selectedweapon].magazine)
        {
            reloadtimer = reloadscript.instance.ReloadStart((int)equiptweapons[selectedweapon].reload, gameObject);
        }
    }
}
