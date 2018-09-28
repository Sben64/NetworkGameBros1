using System;
using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using Microsoft.Xna.Framework.Graphics;
using NetworkGameLibrary;

namespace NetworkGame
{
    class ManagerNetwork
    {
        private NetClient _client;
        public Player Player { get; set; }
        public List<Player> OtherPlayers { get; set; }
        public static bool isConnected = false;
        public bool Active { get; set; }
        public ManagerNetwork()
        {
            OtherPlayers = new List<Player>();
        }

        public bool Start()
        {
            var random = new Random();
            NetPeerConfiguration config = new NetPeerConfiguration("ng");
            _client = new NetClient(config);
            _client.Start();
            Player = new Player("name_" + random.Next(0, 100), 0, 0);
            var outmsg = _client.CreateMessage();
            outmsg.Write((byte)PacketType.Login);
            outmsg.Write(Player.Name);
            _client.Connect("localhost", 63763, outmsg);

            return EsablishInfo();
        }

        private bool EsablishInfo()
        {
            var time = DateTime.Now;
            bool canStart = false;
            NetIncomingMessage inc;
            while (!canStart)
            {
                if ((inc = _client.ReadMessage()) != null)
                {
                    foreach (var item in inc.Data)
                    {
                        if (item != 32)
                        {
                            Console.Write(item + " ");
                        }
                    }
                    Console.WriteLine();
                    if (inc.ReadByte() == 5)
                    {
                        NetOutgoingMessage outmsg = _client.CreateMessage();

                        outmsg.Write((byte)PacketType.AcceptedConnection);
                        outmsg.Write(Player.Name);
                        _client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
                        isConnected = true;
                        canStart = true;
                    }
                    if (inc.ReadByte() == (byte)PacketType.Login)
                    {
                        Console.WriteLine("Hallelujah");
                    }
                }
            }
            return canStart;
        }

        public void Update()
        {
            NetIncomingMessage inc;
            while ((inc = _client.ReadMessage()) != null)
            {
                if (inc.MessageType != NetIncomingMessageType.Data) continue;
                var packageType = (PacketType)inc.ReadByte();
                switch (packageType)
                {
                    case PacketType.AcceptedConnection:
                        Console.WriteLine(inc.ReadBoolean());
                        break;
                    case PacketType.Login:
                        if (inc.ReadBoolean())
                        {
                            Player.xPosition = inc.ReadInt32();
                            Player.yPosition = inc.ReadInt32();
                            ReceiveAllPlayers(inc);
                        }
                        break;
                    case PacketType.NewPlayer:
                        var player = new Player();
                        inc.ReadAllProperties(player);
                        OtherPlayers.Add(player);
                        break;
                    case PacketType.AllPlayers:
                        ReceiveAllPlayers(inc);
                        break;
                    case PacketType.Disconnected:
                        var playerDc = new Player();
                        inc.ReadAllProperties(playerDc);
                        OtherPlayers.Remove(OtherPlayers.Find(x => x.Name == playerDc.Name));
                        break;

                    default:
                        break;
                }
            }
        }

        private void ReceiveAllPlayers(NetIncomingMessage inc)
        {
            var count = inc.ReadInt32();
            for (int n = 0; n < count; n++)
            {
                var player = new Player();
                inc.ReadAllProperties(player);
                if (player.Name == Player.Name)
                    continue;
                if (OtherPlayers.Any(p => p.Name == player.Name))
                {
                    var oldPlayer = OtherPlayers.FirstOrDefault(p => p.Name == player.Name);
                    oldPlayer.xPosition = player.xPosition;
                    oldPlayer.yPosition = player.yPosition;
                }
                else
                {
                    OtherPlayers.Add(player);
                }
            }
        }

        public void SendDisconnect()
        {
            var outmsg = _client.CreateMessage();
            outmsg.Write((byte)PacketType.Disconnected);
            outmsg.WriteAllProperties(Player);
            _client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
        }

    }
}
