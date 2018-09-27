using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
namespace NetworkGameLibrary
{
    public class Player
    {
        public string Name { get; set; }

        public NetConnection connection { get; set; }

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
