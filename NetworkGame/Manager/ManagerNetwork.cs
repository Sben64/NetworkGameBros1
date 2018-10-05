using System;
using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using Microsoft.Xna.Framework.Input;
using NetworkGameLibrary;
using NetworkGame.Manager;

namespace NetworkGame
{
    class ManagerNetwork
    {
        private NetClient _client;
        public Player Player { get; set; }
        public List<Player> OtherPlayers { get; set; }
        public static bool isConnected = false;
        public bool Active { get; set; }

        /// <summary>
        /// Inputs
        /// </summary>
        Keys down = Keys.Down;
        Keys up = Keys.Up;
        Keys right = Keys.Right;
        Keys left = Keys.Left;


        ManagerInput _input;
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
            _input = new ManagerInput(this);

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
                        break;
                    case PacketType.Login:
                        if (inc.ReadBoolean())
                        {
                            Player.xPosition = inc.ReadFloat();
                            Player.yPosition = inc.ReadFloat();
                            ReceiveAllPlayers(inc);
                        }
                        break;
                        //Recois un Packet de type nouveau joueur.
                    case PacketType.NewPlayer:
                        //Créé l'instance du nouveau joueur
                        var player = new Player();
                        //Assigne les propriétés reçu au nouveau joueur
                        inc.ReadAllProperties(player);
                        //Si le nom du nouveau joueur est différent du nom du joueur alors on le rajoute à la liste
                        if (player.Name != Player.Name)
                        {
                            OtherPlayers.Add(player);
                        }
                        break;
                    case PacketType.AllPlayers:
                        //Reçois tous les joueurs déjà présent.
                        ReceiveAllPlayers(inc);
                        break;
                    case PacketType.Disconnected:
                        //Quand un joueur se déconnecte on lui envoie les propriétés du joueur déconnecté et on le supprime de la liste
                        var playerDc = new Player();
                        inc.ReadAllProperties(playerDc);
                        OtherPlayers.Remove(OtherPlayers.Find(x => x.Name == playerDc.Name));
                        break;
                    case PacketType.Input:
                        var playerPos = new Player();
                        inc.ReadAllProperties(playerPos);
                        if (Player.Name == playerPos.Name)
                        {
                            Player._position.X = playerPos._position.X;
                            Player._position.Y = playerPos._position.Y;
                        }
                        else if(OtherPlayers.Count > 0)
                        {
                            try
                            {
                                OtherPlayers.Find(x => x.Name == playerPos.Name)._position.X = playerPos._position.X;
                                OtherPlayers.Find(y => y.Name == playerPos.Name)._position.Y = playerPos._position.Y;
                            }
                            catch
                            {

                            }
                        }
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
                    oldPlayer._position.X = player._position.X;
                    oldPlayer._position.Y = player._position.Y;
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
            _client.Disconnect("Byebyelesgars");
        }

        public void SendInput(Keys keys)
        {
            var outmessage = _client.CreateMessage();
            outmessage.Write((byte)PacketType.Input);
            outmessage.Write((byte)keys);
            outmessage.Write(Player.Name);
            _client.SendMessage(outmessage, NetDeliveryMethod.ReliableOrdered);
        }
    }
}
