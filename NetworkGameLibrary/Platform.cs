using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkGameLibrary
{
    public class Platform
    {
        public Texture2D _texture;
        public Vector2 _position;
        public Rectangle _boundingBox;
        public float _gravity = 10f;
        private bool collide;

        public Platform(Texture2D texture2,Vector2 vector2)
        {
            _texture = texture2;
            _position = vector2;
            _boundingBox = new Rectangle(
                                (int)_position.X,
                                (int)_position.Y,
                                _texture.Width,
                                _texture.Height
                                );
        }

        public bool CollisionPlatform(List<Player> players)
        {
            
            foreach (var item in players)
            {
                if (this._boundingBox.Intersects(item.BoundingBox))
                {
                    collide = item._inAir = false;
                }
            }
            return collide;
        }
    }
}
