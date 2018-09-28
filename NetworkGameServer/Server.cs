using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Lidgren.Network;
using NetworkGameLibrary;
using Microsoft.Xna.Framework.Input;

namespace NetworkGameServer
{
    class Server
    {
        private List<Player> _players;
        private NetPeerConfiguration _config;
        //private NetServer _netPeer;
        private List<NetConnection> connectedClients;
        private NetPeer _netPeer;
        public Server()
        {
            _players = new List<Player>();
            connectedClients = new List<NetConnection>();
            _config = new NetPeerConfiguration("ng") { Port = 63763 };
            _config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            //_netPeer = new NetServer(_config);
            _netPeer = new NetServer(_config);
        }

        public void Run()
        {
            //_server.Start();
            _netPeer.Start();
            NetIncomingMessage inc;
            Console.WriteLine("Server started...");
            while (true)
            {
                if ((inc = _netPeer.ReadMessage()) != null)
                {
                    switch (inc.MessageType)
                    {

                        case NetIncomingMessageType.ConnectionApproval:
                            ConnectionApproval(inc);
                            break;
                        case NetIncomingMessageType.Data:
                            var type = inc.ReadByte();
                            if (type == (byte)PacketType.AcceptedConnection)
                            {
                                ConnectionAccepted(inc);
                                SendFullPlayerList();
                            }
                            else if (type == (byte)PacketType.Disconnected)
                            {
                                var player = new Player();
                                inc.ReadAllProperties(player);

                                var outmsg = _netPeer.CreateMessage();
                                outmsg.Write((byte)PacketType.Disconnected);
                                outmsg.WriteAllProperties(_players.Find(x => x.Name == player.Name));

                                _netPeer.SendMessage(outmsg, connectedClients, NetDeliveryMethod.ReliableOrdered, 0);
                                
                                //Enleve la connection et le joueur des listes
                                connectedClients.Remove(inc.SenderConnection);
                                _players.Remove(_players.Find(x => x.Name == player.Name));
                            }
                            break;
                        case NetIncomingMessageType.StatusChanged:
                            Console.WriteLine(inc.SenderConnection.ToString() + " changed : " + inc.SenderConnection.Status);
                            if (inc.SenderConnection.Status == NetConnectionStatus.Disconnected)
                            {
                                var outmsg = _netPeer.CreateMessage();
                                
                                foreach (var item in connectedClients)
                                {
                                    if (item.Status == NetConnectionStatus.Disconnected)
                                    {
                                        outmsg.Write((byte)PacketType.Disconnected);
                                        outmsg.WriteAllProperties(_players.Find(x => x.connection == item));
                                    }
                                }
                                _netPeer.SendMessage(outmsg, connectedClients, NetDeliveryMethod.ReliableOrdered, 0);
                                connectedClients.Remove(inc.SenderConnection);
                                _players.Remove(_players.Find(x => x.connection == inc.SenderConnection));
                            }
                            break;
                        default:
                            Console.WriteLine("What is happening ??");
                            break;
                    }
                    /*
                     * Que faire quand un client se disconnecte : 
                     * 1) Détecter
                     *
                     * 2) Envoie de l'information aux clients
                     * 
                     * 3) Supprimer le client de la liste
                     * 
                     * 4) Continuer
                     */


                }

            }
        }

        private void ConnectionAccepted(NetIncomingMessage inc)
        {
            //Envoie du paquet AcceptedConnection
            var outmsg = _netPeer.CreateMessage();
            outmsg.Write((byte)PacketType.AcceptedConnection);
            outmsg.Write(true);
            _netPeer.SendMessage(outmsg, inc.SenderConnection, NetDeliveryMethod.ReliableOrdered);
            Console.WriteLine("Packets send (PacketType : AcceptedConnection )");

            //Envoie du paquet Login
            outmsg = _netPeer.CreateMessage();
            var player = CreatePlayer(inc);
            _players.Add(player);
            outmsg.Write((byte)PacketType.Login);
            outmsg.Write(true);
            outmsg.Write(player.xPosition);
            outmsg.Write(player.yPosition);
            outmsg.Write(_players.Count - 1);
            Console.WriteLine("Packets send (PacketType : Login )");
            //Envoie du paquet NewPlayer
            for (int n = 0; n < _players.Count - 1; n++)
            {
                outmsg.WriteAllProperties(_players[n]);
            }
            _netPeer.SendMessage(outmsg, inc.SenderConnection, NetDeliveryMethod.ReliableOrdered);
            Console.WriteLine("Packets send (PacketType : NewPlayer )");


            SendNewPlayer(player, inc);

        }

        private void ConnectionApproval(NetIncomingMessage inc)
        {
            Console.WriteLine("New connection...");
            var data = inc.ReadByte();
            if (data == (byte)PacketType.Login)
            {
                Console.WriteLine("..connection accepted.");
                Console.WriteLine(inc.ReadString() + "..");
                inc.SenderConnection.Approve();
                connectedClients.Add(inc.SenderConnection);
            }
            else
            {
                inc.SenderConnection.Deny("Didn't send correct information.");
            }
        }

        private Player CreatePlayer(NetIncomingMessage inc)
        {
            var random = new Random();
            var player = new Player
            {
                Name = inc.ReadString(),
                connection = inc.SenderConnection,
                xPosition = random.Next(0, 750),
                yPosition = random.Next(0, 420)
            };
            return player;
        }

        private void SendNewPlayer(Player player, NetIncomingMessage inc)
        {
            Console.WriteLine("Sending out new player position");
            var outmessage = _netPeer.CreateMessage();
            outmessage.Write((byte)PacketType.NewPlayer);
            outmessage.WriteAllProperties(player);
            _netPeer.SendMessage(outmessage, connectedClients, NetDeliveryMethod.ReliableOrdered, 0);
        }

        private void SendFullPlayerList()

        {

            Console.WriteLine("Sending full player list");
            var outmessage = _netPeer.CreateMessage();
            outmessage.Write((byte)PacketType.AllPlayers);
            outmessage.Write(_players.Count);
            foreach (var player in _players)
            {
                outmessage.WriteAllProperties(player);
            }
            _netPeer.SendMessage(outmessage, connectedClients, NetDeliveryMethod.ReliableOrdered, 0);

        }

        private void Inputs(NetIncomingMessage inc)
        {
            Console.WriteLine("Received new input");
            var key = (Keys)inc.ReadByte();

            switch (key)
            {
                case Keys.Down:
                    _players.Find(x => x.connection == inc.SenderConnection).yPosition++;
                    break;
                case Keys.Up:
                    _players.Find(x => x.connection == inc.SenderConnection).yPosition--;
                    break;
                case Keys.Left:
                    _players.Find(x => x.connection == inc.SenderConnection).xPosition--;
                    break;
                case Keys.Right:
                    _players.Find(x => x.connection == inc.SenderConnection).xPosition++;
                    break;
            }
            SendNewPlayer(_players.Find(x=>x.connection == inc.SenderConnection), inc);
        }
    }
}