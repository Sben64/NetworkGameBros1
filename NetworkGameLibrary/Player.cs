using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace NetworkGameLibrary
{
    public class Player
    {
        public string Name { get; set; }

        public NetConnection connection { get; set; }
        public Texture2D _texture;       
        public Vector2 _position;
        public Vector2 _velocity;
        public float speed = 5f;
        public float jumpSpeed = 500f;
        public bool _inAir = true;
        public Rectangle BoundingBox => new Rectangle(
            (int)xPosition,
            (int)yPosition,
            32,
            48
            );



        public float xPosition
        {
            get => _position.X;
            set => _position.X = value;
        }
        //public int xPosition { get; set; }
        public float yPosition
        {
            get => _position.Y;
            set => _position.Y = value;
        }
        //public int yPosition { get; set; }

        public Player(string name, int xPos, int yPos)
        {
            Name = name;
            
            xPosition  = xPos;
            yPosition  = yPos;
        }

        public Player()
        {
            
        }

        public bool Collision(List<Player> otherPlayer)
        {
            bool collide = false;
            foreach (var item in otherPlayer)
            {
                if(this.BoundingBox.Intersects(item.BoundingBox))
                    collide = true;
            }
            return collide;
        }
    }
}
