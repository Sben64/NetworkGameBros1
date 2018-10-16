using System;
using System.Collections.Generic;
using Lidgren.Network;
using NetworkGameLibrary;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace NetworkGameServer
{
    class Server
    {
        private List<Player> _players;
        private NetPeerConfiguration _config;
        private List<NetConnection> connectedClients;
        private NetPeer _netPeer;
        private GameTime _gametime = new GameTime();
        public Server()
        {
            _players = new List<Player>();
            connectedClients = new List<NetConnection>();
            _config = new NetPeerConfiguration("ng") { Port = 63763 };
            _config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            _netPeer = new NetServer(_config);
        }

        public void Run()
        {
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
                            //Quand un joueur se déconnecte il envoie le Packet 'Disconnect'
                            else if (type == (byte)PacketType.Disconnected)
                            {
                                //On créer un nouveau joueur
                                var player = new Player();
                                //On assigne les propriétés au nouveau joueur
                                inc.ReadAllProperties(player);
                                //On créé un nouveau message
                                var outmsg = _netPeer.CreateMessage();
                                outmsg.Write((byte)PacketType.Disconnected);
                                outmsg.WriteAllProperties(_players.Find(x => x.Name == player.Name));

                                _netPeer.SendMessage(outmsg, connectedClients, NetDeliveryMethod.ReliableOrdered, 0);

                                //Enleve la connection et le joueur des listes
                                connectedClients.Remove(inc.SenderConnection);
                                _players.Remove(_players.Find(x => x.Name == player.Name));
                            }
                            else if (type == (byte)PacketType.Input)
                            {

                                Inputs(inc);
                            }
                            break;
                        case NetIncomingMessageType.StatusChanged:
                            Console.WriteLine(inc.SenderConnection.ToString() + " changed : " + inc.SenderConnection.Status);
                            //Si une connection est fermé sans envoyer un packet de type disconnected alors on le détecte et on l'enleve de la liste
                            if (inc.SenderConnection.Status == NetConnectionStatus.Disconnected)
                            {

                                foreach (var item in connectedClients)
                                {
                                    if (item.Status == NetConnectionStatus.Disconnected)
                                    {
                                        var outmsg = _netPeer.CreateMessage();
                                        outmsg.Write((byte)PacketType.Disconnected);
                                        outmsg.WriteAllProperties(_players.Find(x => x.connection == item));
                                        _netPeer.SendMessage(outmsg, connectedClients, NetDeliveryMethod.ReliableOrdered, 0);
                                    }
                                }
                                connectedClients.Remove(inc.SenderConnection);
                                _players.Remove(_players.Find(x => x.connection == inc.SenderConnection));
                            }
                            break;
                        default:
                            Console.WriteLine("What is happening ??");
                            break;
                    }


                }
                Fall();
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
                //_position = new Vector2(random.Next(0, 750), random.Next(0, 420))
                xPosition = random.Next(0, 750),
                yPosition = random.Next(0, 420)
            };

            return player;
        }
        private void SendNewPosition(Player player, NetIncomingMessage inc)
        {
            Console.WriteLine("Send player position");
            var outmessage = _netPeer.CreateMessage();
            outmessage.Write((byte)PacketType.Input);
            outmessage.WriteAllProperties(player);
            _netPeer.SendMessage(outmessage, connectedClients, NetDeliveryMethod.ReliableOrdered, 0);
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
            var player = new Player();
            bool collided = false;
            player = _players.Find(x => x.connection == inc.SenderConnection);
            player._velocity = Vector2.Zero;

            switch (key)
            {
                case Keys.Down:
                    if (!collided || _players.Count == 1)
                    {
                        player._velocity.Y += player.speed;
                    }
                    foreach (var item in _players)
                    {
                        if (player != item)
                        {
                            if (player._velocity.Y < 0 && IsTouchingBottom(player, item) || player._velocity.Y > 0 && IsTouchingTop(player, item))
                            {
                                Console.WriteLine(IsTouchingBottom(player, item) + " : " + IsTouchingTop(player, item));
                                collided = true;
                                player._velocity.Y = 0f;
                                continue;
                            }
                        }

                    }

                    break;

                case Keys.Up:

                    foreach (var item in _players)
                    {
                        if (player != item)
                        {
                            if (player._velocity.Y < 0 && IsTouchingBottom(player, item) || player._velocity.Y > 0 && IsTouchingTop(player, item))
                            {
                                Console.WriteLine(IsTouchingBottom(player, item) + " : " + IsTouchingTop(player, item));
                                collided = true;
                                player._velocity.Y = 0f;
                                continue;
                            }
                        }

                    }

                    break;

                case Keys.Left:
                    if (!collided || _players.Count == 1)
                    {
                        player._velocity.X -= player.speed;
                    }
                    foreach (var item in _players)
                    {
                        if (player != item)
                        {
                            if (player._velocity.X < 0 && IsTouchingRight(player, item) || player._velocity.X > 0 && IsTouchingLeft(player, item))
                            {
                                Console.WriteLine(IsTouchingRight(player, item) + " : " + IsTouchingLeft(player, item));
                                collided = true;
                                player._velocity.X = 0f;
                                continue;
                            }
                        }
                    }

                    break;
                case Keys.Right:
                    player._velocity.X += player.speed;
                    foreach (var item in _players)
                    {
                        if (player != item)
                        {
                            if (player._velocity.X > 0 && IsTouchingLeft(player, item) || player._velocity.X < 0 && IsTouchingRight(player, item))
                            {
                                Console.WriteLine(IsTouchingRight(player, item) + " : " + IsTouchingLeft(player, item));
                                collided = true;
                                player._velocity.X = 0f;
                                continue;
                            }
                        }
                    }
                    break;
                default:
                   
                    break;
            }

            player._position += player._velocity;
            SendNewPosition(player, inc);
        }
        private void Fall()
        {
            if (_players.Count > 0)
            {
                foreach (var player in _players)
                {
                    if (player._inAir == true)
                    {
                        player._velocity.Y += 10f * 1f;
                    }
                }
            }
        }
        // NICO :
        // IsTouchingLeft et IsTouchingRight donnent le même résultat
        // Probablement Down et Up
        protected bool IsTouchingLeft(Player player1, Player player2)
        {
            return player1.BoundingBox.Right + player1._velocity.X > player2.BoundingBox.Left &&
                   player1.BoundingBox.Left < player2.BoundingBox.Left &&
                   player1.BoundingBox.Bottom > player2.BoundingBox.Top &&
                   player1.BoundingBox.Top < player2.BoundingBox.Bottom;
        }

        protected bool IsTouchingRight(Player player1, Player player2)
        {
            return player1.BoundingBox.Left + player1._velocity.X < player2.BoundingBox.Right &&
                   player1.BoundingBox.Right > player2.BoundingBox.Right &&
                   player1.BoundingBox.Bottom > player2.BoundingBox.Top &&
                   player1.BoundingBox.Top < player2.BoundingBox.Bottom;
        }

        protected bool IsTouchingTop(Player player1, Player player2)
        {
            return player1.BoundingBox.Bottom + player1._velocity.Y > player2.BoundingBox.Top &&
                   player1.BoundingBox.Top < player2.BoundingBox.Top &&
                   player1.BoundingBox.Right > player2.BoundingBox.Left &&
                   player1.BoundingBox.Left < player2.BoundingBox.Right;
        }

        protected bool IsTouchingBottom(Player player1, Player player2)
        {
            return player1.BoundingBox.Top + player1._velocity.Y < player2.BoundingBox.Bottom &&
                   player1.BoundingBox.Bottom > player2.BoundingBox.Bottom &&
                   player1.BoundingBox.Right > player2.BoundingBox.Left &&
                   player1.BoundingBox.Left < player2.BoundingBox.Right;
        }

    }
}