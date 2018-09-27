using System;
using System.Collections.Generic;
using Lidgren.Network;
using NetworkGameLibrary;

namespace NetworkGameServer
{
    class Program
    {

     
        static void Main(string[] args)
        {

            var server = new Server();
            server.Run();
        }

        
    }
}
