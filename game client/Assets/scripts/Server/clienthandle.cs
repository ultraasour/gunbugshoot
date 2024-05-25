using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class clienthandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        //TCP
        string _msg = _packet.ReadString();
        int _myid = _packet.ReadInt();
        
        Debug.Log($"Message from server: {_msg}");
        Client.instance.myID = _myid;
        ClientSend.WelcomeReceived();

        //UDP
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation);
    }

    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        GameManager.players[_id].transform.position = _position;
    }

    public static void PlayerRotation(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        GameManager.players[_id].transform.rotation = _rotation;
    }

    public static void PlayerDisconnected(Packet _packet)
    {
        int _id = _packet.ReadInt();

        Destroy(GameManager.players[_id].gameObject);
        GameManager.players.Remove(_id);
    }

    public static void SpawnEnemy(Packet _packet)
    {
        int _id = _packet.ReadInt();
        int _type = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        float _health = _packet.ReadFloat();

        AImanager.instance.Spawn(_id, _type, _position, _health);
    }

    public static void EnemyLocations(Packet _packet)
    {
        List<Vector3> _nextposlist = new List<Vector3>();
        int _length = _packet.ReadInt();

        for(int i = 0; i < _length; i++)
        {
            _nextposlist.Add(_packet.ReadVector3());
        }

        int _start = _packet.ReadInt();
        int _end = _packet.ReadInt();
        int _iteration = _packet.ReadInt();

        AImanager.instance.UpdateEnemyTransformList(_nextposlist, _start, _end, _iteration);
    }
    public static void EnemyRotations(Packet _packet)
    {
        List<float> _nextrotlist = new List<float>();
        int _length = _packet.ReadInt();

        for(int i = 0; i < _length; i++)
        {
            _nextrotlist.Add(_packet.ReadFloat());
        }

        int _start = _packet.ReadInt();
        int _end = _packet.ReadInt();

        AImanager.instance.UpdateEnemyRotationList(_nextrotlist, _start, _end);
    }

    public static void DestroyEnemy(Packet _packet)
    {
        int _id = _packet.ReadInt();
        int _index = _packet.ReadInt();

        AImanager.instance.DestroyEnemy(_id, _index);
    }

    public static void UpdateEnemyHealth(Packet _packet)
    {
        int _id = _packet.ReadInt();
        float _health = _packet.ReadFloat();

        AImanager.instance.UpdateHealth(_id, _health);
    }
}
