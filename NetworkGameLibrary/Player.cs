using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NetworkGameLibrary
{
    public class Player
    {
        public string Name { get; set; }

        public float speed;

        public NetConnection connection { get; set; }
        //public Vector2 _position;
        //public Vector2 _velocity;
        public Rectangle BoundingBox => new Rectangle(
            (int)xPosition,
            (int)yPosition,
            32,
            48
            );
  
        public int xPosition { get; set; }
        public int yPosition { get; set; }

        public Player(string name, int xPos, int yPos)
        {
            Name = name;
            xPosition = xPos;
            yPosition = yPos;
        }
        public Player() { }
    }
}
