using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using Game.Net;

namespace Game
{
    /**
     * This class provides networking functionality.
     */
    public class NetClient
    {
        public const int PORT = 13579;

        // Used to register an event
        public delegate void WhenDone();

        // True if hosting a game
        public bool isHosting { get; private set; }

        private Connection[] players;

        /**
         * Constructor.
         */
        public NetClient()
        {
            isHosting = false;
            players = null;
        }

        /**
         * Called to host a game asynchronously
         * 
         * @param numPlayers       Number of players to wait for
         * @param whenAllConnected Called when all playes connected
         */
        public void hostGame(uint numPlayers, WhenDone whenAllConnected)
        {
            players = new Connection[numPlayers-1];

            isHosting = true;

            uint connPlayers = 0;      // Number of players connected
            List<IPAddress> hosts = new List<IPAddress>();

            Socket servSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            servSock.Bind(new IPEndPoint(IPAddress.Any, PORT));
            servSock.Listen(4);
            servSock.BeginAccept((result) => {
                if (result.IsCompleted) {
                    Socket cliSock = servSock.EndAccept(result);
                    IPEndPoint endPoint = (IPEndPoint)cliSock.RemoteEndPoint;
                    hosts.Add(endPoint.Address);
                    players[connPlayers++] = new Connection(servSock.EndAccept(result));
                }

                if (connPlayers == numPlayers-1) {
                    // Send player list message to all players
                    PlayerListMessage msg = new PlayerListMessage();
                    msg.hosts = hosts.ToArray();
                    foreach (Connection c in players) {
                        c.send(msg);
                    }
 
                    whenAllConnected();
                }
            }, servSock);
        }

        /**
         * Called to join a game asynchronously
         * @param hostName String containing the hostname of the game to join.
         * @param whenConnected Called when connected
         */
        public void joinGame(String hostName, WhenDone whenConnected)
        {
            Connection host = new Connection(hostName, PORT, () => {
            });
        }
    }
}
