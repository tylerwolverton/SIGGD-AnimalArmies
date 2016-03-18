using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game
{
    public class Constants
    {

        // Player
        public const int PLAYER_MASS = 30;
        public const int PLAYER_HEALTH = 100;
        public const float PLAYER_FRICTION = .3f;
        public const float PLAYER_SPEED = 10000.0f;

        // Blob
        public const int BLOB_MASS = 35;
        public const float BLOB_FRICTION = .13f;

        // FireShuriken
        public const int FIRESHURIKEN_MASS = 10;
        public const float FIRESHURIKEN_FRICTION = .01f;

        // Flare
        public const int FLARE_MASS = 20;
        public const float FLARE_FRICTION = .1f;
        public const int FLARE_COUNTER = 300;

        // WalkerBot
        public const int WALKER_MASS = 50;
        public const float WALKER_FRICTION = .2f;

        // NetRobot
        public const float NETROBOT_FRICTION = .2f;
        public const int NETROBOT_MASS = 20;
        public const int NETROBOT_NET_COOLDOWN = 300;

        // Net
        public const int NET_MASS = 10;
        public const float NET_FRICTION = .01f;
    }
}
