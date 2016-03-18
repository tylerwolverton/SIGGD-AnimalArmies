using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using Game.Net;
using System.Net;

namespace Game
{
    /**
     * Represents a connection to another client.
     */
    class Connection
    {
        public delegate void WhenDone();

        private NetworkStream stream;
        private BinaryFormatter form;

        /**
         * Constructor
         * 
         * @param hostName      The name of the host
         * @param port          The port
         * @param whenConnected Called once connection established
         */
        public Connection(string hostName, int port, WhenDone whenConnected)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            sock.BeginConnect(hostName, port, (result) => {
                sock.EndConnect(result);

                stream = new NetworkStream(sock, true);

                if (result.IsCompleted) {
                    whenConnected();
                }
            }, sock);
            
            form = new BinaryFormatter();
        }

        /**
         * Constructor
         * 
         * @sock The socket 
         */
        public Connection(Socket sock)
        {
            stream = new NetworkStream(sock, true);
            form = new BinaryFormatter();
        }
        
        /**
         * Sends a message
         */
        public void send(Object msg) {
            form.Serialize(stream, msg);
        }
    }
}
