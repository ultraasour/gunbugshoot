using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AImanager : MonoBehaviour
{
    public static AImanager instance;
    //  list of all enemy prefabs, their models and AI are there as well
    public GameObject[] enemyprefabs;
    //  list of all enemies
    List<GameObject> enemylist = new List<GameObject>();
    //  these values are kept on the enemy objects themselves, this list is an artifact, it might be removeable later
    List<Vector3> nextpositionlist = new List<Vector3>(); 
    List<float> nextrotationlist = new List<float>();

    private float timer; //debugging (hah) spawn timer
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object");
            Destroy(this);
        }
    }

    public void Update()
    {
        timer += Time.deltaTime; //once again for debugging
    }

    //  spawns an enemy at a point, instantiates all values within a list
    public void Spawn(int _id, int _type, Vector3 _location, float _health)
    {
        GameObject _enemy;
        _enemy = Instantiate(enemyprefabs[_type], _location, Quaternion.identity);
        _enemy.GetComponent<enemyhealth>().currenthealth = _health;
        _enemy.GetComponent<enemyhealth>().id = _id;
        enemylist.Add(_enemy);
        nextpositionlist.Add(Vector3.zero);
        nextrotationlist.Add(0f);
    }

    //  Iterates through enemies updating the list and more importantly calls UpdateMovement() on the enemy script
    public void UpdateEnemyTransformList(List<Vector3> _positions, int _start, int _end, int _iteration)
    {
        //fall through only used if there are fewer than 5 enemies
        if(_end < 5)
        {
            for(int i = 0; i < _end && i < enemylist.Count; i++)
            {
                nextpositionlist[i] = _positions[i];

                enemylist[i].GetComponent<AI_horde>().UpdateMovement(_positions[i]);
            }
        }
        else //general case
        {
            for(int i = _start; i < _end && i < enemylist.Count; i++)
            {
                nextpositionlist[i] = _positions[(i - _start)];

                enemylist[i].GetComponent<AI_horde>().UpdateMovement(_positions[(i - _start)]);
            }
        }
    }
    // same as above but for rotations. tracked as floats since they only actually rotate around the y axis
    public void UpdateEnemyRotationList(List<float> _rotations, int _start, int _end)
    {
        //fall through only used if there are fewer than 5 enemies
        if(_end < 5)
        {
            for(int i = 0; i < _end && i < enemylist.Count; i++)
            {
                nextrotationlist[i] = _rotations[i];

                enemylist[i].GetComponent<AI_horde>().desiredrotation = _rotations[i];
            }
        }
        else //general case
        {
            for(int i = _start; i < _end && i < enemylist.Count; i++)
            {
                nextrotationlist[i] = _rotations[(i - _start)];

                enemylist[i].GetComponent<AI_horde>().desiredrotation = _rotations[(i - _start)];
            }
        }
    }

    //  destroys an enemy and all related stuff, do not destroy things outside of this function
    public void DestroyEnemy(int _id, int _index)
    {
        if(enemylist[_index].GetComponent<enemyhealth>().id == _id)
        {
            GameObject _temp = enemylist[_index];
                
            enemylist.RemoveAt(_index);
            nextpositionlist.RemoveAt(_index);
            nextrotationlist.RemoveAt(_index);

            Destroy(_temp);
        }
        else //if the given index and ID do not match across the client and server
        {
            Debug.Log("mismatched ID between client and server when trying to destroy object");
            _index = FindIDInEnemies(_id);

            GameObject _temp = enemylist[_index];
                
            enemylist.RemoveAt(_index);
            nextpositionlist.RemoveAt(_index);
            nextrotationlist.RemoveAt(_index);

            Destroy(_temp);
        }
    }
    public int FindIDInEnemies(int _id)
    {
        for(int i = 0; i < enemylist.Count; i++)
        {
            if(enemylist[i].GetComponent<enemyhealth>().id == _id)
            {
                return i;
            }
        }
        FixEnemyList();
        return _id;
    }
    public void FixEnemyList()
    {
        //TODO call the initial spawn enemies again like when spawning 
        //into the game and delete all the old stuff
    }

    public void UpdateHealth(int _id, float _newhealth)
    {
        if(_newhealth <= 0f)
        {
            enemylist[FindIDInEnemies(_id)].GetComponent<enemyhealth>().Die();
        }
        else
        {
            enemylist[FindIDInEnemies(_id)].GetComponent<enemyhealth>().currenthealth = _newhealth;
        }
    }
}
