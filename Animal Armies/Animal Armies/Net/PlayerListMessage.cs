using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Game.Net
{
    /**
     * Sent from host to clients to inform them of the other players.
     */
    [Serializable]
    public class PlayerListMessage
    {
        public IPAddress[] hosts;
    }
}
