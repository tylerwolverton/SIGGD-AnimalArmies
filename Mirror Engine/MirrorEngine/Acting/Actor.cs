/**
 * @file Actor.cs
 * @author SIGGD, PURDUE ACM  <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Engine
{
    //Represents a world entity that has a behavior
    public class Actor
    {
        //Data
        public World world { get; set; }
        public string actorName;                    //The name of the actor
        public Behavior myBehavior { get; set; }    //Defines what the actor does
        public bool active;                         //Determines whether the actor's behavior is updated
        
        //Positioning
        public Tile curTile { get { return world.getTileAt(position.x, position.y); } } //The tile the actor is standing on
        public Vector2 spawn;       //Spawn location
        public Vector2 lastPos;     //Previous position of the actor
        public Vector2 position_;   //Current position of the actor
        public Vector2 position
        {
            get
            {
                return position_;
            }
            set
            {
                lastPos = value;
                position_ = value;
            }
        }

        //Sizing
        public float diameter;  //The diameter of the bounding circle
        public Vector2 size;    //The size of the bounding rectangle

        //Weight
        public float mass;                  //Represents the actor's resistance to change of velocity
        public float frictionCoefficient;   //Coefficient of kinetic friction       //Friction is defined by the following equation: Force of friction = Coefficient of Friction * Normal Force
        //staticfrictioncoefficient
        public float fNormal;               //Normal force. Doesn't make sense, should just be mass*gravity     //(game programmers: set this depending on the tile an actor touches/occupies)
        //elasticity

        //Coloring
        public Color color;                 //The actor's current color
        public float colorChangeRate;       //Dunno
        public Color tint = Color.WHITE;    //The overriding color of the actor
        
        //Animation
        public Handle sprite;           //The actor's current image
        public int xoffset;             //xOffset for the actor's image
        public int yoffset;             //yOffset for the actor's image
        public Vector2 world2model;     //Shouldn't this just use the offsets? Added to the game coordinates to get the sprite coordinates. Allows collision to be in a different position than drawing
        private Animation _anim;        //The animation that the actor displays
        public Animation anim
        {
            get
            {
                return _anim;
            }
            set
            {
                if (_anim != null && _anim != value)
                {
                    _anim.reset();
                }
                _anim = value;

                if (_anim != null)
                {
                    this.sprite = _anim.getCurTex();
                    this.xoffset = _anim.xoffset;
                    this.yoffset = _anim.yoffset;
                }
            }
        }

        //Drawing
        public delegate void CustomDraw(Actor a);   //Delegate used with customDraw, in order to provide custom drawing functionality for an actor.
        public CustomDraw customDraw;               //The actor's custom draw routine, if any
        public bool defaultDraw = true;             //Determines whether the actor should use the custom draw

        //Movement
        public bool ignoreAvE;  //Ignore Actor vs Environment rectification
        public Vector2 force = Vector2.Zero;   //The sum of the forces acting on the object
        public Vector2 velocity //Change in position since the last frame
        {
            get { return position - lastPos; }
            set { lastPos = position - value; }
        }

        //Lists of Tiles that the actor is clipping against
        public List<Tile> topClipTiles { get; internal set; }
        public List<Tile> rightClipTiles { get; internal set; }
        public List<Tile> botClipTiles { get; internal set; }
        public List<Tile> leftClipTiles { get; internal set; }

        //Removal
        public delegate void RemEvent();    //Delegate for removeEvent
        public event RemEvent removeEvent;  //The actor's remove event
        public bool removeMe;               //If set, actor will be removed by World after update
        public void notifyRemoved()         //Notifies the actor of its removal
        {
            if (removeEvent != null) removeEvent();
        }

        /**
        * Constructor
        */
        public Actor(World world, Vector2 spawn, Vector2 initialVelocity, float diameter, Vector2 size, Vector2 world2model)
        {
            //Data
            this.world = world;
            this.actorName = "noname";
            this.active = true;

            //Positioning
            this.spawn = spawn;
            this.position = spawn;
            this.velocity = initialVelocity;

            //Sizing
            this.diameter = diameter;
            this.size = size;

            //Weight
            this.mass = 25; // Hardcoded for now.
            this.frictionCoefficient = 1f; // Hardcoded for now.  Will eventually change based on tile you are walking on.
            //staticfrictioncoeficient
            this.fNormal = 0;  // Default: Use if you want friction
            //elasticity

            //Coloring
            this.colorChangeRate = .3f;
            if (world.getTileAt(spawn) != null) this.color = world.getTileAt(spawn).getFinalColor();
            this.world2model = world2model;

            //Clipping tiles
            this.topClipTiles = new List<Tile>();
            this.rightClipTiles = new List<Tile>();
            this.botClipTiles = new List<Tile>();
            this.leftClipTiles = new List<Tile>();
        }

        /**
        * Runs the actor's behavior an animates the actor
        */
        public virtual void Update()
        {
            if (active)
            {
                if (myBehavior != null) 
                myBehavior.run();
                animate();
            }
        }

        /**
        * Notifies the actor that it has collided with another actor
        * Calls the collide script method
        *
        * @param a The other actor
        */
        public virtual void collide(Actor a)
        {
            PhysicsComponent.avaResolve(this, a);

            dynamic collide = myBehavior.getVariable("collide");
            try
            {
                if (collide != null) collide(this, a);
            }
            catch (Exception e)
            {
                Trace.WriteLine(Script.getRuntimeError(e, actorName));
                myBehavior.removeVariable("collide");
            }
        }

        /**
        * Notifies the actor that it has it a wall
        */
        public virtual void hitWall(){}

        /**
        * Updates the actor's animation and sets the sprite to the new frame
        */
        public virtual void animate()
        {
            if (anim != null)
            {
                anim.run();
                sprite = anim.getCurTex();
            }
        }

        /**
        * Used to map rotation to frames. Assumes frames are arranged assending in clockwise order.
        *
        * It divides subdivisions by two so that we can work with atan's mapping to -pi/2 to pi/2 properly.
        * We use halfsubdivs + subdivisions*rotationoffset+.5 so that we can shift the result of atan out of that range and into to the proper quadrant.
        * atan2 gives the angle in the range -pi/2 to pi/2
        * We divide by pi so that we end up with a proportion instead of a radian.
        * 
        * @param direction A vector in the pie slice of the subdivided circle
        * @param subdivisions Range of returned integers
        * @param rotationoffset Rotationoffset is radians/pi for the offset needed to line up the southern sprite.
        *
        * @return returndeschere
        */
        //Needs dir constants
        public static int indexFromDirection(Vector2 direction, int subdivisions, float rotationoffset)
        {
            float halfsubdivs = subdivisions / 2;
            return (int)Math.Floor((halfsubdivs + subdivisions * rotationoffset + .5 + halfsubdivs * Math.Atan2(direction.y, direction.x) / (Math.PI)) % subdivisions);
        }
    }
}
