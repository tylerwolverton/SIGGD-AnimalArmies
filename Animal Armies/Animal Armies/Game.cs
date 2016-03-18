using System;
using System.Collections.Generic;
using Engine;
using Tao.Sdl;

namespace Game
{
    public class Game : MirrorEngine
    {

        private const string WINDOWTITLE = "Animal Armies";
        /*
        public new GameResources resourceComponent
        {
            get
            {
                return base.resourceComponent as GameResources;
            }
            set
            {
                base.resourceComponent = value;
            }
        }*/

        public new GameInput inputComponent
        {
            get
            {
                return base.inputComponent as GameInput;
            }
            set
            {
                base.inputComponent = value;
            }
        }

        public new GamePhysics physicsComponent
        {
            get
            {
                return base.physicsComponent as GamePhysics;
            }
            set
            {
                base.physicsComponent = value;
            }
        }

        public new GameGraphics graphicsComponent
        {
            get
            {
                return base.graphicsComponent as GameGraphics;
            }
            set
            {
                base.graphicsComponent = value;
            }
        }
        /*
        public new GameAudio audioComponent
        {
            get
            {
                return base.audioComponent as GameAudio;
            }
            set
            {
                base.audioComponent = value;
            }
        }*/

        public Game()
            : base(WINDOWTITLE)
        {
            // NOTE: This is the order of the game loop as well
            //resourceComponent = new GameResources(this);
            inputComponent = new GameInput(this);
            physicsComponent = new GamePhysics(this);
            graphicsComponent = new GameGraphics(this);
            //audioComponent = new GameAudio(this);
            graphicsComponent.gui = new GameGUI(graphicsComponent);
            //editorComponent = new EditorComponent(this);
			Tile.size = 32;
        }

        protected override void initialize()
        {
            base.initialize();
            setWorld("Maps/Menu.map");
        }

        public override World constructGameWorld(Mapfile path)
        {
            return new GameWorld(this, path);
        }



        protected override void loadContent()
        {
            base.loadContent();
        }

        protected override void unloadContent()
        {
            base.unloadContent();
        }
    }
}
