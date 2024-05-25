using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class reloadscript : MonoBehaviour
{
    public static reloadscript instance;

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
    public float ReloadStart(int _reloadtype, GameObject _player)
    {
        switch(_reloadtype)
        {
            case 0:
                return MagazineStart(_player);
            case 1:
                return RoundReloadStart(_player);
            default:
                Debug.Log("no matching reload type for weapon specified was found");
                return 0f;
        }
    }
    public int ReloadFinish(int _reloadtype, GameObject _player)
    {
        switch(_reloadtype)
        {
            case 0:
                return MagazineFinish(_player);
            case 1:
                return RoundsFinish(_player);
            default:
                Debug.Log("no matching reload type for weapon specified was found");
                return 0;
        }
    }

    //  used for any weapon which reloads the entire magazine at once
    public float MagazineStart(GameObject _player)
    {
        int _selectedweapon = _player.GetComponent<PlayerController>().selectedweapon;
        weapon _weapon = _player.GetComponent<PlayerController>().equiptweapons[_selectedweapon];

        //add the ammo in the magazine to the ammo pool and then set the loaded ammo to 0
        _player.GetComponent<PlayerController>().ammopools[_selectedweapon] += _player.GetComponent<PlayerController>().loadedammo[_selectedweapon];
        _player.GetComponent<PlayerController>().loadedammo[_selectedweapon] = 0;

        return _weapon.reloadtime;
    }
    public int MagazineFinish(GameObject _player)
    {
        int _selectedweapon = _player.GetComponent<PlayerController>().selectedweapon;
        weapon _weapon = _player.GetComponent<PlayerController>().equiptweapons[_selectedweapon];

        //check if theres enough ammo to load the gun or not
        if(_player.GetComponent<PlayerController>().ammopools[_selectedweapon] >= _weapon.magazine)
        {
            _player.GetComponent<PlayerController>().ammopools[_selectedweapon] -=_weapon.magazine;
            return _weapon.magazine;
        }
        else
        {
            int _reload = _player.GetComponent<PlayerController>().ammopools[_selectedweapon];
            _player.GetComponent<PlayerController>().ammopools[_selectedweapon] = 0;
            return _reload;
        }
    }

    //  used for any weapon which can or does reload one round at a time
    public float RoundReloadStart(GameObject _player)
    {
        int _selectedweapon = _player.GetComponent<PlayerController>().selectedweapon;
        weapon _weapon = _player.GetComponent<PlayerController>().equiptweapons[_selectedweapon];

        return _weapon.reloadtime;
    }
    
    public int RoundsFinish(GameObject _player)
    {
        _player.GetComponent<PlayerController>().Reload();
        return 1;
    }
}
