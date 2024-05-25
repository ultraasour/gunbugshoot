using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int databuffersize = 4096;

    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myID = 0;
    public TCP tcp;
    public UDP udp;

    private bool isconnected = false;

    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packethandlers;

    private void Awake()
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

    public void SetIP(string _ip)
    {
        ip = _ip;
        return;
    }
    private void Start()
    {
        tcp = new TCP();
        udp = new UDP();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void ConnectToServer()
    {
        InitializeClientData();

        isconnected = true;
        tcp.Connect();
    }

    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private Packet receivedata;
        private byte[] receivebuffer;

        public void Connect()
        {
            socket = new TcpClient
            {
                //editted this code because his did not work
                ReceiveBufferSize = databuffersize,
                SendBufferSize = databuffersize
            };

            receivebuffer = new byte[databuffersize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult _result)
        {
            socket.EndConnect(_result);

            if(!socket.Connected)
            {
                return;
            }

            stream = socket.GetStream();

            receivedata = new Packet();

            stream.BeginRead(receivebuffer, 0, databuffersize, ReceiveCallback, null);
        }
        
        public void SendData(Packet _packet)
        {
            try 
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch(Exception _ex)
            {
                Debug.Log($"Error sending data to server via TCP: {_ex}");
            }
        }


        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _bytelength = stream.EndRead(_result);
                if (_bytelength <= 0)
                {
                    instance.Disconnect();
                    return;
                }

                byte[] _data = new byte[_bytelength];
                Array.Copy(receivebuffer, _data, _bytelength);

                receivedata.Reset(HandleData(_data));
                stream.BeginRead(receivebuffer, 0, databuffersize, ReceiveCallback, null);
            }
            catch
            {
                Disconnect();
            }
        }

        private bool HandleData(byte[] _data)
        {
            int _packetlength = 0;

            receivedata.SetBytes(_data);

            if (receivedata.UnreadLength() >= 4)
            {
                _packetlength = receivedata.ReadInt();
                if (_packetlength <= 0)
                {
                    return true;
                }
            }

            while (_packetlength > 0 && _packetlength <= receivedata.UnreadLength())
            {
                byte[] _packetbytes = receivedata.ReadBytes(_packetlength);
                ThreadManager.ExecuteOnMainThread(() => 
                {
                    using (Packet _packet = new Packet(_packetbytes))
                    {
                        int _packetid = _packet.ReadInt();
                        packethandlers[_packetid](_packet);
                    }
                });

                _packetlength = 0;
                if (receivedata.UnreadLength() >= 4)
                {
                    _packetlength = receivedata.ReadInt();
                    if (_packetlength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (_packetlength <= 1)
            {
                return true;
            }

            return false;
        }
    
        private void Disconnect()
        {
            instance.Disconnect();

            stream = null;
            receivedata = null;
            receivebuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endpoint;

        public UDP()
        {
            endpoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        public void Connect(int _localport)
        {
            socket = new UdpClient(_localport);

            socket.Connect(endpoint);
            socket.BeginReceive(ReceiveCallBack, null);

            using (Packet _packet = new Packet())
            {
                SendData(_packet);
            }
        }
        
        public void SendData(Packet _packet)
        {
            try
            {
                _packet.InsertInt(instance.myID);
                if(socket != null)
                {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }
            }
            catch(Exception _ex)
            {
                Debug.Log($"Error sending data to server via UDP: {_ex}");
            }
        }

        private void ReceiveCallBack(IAsyncResult _result)
        {
            try
            {
                byte[] _data = socket.EndReceive(_result, ref endpoint);
                socket.BeginReceive(ReceiveCallBack, null);

                if(_data.Length < 4)
                {
                    instance.Disconnect();
                    return;
                }

                HandleData(_data);
            }
            catch
            {
                Disconnect();
            }
        }

        private void HandleData(byte[] _data)
        {
            using(Packet _packet = new Packet(_data))
            {
                int _packetlength = _packet.ReadInt();
                _data = _packet.ReadBytes(_packetlength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using(Packet _packet = new Packet(_data))
                {
                    int _packetid = _packet.ReadInt();
                    packethandlers[_packetid](_packet);
                }
            });
        }
   
        private void Disconnect()
        {
            instance.Disconnect();

            endpoint = null;
            socket = null;
        }
    }

    private void InitializeClientData()
    {
        packethandlers = new Dictionary<int, PacketHandler>()
        {
            {(int)ServerPackets.welcome, clienthandle.Welcome},
            {(int)ServerPackets.SpawnPlayer, clienthandle.SpawnPlayer},
            {(int)ServerPackets.playerposition, clienthandle.PlayerPosition},
            {(int)ServerPackets.playerrotation, clienthandle.PlayerRotation},
            {(int)ServerPackets.playerdisconnected, clienthandle.PlayerDisconnected},
            {(int)ServerPackets.spawnenemy, clienthandle.SpawnEnemy},
            {(int)ServerPackets.enemylocations, clienthandle.EnemyLocations},
            {(int)ServerPackets.enemyrotations, clienthandle.EnemyRotations},
            {(int)ServerPackets.destroyenemy, clienthandle.DestroyEnemy},
            {(int)ServerPackets.updateenemyhealth, clienthandle.UpdateEnemyHealth}
        };
        Debug.Log("Initialized packets");
    }

    private void Disconnect()
    {
        if(isconnected)
        {
            isconnected = false;
            tcp.socket.Close();
            udp.socket.Close();

            Debug.Log("Disconnected from server.");
        }
    }
}
