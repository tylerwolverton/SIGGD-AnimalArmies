/**
 * @file
 * @author PURDUE ACM, SIGGD <siggd.purdue@gmail.com>
 * @version 1.0
 * 
 * @section LICENSE
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace Engine
{
    //Represents the data used to construct and save a map
    public class Mapfile
    {
        //Represents everything needed to reconstruct a tile's initial state
        public struct TileData
        {
            //Ignore values
            public const string IGNORESTRING = "IGNORE";
            public const byte IGNOREBYTE = byte.MaxValue;
            public const float IGNOREFLOAT = float.NaN;

            public string texture; //The handle key of the texture
            public string behavior; //The handle key of the behavior

            public byte solidity; //bool to set solidity
            public byte opacityFlip; //bool to flip the opacity from its default

            public float leftSlope; //Height percentage of the slope on the left side
            public float rightSlope; //Height percentage of the slope on the right side
            public byte normal; //Normal direction of the slope, if slope is present. 1 for up, 0 for down.

            //Flaggies to come later

            //struct initializer
            public TileData(string texture = "", string behavior = "", byte solidity = 0, byte opacityFlip = 0, float leftSlope = 1f, float rightSlope = 1f, byte normal = 1)
            {
                this.texture = texture;
                this.behavior = behavior;
                this.solidity = solidity;
                this.opacityFlip = opacityFlip;
                this.leftSlope = leftSlope;
                this.rightSlope = rightSlope;
                this.normal = normal;
            }

            //Set all fields to ignore
            public void setToIgnore()
            {
                this.texture = IGNORESTRING;
                this.behavior = IGNORESTRING;
                this.solidity = IGNOREBYTE;
                this.opacityFlip = IGNOREBYTE;
                this.leftSlope = IGNOREFLOAT;
                this.rightSlope = IGNOREFLOAT;
                this.normal = IGNOREBYTE;
            }

            public bool isNonstandard()
            {
                if ((behavior != IGNORESTRING && behavior != "")) return true;
                return false;
            }

            //Checks if the non-ignore-valued data of the given TileData is equal to this TileData's data
            public override bool Equals(object obj)
            {
                if (!(obj is TileData))
                {
                    return false;
                }
                TileData data = (TileData) obj;

                if (this.texture == null) this.texture = "";
                if (this.behavior == null) this.behavior = "";
                if (data.texture == null) data.texture = "";
                if (data.behavior == null) data.behavior = "";

                if (data.texture != IGNORESTRING && data.texture != this.texture)                       return false;
                if (data.behavior != IGNORESTRING && data.behavior != this.behavior)                    return false;
                if (data.solidity != IGNOREBYTE && data.solidity != this.solidity)                      return false;
                if (data.opacityFlip != IGNOREBYTE && data.opacityFlip != this.opacityFlip)             return false;
                if (!data.leftSlope.Equals(IGNOREFLOAT) && !data.leftSlope.Equals(this.leftSlope))      return false;
                if (!data.rightSlope.Equals(IGNOREFLOAT) && !data.rightSlope.Equals(this.rightSlope))   return false;
                if (data.normal != IGNOREBYTE && data.normal != this.normal)                            return false;

                return true;
            }

            //Overwrites this TileData's data based on the non-ignore-valued data of the given TileData
            public void overWriteData(TileData data)
            {
                if (data.texture != IGNORESTRING && this.texture != IGNORESTRING)                   texture = data.texture;
                if (data.behavior != IGNORESTRING && this.behavior != IGNORESTRING)                 behavior = data.behavior;
                if (data.solidity != IGNOREBYTE && this.solidity != IGNOREBYTE)                     solidity = data.solidity;
                if (data.opacityFlip != IGNOREBYTE && this.opacityFlip != IGNOREBYTE)               opacityFlip = data.opacityFlip;
                if (!data.leftSlope.Equals(IGNOREFLOAT) && !this.leftSlope.Equals(IGNOREFLOAT))     leftSlope = data.leftSlope;
                if (!data.rightSlope.Equals(IGNOREFLOAT) && !this.rightSlope.Equals(IGNOREFLOAT))   rightSlope = data.rightSlope;
                if (data.normal != IGNOREBYTE && this.normal != IGNOREBYTE)                         normal = data.normal;
            }
        }

        //Represents everything needed to reconstruct an actor's initial state
        public struct ActorData
        {
            public byte id; //The id of the actor
            public float x; //The x location of the actor
            public float y; //The y location of the actor
            public byte z; //The layer the actor is on

            public ActorData(byte id = 0, ushort x = 0, ushort y = 0, byte z = 0)
            {
                this.id = id;
                this.x = x;
                this.y = y;
                this.z = z;
            }
        }

        //WorldData
        public string worldName;
        public string filePath; //The key of the world
        public string worldBehaviorKey; //The handle key of the world's behavior
        public TileData[, ,] worldTileData; //The 3D TileData array used in reconstructing the world's tiles
        public List<ActorData> worldActorData; //The list of ActorData used in reconstructing the world's actors
        
        //A dictionary from strings to integers that indexes handle keys
        public Dictionary<string, int> handleKeysIndex; 

        /*
         * Constructs a new mapfile of an empty world with the given dimensions
         * @param width the width of the empty world
         * @param height the height of the empty world
         * @param name the name of the empty world
        */
        public Mapfile(int width, int height, string name = "NewWorld")
        {
            initWorldData(name, width, height);
            createHandleKeyDictionary();
        }

        //Constructs the mapfile from the disk
        //@param filepath The full or relative path to the map file on the disk
        public Mapfile(string filePath)
        {
            initWorldData(filePath);
            load();
        }

        //Constructs the mapfile from an existing world
        public Mapfile(World world)
        {
            initWorldData(world.worldName, world.width, world.height, world.myBehavior.scriptKey);
            createHandleKeyDictionary();
        }

        //Initializes the world data given its initial state
        public void initWorldData(string filePath = "", int width = 0, int height = 0, string behavior = null, List<ActorData> actors = null)
        {
            this.worldName = ResourceComponent.getKeyFromPath(filePath);
            this.filePath = ResourceComponent.getFullPathFromKey(filePath);
            this.worldBehaviorKey = behavior;
            this.worldTileData = new TileData[1, width, height];
            this.worldActorData = actors;

            if (this.worldActorData == null) this.worldActorData = new List<ActorData>();
            if (this.worldBehaviorKey == null) this.worldBehaviorKey = "";

            //Construct tiles
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    worldTileData[0, x, y] = new TileData("");
                }
            }
        }

        //Creates the handle key dictionary
        public void createHandleKeyDictionary()
        {
            handleKeysIndex = new Dictionary<string,int>();
            handleKeysIndex.Add("", -1);

            int i = 0;
            foreach(TileData td in worldTileData){
                //Add the tile's texture if not already indexed
                if (!handleKeysIndex.ContainsKey(td.texture))
                {
                    handleKeysIndex.Add(td.texture, i++);
                }

                //Add the tile's behavior if not already indexed
                if (!handleKeysIndex.ContainsKey(td.behavior))
                {
                    handleKeysIndex.Add(td.behavior, i++);
                }
            }
        }

        /**
        * Loads the worldData from disk
        */
        public void load()
        {
            FileStream loadStream = new FileStream(filePath, FileMode.Open);
            BinaryReader file = new BinaryReader(loadStream);
            if (file == null)  return;

            //Read magic Number
            const uint MAGIC = 0x67319642;
            uint magic = file.ReadUInt32(); //1
            if (magic != MAGIC) throw new Exception("Map load error: Bad file format");

            //Read map key
            filePath = file.ReadString(); //2

            //Read behavior key
            worldBehaviorKey = file.ReadString(); //3

            //Read handle key index
            Dictionary<int, string> keyIDs = new Dictionary<int, string>();
            int numIDs = file.ReadInt32(); //4
            for (int i = 0; i < numIDs; i++) //5
            {
                int id = file.ReadInt32(); //a
                string handleKey = file.ReadString(); //b
                keyIDs.Add(id, handleKey);
            }

            //Read tiles
            byte width = file.ReadByte(); //6
            byte height = file.ReadByte(); //7
            worldTileData = new TileData[1, width, height];
            for (int x = 0; x < width; x++) //8
            {
                for (int y = 0; y < height; y++)
                {
                    TileData td = new TileData("");

                    //Read texture index
                    td.texture = keyIDs[file.ReadInt32()]; //a
                    
                    //Read behavior index
                    td.behavior = keyIDs[file.ReadInt32()]; //b

                    //Read solidity
                    td.solidity = file.ReadByte(); //c
                    
                    //Read opactiy
                    td.opacityFlip = file.ReadByte(); //d

                    worldTileData[0, x, y] = td;
                }
            }

            //Read actors
            ushort nWorldObjects = file.ReadUInt16(); //9
            worldActorData = new List<ActorData>();
            for (int i = 0; i < nWorldObjects; i++) //10
            {
                ActorData wod = new ActorData();

                wod.id = file.ReadByte(); //a
                wod.x = file.ReadSingle(); //b
                wod.y = file.ReadSingle(); //c
                wod.z = file.ReadByte(); //d

                worldActorData.Add(wod);
            }

            // Magic Number
            magic = file.ReadUInt32(); //11
            if (magic != MAGIC) throw new Exception("Map load error: Bad file format");

            createHandleKeyDictionary();
            loadStream.Close();
        }

        /**
        * Saves the worldData to disk
        */
        public void save()
        {
            FileStream saveStream = new FileStream(filePath, FileMode.Create);
            BinaryWriter file = new BinaryWriter(saveStream);
            if (file == null) return;

            //Bounds checking
            if (worldTileData.GetLength(0) > byte.MaxValue) throw new Exception("Map write error: too many layers.");
            if (worldTileData.GetLength(1) > byte.MaxValue) throw new Exception("Map write error: too wide.");
            if (worldTileData.GetLength(2) > byte.MaxValue) throw new Exception("Map write error: too tall.");
            if (worldActorData.Count > ushort.MaxValue) throw new Exception("Map write error: too many actors.");

            const uint MAGIC = 0x67319642; //changed

            //Write magic number
            file.Write(MAGIC); //1

            //Write map key
            file.Write(filePath); //2

            //Write behavior key
            file.Write(worldBehaviorKey); //3

            //Write handle key index with keys and values reversed
            createHandleKeyDictionary();
            file.Write(this.handleKeysIndex.Count); //4
            foreach (KeyValuePair<string, int> p in handleKeysIndex) //5
            {
                file.Write(p.Value); //a
                file.Write(p.Key); //b
            }

            //Write dimensions
            byte width = (byte)worldTileData.GetLength(1); 
            byte height = (byte)worldTileData.GetLength(2); 
            file.Write((byte)worldTileData.GetLength(1)); //6
            file.Write((byte)worldTileData.GetLength(2)); //7

            //Write tiles
            for (int x = 0; x < width; x++) //8
            {
                for (int y = 0; y < height; y++)
                {
                    TileData td = worldTileData[0, x, y];

                    //write texture index
                    file.Write(handleKeysIndex[td.texture]); //a

                    //write behavior index
                    file.Write(handleKeysIndex[td.behavior]); //b

                    //Write solidity
                    file.Write(td.solidity); //c

                    //Write opacityFlip
                    file.Write(td.opacityFlip); //d
                }
            }

            //Write num actors
            file.Write((ushort)worldActorData.Count); //9

            //Write actors
            foreach (ActorData wod in worldActorData) //10
            {
                file.Write(wod.id); //a
                file.Write(wod.x); //b
                file.Write(wod.y); //c
                file.Write(wod.z); //d
            }

            //Write magic number
            file.Write(MAGIC); //11

            saveStream.Close();
        }
    }
}

/* Current .map format:
 * 
 * 1. uint MAGIC
 * 2. string filePath/worldName
 * 3. string worldBehavior
 * 4. int numUniqueHandleKeys
 * 5. foreach uniqueHandleKey found in the world's tiles:
 *      a. int tempIndex
 *      b. string handleKey
 * 6. byte worldWidth
 * 7. byte worldHeight
 * 8. foreach tiledata:
 *      a. int handleKeyIndex for tile texture
 *      b. int hanldeKeyIndex for tile behavior
 *      c. byte solidity
 *      d. byte opacityFlip
 * 9. ushort numActors
 * 10. foreach actordata:
 *      a. byte id
 *      b. float x
 *      c. float y
 *      d. byte z
 * 11. uint MAGIC
 */

/* Interim save format:
 * 
 * No interim save format,
 * Save and load are both up to date and matching,
 * See current .map format.
 */

/* Last .map format:
 * 
 * Unknown.
 */