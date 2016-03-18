/**
 * @file EditorComponent.cs
 * @author SIGGD, Purdue ACM <siggd.purdue@gmail.com>
 * @version 2.0
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public class EditorComponent : Component
    {
        public bool isEditing = true; //Determines whether the editor is currently editing

        //Settings
        public bool showGrid = true;        //Determines whether the grid should be displayed
        public bool showOverlay = true;     //Determines whether the remaining overlay should be displayed
        public bool fullAmbient = true;     //Determines whether the World's ambient lighting should be overridden
        private bool _pauseEngine = true;   //Determines whether the editor should run the game simultaneously or not
        public bool pauseEngine
        {
            get
            {
                return _pauseEngine;
            }
            set
            {
                _pauseEngine = value;
                engine.paused = _pauseEngine;
            }
        }

        //Zoom constants
        private const int ZOOMMIN =     0;
        public const int  ZOOMDEFAULT = 3;
        private const int ZOOMMAX =     5;

        //Gui
        public EditorGUI editorGui = null;  //The editor's Gui
        public GUI gameGui = null;          //The game's Gui
        private Vector2 lastPos;            //the position of the camera after the last switch

        //Tools
        public WorldSettingsTool worldSettingsTool;
        public Tool activeTool;
        public PencilTool pencilTool;
        public FillTool fillTool;
        public SelectionTool selectionTool;
        public ActorTool actorTool;
        public EraserTool eraseTool;

        public NewMapTool newMapTool;
        public LoadMapTool loadMapTool;
        public SaveMapTool saveMaptool;
        public ResetTool resetTool;
        public EditorSettingsTool editorSettingsTool;

        //Current brushes and tiles
        public Mapfile.TileData currentValues;  //The current values selected by the PencilTool
        public Mapfile.TileData currentTile;    //The current brush of the PencilTool, with ignore settings factored in
        public Tile curTile;                    //The Tile under the mouse
        public Tile prevTile;                   //The previous Tile under the mouse

        /* 
         * Constructor. Instantiates the current brushes.
         */ 
        public EditorComponent(MirrorEngine engine)
            : base(engine)
        {
            lastPos = new Vector2(0, 0);
            
            currentValues = new Mapfile.TileData("");
            currentTile.setToIgnore();
            currentTile.texture = currentValues.texture;
            
            isActive = false;
        }

        /*
         * Initializes the gui and the input bindings.
         */ 
        public override void initialize()
        {
            //Initialize the gui
            editorGui = new EditorGUI(engine.graphicsComponent);
            editorGui.initialize();

            //Initialize the input
            engine.inputComponent.setContext(typeof(InputComponent.EditorBindings), this);
            (engine.inputComponent[InputComponent.GameStateBindings.SWAP] as SinglePressBinding).downEvent += swap;
            (engine.inputComponent[InputComponent.EditorBindings.MOUSEBUTTON] as MouseKeyBinding).mouseKeyDown += mouseDownHandler;
            (engine.inputComponent[InputComponent.EditorBindings.MOUSEBUTTON] as MouseKeyBinding).mouseKeyUp += mouseUpHandler;
            (engine.inputComponent[InputComponent.EditorBindings.PLUS] as SinglePressBinding).downEvent += () => { zoomMouse++; };
            (engine.inputComponent[InputComponent.EditorBindings.MINUS] as SinglePressBinding).downEvent += () => { zoomMouse--; };

            /* Disabled on 4/14/13 by Dan Roberts because every time we hit enter the camera would center itself, even outside the editor. */
            //(engine.inputComponent[InputComponent.EditorBindings.CENTER] as SinglePressBinding).downEvent += center;

            //(engine.inputComponent[InputComponent.EditorBindings.ACTOR] as SinglePressBinding).downEvent += () => { editorGui.toolClick(actorTool, editorGui.actorToolButton); };
            //(engine.inputComponent[InputComponent.EditorBindings.PENCIL] as SinglePressBinding).downEvent += () => { editorGui.toolClick(pencilTool, editorGui.pencilToolButton); };
            //(engine.inputComponent[InputComponent.EditorBindings.FILL] as SinglePressBinding).downEvent += () => { editorGui.toolClick(fillTool, editorGui.fillToolButton); };
        }

        /*
         * Initializes all tools in the order they appear.
         * Selects the pencilTool as the initial tool.
         */ 
        public override void loadContent()
        {
            //Order they are added to the gui, top down
            worldSettingsTool = new WorldSettingsTool(this);
            editorGui.addToolButton(null, true);
            pencilTool = new PencilTool(this);
            fillTool = new FillTool(this);
            selectionTool = new SelectionTool(this);
            editorGui.addToolButton(null, true);
            actorTool = new ActorTool(this);
            eraseTool = new EraserTool(this);
            
            //bottom up
            editorSettingsTool = new EditorSettingsTool(this);
            saveMaptool = new SaveMapTool(this);
            loadMapTool = new LoadMapTool(this);
            newMapTool = new NewMapTool(this);
            resetTool = new ResetTool(this);

            //Default menus
            selectTool(pencilTool);
        }

        /*
         * Toggles whether the editor is active
         */ 
        public void swap()
        {
            if (gameGui == null)
            {
                switchTo();
            }
            else
            {
                switchFrom();
            }
        }

        /*
         * Switches to the editor component.
         * Updates camera position and sets the gui to the editorgui
         */ 
        private void switchTo()
        {
            //Update camera position
            if (lastPos.Equals(new Vector2(0, 0)))
            {
                lastPos = engine.graphicsComponent.camera.position;
            }
            engine.graphicsComponent.camera.position = lastPos;

            //Set gui
            gameGui = engine.graphicsComponent.gui;
            engine.graphicsComponent.switchGUI(editorGui);

            engine.paused = true;
            isActive = true;
        }

        /*
         * Switches away from the editor.
         * Sets lastPos, sets the gui to the gameGui, fixes the zoom, and updates the pausecheckbox.
         */ 
        private void switchFrom()
        {
            lastPos = engine.graphicsComponent.camera.position;

            //Set Gui
            engine.graphicsComponent.switchGUI(gameGui);
            gameGui = null;

            //Fix zoom
            zoomCenter = ZOOMDEFAULT;

            //Update pausecheckbox
            if(!editorSettingsTool.pauseCheckBox.isDown)
                editorSettingsTool.pauseCheckBox.toggle();
            engine.paused = false;
            isActive = false;
        }

        /*
         * Selects a tool to use
         * 
         * @param tool the tool to use
         */ 
        public void selectTool(Tool tool)
        {
            tool.activate();
        }

        /*
         * Updates the editor component
         */ 
        public void Update()
        {
            if (isActive)
            {
                //update camera
                engine.graphicsComponent.camera.position += new Vector2(4 * (engine.inputComponent[InputComponent.EditorBindings.XMOVE] as AxisBinding).position, -3 * (engine.inputComponent[InputComponent.EditorBindings.YMOVE] as AxisBinding).position);

                //sexy fade-in
                foreach (Actor a in engine.world.actors) a.tint.a += .02f;

                //Update active tool
                activeTool.update();

                //Call move action if on new tile
                Vector2 mousePos = engine.inputComponent.getMousePosition();
                curTile = engine.world.getTileAt(engine.graphicsComponent.camera.screen2World(mousePos));
                if (curTile != prevTile)
                {
                    prevTile = curTile;
                    activeTool.moveAction(mousePos);
                }
            }
        }

        /*
         * Handles a mouse down action
         */ 
        internal void mouseDownHandler(MouseKeyBinding.MouseButton button)
        {
            //Update floating pics (could be refactored)
            if (activeTool == actorTool)
                actorTool.floatingPic.pos = new Vector2(engine.graphicsComponent.width, engine.graphicsComponent.height);
            if (activeTool == eraseTool)
                eraseTool.floatingPic.pos = new Vector2(engine.graphicsComponent.width, engine.graphicsComponent.height);

            //Get the mouse position
            Vector2 screenPos = engine.inputComponent.getMousePosition();

            if (isActive && isEditing && editorGui.getItemAt(screenPos) == null)
            {
                //Fix floating pics
                if (activeTool == actorTool)
                    actorTool.floatingPic.pos = screenPos + (new Vector2(actorTool.theActor.xoffset, actorTool.theActor.yoffset));
                if (activeTool == eraseTool)
                    eraseTool.floatingPic.pos = new Vector2(screenPos.x - eraseTool.floatingPic.size.x / 2, screenPos.y - eraseTool.floatingPic.size.y / 2);
                
                //Notify active tool
                activeTool.downAction(button);
            }
        }

        /*
         * Handles a mouse up action
         */ 
        internal void mouseUpHandler(MouseKeyBinding.MouseButton button)
        {
            //Get the mouse position
            Vector2 screenPos = engine.inputComponent.getMousePosition();
            
            if (isActive && isEditing && editorGui.getItemAt(screenPos) == null)
            {
                //Notify active tool
                activeTool.upAction(button);
            }
        }

        /*
         * Centers the camera to the world
         */ 
        public void center()
        {
            engine.graphicsComponent.camera.centerToWorld();
        }

        //Perform zooming by setting the zoom property
        private int _zoom = ZOOMDEFAULT; //The current zoom value
        
        public int zoomMouse //Used to increment or decrement the zoom using the mouse as the focal point
        {
            get { return _zoom; }
            set
            {
                //value checks
                if (value > ZOOMMAX) value = ZOOMMAX;
                else if (value < ZOOMMIN) value = ZOOMMIN;
                if (value == _zoom) return;

                //Get initial mouse in world coords and then center to it
                Vector2 mousePos = engine.graphicsComponent.camera.screen2World(engine.inputComponent.getMousePosition());
                engine.graphicsComponent.camera.centerToMouse();

                //Calculate zoom factor and perform the scaling
                float factor = (float)Math.Pow(2, value - _zoom);
                engine.graphicsComponent.camera.performScale(factor);

                //Get the new mouse in world coords and shift by the difference from original
                Vector2 newMousePos = engine.graphicsComponent.camera.screen2World(engine.inputComponent.getMousePosition());
                engine.graphicsComponent.camera.position -= newMousePos - mousePos;

                _zoom = value;
            }
        }

        public int zoomCenter //Used to increment or decrement the zoom using the center of the screen as the focal point
        {
            get { return _zoom; }
            set
            {
                //value checks
                if (value > ZOOMMAX) value = ZOOMMAX;
                else if (value < ZOOMMIN) value = ZOOMMIN;
                if (value == _zoom) return;

                //Calculate zoom factor and perform the scaling
                float factor = (float)Math.Pow(2, value - _zoom);
                engine.graphicsComponent.camera.performScale(factor);

                _zoom = value;
            }
        }
    }
}
