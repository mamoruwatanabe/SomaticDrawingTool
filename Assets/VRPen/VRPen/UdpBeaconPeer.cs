using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace VRPen.VRPenPro
{
    public class UdpBeaconPeer
    {
        public string Name;

        public bool IsOpen;
        public bool IsHub;

        private UdpClient _udpClient;
        //private Task<UdpReceiveResult> _udpReceive;
        //private Task<int>              _udpSend;

        private ByteBuffer _udpSendBuffer = new ByteBuffer();
        private ByteBuffer _udpReadBuffer = new ByteBuffer();

        private List<IPEndPoint> _udpBroadcastIps;
        private string           _cookie = "UpdBeacon2";
        private string           _msg;
        private float            _lastAnnounceSent;
        private ushort           _portHash;
        private byte[]           _guid;

        public  Dictionary<Guid, Peer> _availableClients = new();
        private Queue<IPEndPoint>      _toConnectQueue   = new();

        public float HeartbeatTimeout = 2;
        public float AnnounceTimeout  = 4;

        enum Id : byte
        {
            Announce,
            Connect,
            HubPeers,
            Data,
            DropMe,
            Heartbeat
        }

        public bool EnableDebug;

        public void Open(Guid guid)
        {
            _origGuid = guid;

            _guid     = guid.ToByteArray();
            _portHash = GetBasePort(_cookie);

            //_name = SystemInfo.deviceModel + " " + _guid[0];

            string base64Guid = _origGuid.ToString().Substring(0, 5);
            _fullName = Name + "_" + base64Guid;

            var baseport = GetPort();

            for (int i = 0; i < 4; i++)
            {
                try
                {
                    var localIp = new IPEndPoint(IPAddress.Any, baseport + i);
                    _udpClient = new UdpClient(localIp);

                    LocalIp = localIp;
                    break;
                }
                catch
                {
                }
            }

            _udpBroadcastIps = new List<IPEndPoint>();
            for (int i = 0; i < 4; i++)
            {
                _udpBroadcastIps.Add(new IPEndPoint(IPAddress.Broadcast, baseport + i));
            }

            _udpClient.EnableBroadcast   = true;
            _udpClient.MulticastLoopback = false;

            //_udpReceive = _udpClient.ReceiveAsync();

            if (IsOpen)
            {
                foreach (var point in _udpBroadcastIps)
                {
                    Announce(point);
                }
            }

            _isReady = true;

            var receive = new Thread(UdpReceiveLoop);
            receive.Start();
        }

        public IPEndPoint LocalIp { get; set; }

        public ConcurrentQueue<UdpResult> _receiveQueue = new();

        public struct UdpResult
        {
            public byte[]     Bytes;
            public IPEndPoint EndPoint;
        }

        private void UdpReceiveLoop()
        {
            while (_isReady)
            {
                var result = new UdpResult();
                result.Bytes = _udpClient.Receive(ref result.EndPoint);
                _receiveQueue.Enqueue(result);
            }
        }

        public void Update()
        {
            if (!_isReady) return;

            if (_toConnectQueue.TryDequeue(out var ipEndPoint))
            {
                Connect(ipEndPoint);
            }

            while (_receiveQueue.TryDequeue(out var result))
            {
                Receive(result.EndPoint, result.Bytes);
            }

            foreach (var client in _availableClients.Values.ToList())
            {
                var announceTimeout  = Time.time - client._lastAnnounceReceived > _announceTime + AnnounceTimeout;
                var heartbeatTimeout = Time.time - client._lastHBReceived > _heartbeatTime + HeartbeatTimeout;

                if (client.IsOpen && announceTimeout && heartbeatTimeout)
                {
                    PeerLeave?.Invoke(client);

                    _availableClients.Remove(client.Guid);
                    if(EnableDebug) Debug.Log(
                        $"peer {client.Name} removed. No announce for {Time.time - client._lastAnnounceReceived}.");
                }

                if (client.IsConnected && heartbeatTimeout)
                {
                    PeerLeave?.Invoke(client);
                    client.IsConnected = false;

                    if(EnableDebug) Debug.Log(
                        $"peer {client.Name} disconnected. No heartbeat for {Time.time - client._lastHBReceived}");

                    if (!client.IsOpen)
                    {
                        _availableClients.Remove(client.Guid);
                        if(EnableDebug) Debug.Log($"peer {client.Name} removed. Peer is not open.");
                    }
                }

                if (client.IsConnected && Time.time - client._lastHBSent >= _heartbeatTime)
                {
                    if (IsOpen || client.IsConnected)
                    {
                        SendHeartbeat(client);
                    }
                }
            }

            if (IsOpen && Time.time - _lastAnnounceSent > _announceTime)
            {
                _lastAnnounceSent = Time.time;
                foreach (var point in _udpBroadcastIps)
                {
                    Announce(point);
                }
            }
        }

        private float _announceTime  = 1f;
        private float _heartbeatTime = 0.45f;

        private void Receive(IPEndPoint endPoint, byte[] buffer)
        {
            //var endPoint = _udpReceive.Result.RemoteEndPoint;
            //_udpReadBuffer.CopyFrom(_udpReceive.Result.Buffer);
            _udpReadBuffer.CopyFrom(buffer);

            var id        = (Id)_udpReadBuffer.ReadByte();
            var guidBytes = new byte[16];
            _udpReadBuffer.ReadBytes(guidBytes, 0, guidBytes.Length);
            var theirGuid = new Guid(guidBytes);

            if (ByteArraysEqual(guidBytes, _guid)) return;

            if (id == Id.Announce || id == Id.Connect || id == Id.HubPeers)
            {
                var cookie = _udpReadBuffer.ReadString();
                if (cookie != _cookie) return;
            }

            if(EnableDebug) Debug.Log($"{id} {Time.time}");


            //Consider any data as a heartbeat
            {
                if (id != Id.DropMe && _availableClients.TryGetValue(theirGuid, out var client))
                {
                    if (client.IsOpen)
                    {
                        client._lastAnnounceReceived = Time.time;
                    }

                    if (id != Id.Announce && client.IsConnected)
                    {
                        client._lastHBReceived = Time.time;
                    }
                }
            }

            switch (id)
            {
                case Id.Heartbeat:
                {
                    if (_availableClients.TryGetValue(theirGuid, out var client))
                    {
                        client._lastHBReceived = Time.time;
                    }
                }
                    break;
                case Id.DropMe:
                {
                    if (_availableClients.TryGetValue(theirGuid, out var client))
                    {
                        if (client.IsConnected)
                        {
                            SendDropMe(client);
                            client.IsConnected = false;
                        }

                        _availableClients.Remove(theirGuid);
                        PeerLeave?.Invoke(client);
                        if(EnableDebug) Debug.Log("Dropped");
                    }
                }
                    break;
                case Id.Announce:
                {
                    if (!_availableClients.TryGetValue(theirGuid, out var peer))
                    {
                        var from = _udpReadBuffer.ReadString();

                        peer = new Peer()
                        {
                            Guid     = theirGuid,
                            Name     = from,
                            Endpoint = endPoint,
                            IsOpen   = true
                        };

                        peer._lastHBReceived = Time.time;
                        _availableClients.Add(theirGuid, peer);
                        PeerAnnounced?.Invoke(peer);
                    }

                    peer._lastAnnounceReceived = Time.time;
                }
                    break;

                case Id.HubPeers:
                {
                    if (_availableClients.TryGetValue(theirGuid, out var client))
                    {
                        var from = _udpReadBuffer.ReadString();

                        int count = _udpReadBuffer.ReadInt();
                        for (int i = 0; i < count; i++)
                        {
                            var bytes = _udpReadBuffer.ReadBytesWithCount();
                            var port  = _udpReadBuffer.ReadInt();
                            var ip    = new IPAddress(bytes);
                            var iep   = new IPEndPoint(ip, port);

                            _toConnectQueue.Enqueue(iep);
                        }
                    }
                }
                    break;
                case Id.Connect:
                {
                    if (!_availableClients.TryGetValue(theirGuid, out var client))
                    {
                        var from   = _udpReadBuffer.ReadString();
                        var isOpen = _udpReadBuffer.ReadBool();

                        client = new Peer()
                        {
                            Guid     = theirGuid,
                            Name     = from,
                            Endpoint = endPoint,
                            IsOpen   = isOpen
                        };

                        client._lastHBReceived       = Time.time;
                        client._lastAnnounceReceived = Time.time;
                        _availableClients.Add(theirGuid, client);
                        PeerAnnounced?.Invoke(client);
                    }

                    if (!client.IsConnected)
                    {
                        client.IsConnected     = true;
                        client._lastHBReceived = Time.time;
                        PeerAdded?.Invoke(client);
                        Connect(endPoint);

                        if (IsHub)
                        {
                            foreach (var peer in _availableClients.Values)
                            {
                                if (peer.IsConnected)
                                {
                                    SendHubPeers(client);
                                }
                            }
                        }
                    }
                }
                    break;
                case Id.Data:
                {
                    if (_availableClients.TryGetValue(theirGuid, out var client) && client.IsConnected)
                    {
                        //client._lastHB = Time.time;
                        DataFromPeer?.Invoke(_udpReadBuffer, client);
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SendHubPeers(Peer client)
        {
            _udpSendBuffer.Clear();
            _udpSendBuffer.WriteByte((byte)Id.HubPeers);
            _udpSendBuffer.WriteBytes(_guid, 0, _guid.Length);
            _udpSendBuffer.WriteString(_cookie);
            _udpSendBuffer.WriteString(_fullName);

            var connected = _availableClients.Values.Where(p => p.IsConnected).ToArray();
            _udpSendBuffer.WriteInt(connected.Length);
            foreach (var peer in connected)
            {
                var bytes = peer.Endpoint.Address.GetAddressBytes();
                _udpSendBuffer.WriteBytesWithCount(bytes);
                _udpSendBuffer.WriteInt(peer.Endpoint.Port);
            }

            _udpClient.Send(_udpSendBuffer.Array, _udpSendBuffer.posWrite, client.Endpoint);
        }

        public void Disconnect()
        {
            if (_availableClients.Count > 0)
            {
                foreach (var value in _availableClients.Values)
                {
                    if (value.IsConnected) SendDropMe(value);
                }

                _availableClients.Clear();
                Disconnected?.Invoke();
            }

            if(EnableDebug) Debug.Log($"Disconnected. Peers cleared.");

            //IsOpen = false;
        }

        public  Action                   Disconnected;
        public  Action<Peer>             PeerAnnounced;
        public  Action<Peer>             PeerAdded;
        public  Action<Peer>             PeerLeave;
        public  Action<ByteBuffer, Peer> DataFromPeer;
        public  Guid                     _origGuid;
        private bool                     _isReady;
        private string                   _fullName;

        public void Close()
        {
            if (_udpClient != null)
            {
                _udpClient.Close();
                _udpClient.Dispose();
            }

            _isReady = false;
        }


        private void Announce(IPEndPoint endPoint)
        {
            _udpSendBuffer.Clear();
            _udpSendBuffer.WriteByte((byte)Id.Announce);
            _udpSendBuffer.WriteBytes(_guid, 0, _guid.Length);
            _udpSendBuffer.WriteString(_cookie);
            _udpSendBuffer.WriteString(_fullName);

            _udpClient.Send(_udpSendBuffer.Array, _udpSendBuffer.posWrite, endPoint);
        }

        private void Connect(IPEndPoint endPoint)
        {
            _udpSendBuffer.Clear();
            _udpSendBuffer.WriteByte((byte)Id.Connect);
            _udpSendBuffer.WriteBytes(_guid, 0, _guid.Length);
            _udpSendBuffer.WriteString(_cookie);
            _udpSendBuffer.WriteString(_fullName);
            _udpSendBuffer.WriteBool(IsOpen);

            _udpClient.Send(_udpSendBuffer.Array, _udpSendBuffer.posWrite, endPoint);
        }

        private void SendDropMe(Peer toPeer)
        {
            toPeer._lastHBSent = Time.time;
            _udpSendBuffer.Clear();
            _udpSendBuffer.WriteByte((byte)Id.DropMe);
            _udpSendBuffer.WriteBytes(_guid, 0, _guid.Length);

            _udpClient.Send(_udpSendBuffer.Array, _udpSendBuffer.posWrite, toPeer.Endpoint);
        }

        private void SendHeartbeat(Peer toPeer)
        {
            toPeer._lastHBSent = Time.time;
            _udpSendBuffer.Clear();
            _udpSendBuffer.WriteByte((byte)Id.Heartbeat);
            _udpSendBuffer.WriteBytes(_guid, 0, _guid.Length);

            _udpClient.Send(_udpSendBuffer.Array, _udpSendBuffer.posWrite, toPeer.Endpoint);
        }

        public void SendToClient(Peer client, Action<ByteBuffer> msg)
        {
            _udpSendBuffer.Clear();
            _udpSendBuffer.WriteByte((byte)Id.Data);
            _udpSendBuffer.WriteBytes(_guid, 0, _guid.Length);
            msg.Invoke(_udpSendBuffer);

            _udpClient.Send(_udpSendBuffer.Array, _udpSendBuffer.posWrite, client.Endpoint);
        }

        private int GetPort()
        {
            return _portHash;
        }

        private static ushort GetBasePort(string str)
        {
            unchecked
            {
                int hash1 = 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                int hash = hash1 + (hash2 * 1566083941);

                ushort hash16 = (ushort)((hash >> 16) ^ hash);

                if (hash16 < 1024) hash16 += 1024;
                return hash16;
            }
        }

        static bool ByteArraysEqual(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
        {
            return a1.SequenceEqual(a2);
        }

        public void ConnectToPeer(Peer server)
        {
            Connect(server.Endpoint);
        }

        public class Peer
        {
            public float _lastAnnounceReceived;
            public float _lastHBReceived;
            public float _lastHBSent;

            public Guid       Guid        { get; set; }
            public string     Name        { get; set; }
            public IPEndPoint Endpoint    { get; set; }
            public bool       IsConnected { get; set; }
            public bool       IsOpen      { get; set; }
            public object     UserData    { get; set; }
        }
    }
}