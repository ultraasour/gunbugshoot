using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class firescript : MonoBehaviour
{
    public static firescript instance;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
    public float FireDown(int _shoottype, GameObject _player)
    {
        switch(_shoottype)
        {
            case 0:
                return RifleShoot(_player);
            case 1:
                return ShotgunShoot(_player);
            default:
                Debug.Log("no matching fire type for weapon specified was found");
                return 0f;
        }
    }
    public void FireUp()
    {
        
    }

    public float RifleShoot(GameObject _player)
    {
        ClientSend.Shoot();

        int _selectedweapon = _player.GetComponent<PlayerController>().selectedweapon;
        weapon _weapon = _player.GetComponent<PlayerController>().equiptweapons[_selectedweapon];
        GameObject firepoint = _player.GetComponent<PlayerController>().firepoint;

        Vector3 spread = firepoint.transform.forward;

        spread.x += Random.Range(-1f * _weapon.spread, _weapon.spread);
        spread.z += Random.Range(-1f * _weapon.spread, _weapon.spread);
        spread.Normalize();
        spread = new Vector3(spread.z, 0f, -spread.x);

        if (Physics.Raycast(firepoint.transform.position, spread, out RaycastHit raycasthit)) //shoot
        {
            if (raycasthit.collider.tag == "enemy")
            {
                int enemyid = raycasthit.collider.gameObject.GetComponent<enemyhealth>().id;

                ClientSend.EnemyHit(_weapon.damage, _weapon.AP, _weapon.STP, enemyid);
                raycasthit.collider.GetComponent<enemyhealth>().TakeDamage(_weapon.damage, _weapon.AP, _weapon.STP);
            }
        }

        return _weapon.firedelay;
    }

    public float ShotgunShoot(GameObject _player)
    {
        int _selectedweapon = _player.GetComponent<PlayerController>().selectedweapon;
        weapon _weapon = _player.GetComponent<PlayerController>().equiptweapons[_selectedweapon];
        GameObject firepoint = _player.GetComponent<PlayerController>().firepoint;

        for (int i = 1; i <= _weapon.shotcount; i++) //in the case of multiple shots, fire more than once
        {
            Vector3 spread = firepoint.transform.forward;

            spread.x += Random.Range(-1f * _weapon.spread, _weapon.spread);
            spread.z += Random.Range(-1f * _weapon.spread, _weapon.spread);
            spread.Normalize();
            spread = new Vector3(spread.z, 0f, -spread.x);

            if (Physics.Raycast(firepoint.transform.position, spread, out RaycastHit raycasthit)) //shoot
            {
                if (raycasthit.collider.tag == "enemy")
                {
                    int enemyid = raycasthit.collider.gameObject.GetComponent<enemyhealth>().id;

                    ClientSend.EnemyHit(_weapon.damage, _weapon.AP, _weapon.STP, enemyid);
                    raycasthit.collider.GetComponent<enemyhealth>().TakeDamage(_weapon.damage, _weapon.AP, _weapon.STP);
                }
            }
        }
        return _weapon.firedelay;
    }
}
