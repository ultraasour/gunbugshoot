using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    #region Packets
    public static void WelcomeReceived()
    {
        using(Packet _packet = new Packet((int)ClientPackets.welcomereceived))
        {
            _packet.Write(Client.instance.myID);
            _packet.Write(UImanager.instance.usernamefield.text);

            SendTCPData(_packet);
        }
    }

    public static void PlayerTransform(Vector3 _position, Quaternion _rotation)
    {
        using(Packet _packet = new Packet((int)ClientPackets.playermovement))
        {
            _packet.Write(_position);
            _packet.Write(_rotation);

            SendUDPData(_packet);
        }
    }

    public static void Shoot()
    {
        using(Packet _packet = new Packet((int)ClientPackets.shoot))
        {
            _packet.Write(Client.instance.myID);

            SendUDPData(_packet);
        }
    }

    public static void EnemyHit(float _damage, int _AP, int _STP, int _id)
    {
        using(Packet _packet = new Packet((int)ClientPackets.enemyhit))
        {
            _packet.Write(_damage);
            _packet.Write(_AP);
            _packet.Write(_STP);
            _packet.Write(_id);

            SendTCPData(_packet);
        }
    }
    #endregion 
}
