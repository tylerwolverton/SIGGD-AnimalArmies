/**
 * @file
 * @author PURDUE ACM, SIGGD <siggd.purdue@gmail.com>
 * @version 1.0
 * 
 * @section LICENSE
 * 
 * @section DESCRIPTION
 */

using System;
using System.Collections.Generic;
using Engine;
using System.IO;

namespace Engine
{
    /// brief classdeschere
    /**
    * classdeschere
    */
    public class ActorTool : Tool
    {
        public ScrollingImageTable thumbs;

        public int currentActorIndex = 0;
        public Actor theActor;

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public ActorTool(EditorComponent editor)
            : base(editor)
        {
            ResourceComponent rc = editor.engine.resourceComponent;
            toolButton.unpressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/001_actor.png")));
            toolButton.pressedImg = rc.get(Path.GetFullPath(Path.Combine(editor.editorGui.rootDirectory, "GUI/000_EngineGUI/002_actor.png")));
            editor.editorGui.addToolButton(toolButton, true);

            createDialog();
            discoverThumbnails();

            isDragging = false;
            
            floatingPic = new GUILabel(editor.editorGui);
            floatingPic.tintTex = new Color(1f, 1f, 1f, .5f);
        }

        void createDialog()
        {
            toolDialog = new GUIControl(editor.editorGui);

            //Right menu
            GUILabel background = new GUILabel(editor.editorGui, editor.editorGui.leftBG, text: "Actor Tool");
            background.size = new Vector2(EditorGUI.RIGHTBOUNDARY, editor.engine.graphicsComponent.height  - EditorGUI.BOTTOMBOUNDARY);
            toolDialog.pos = new Vector2(editor.engine.graphicsComponent.width - EditorGUI.RIGHTBOUNDARY, 0);
            toolDialog.add(background);

            int numCols = 64 / 32;
            int numRows = editor.editorGui.graphics.height / 32;
            thumbs = new ScrollingImageTable(editor.editorGui, numRows, numCols, 32, 32, ScrollingImageTable.ScrollDirection.VERTICAL, new Vector2(0, 20));
            thumbs.padding = 4;
            toolDialog.add(thumbs);
        }

        void discoverThumbnails()
        {
            //Get actor sprite directories
            ResourceComponent rc = editor.engine.resourceComponent;
            String contentPath = Path.Combine(ResourceComponent.rootDirectory, "Sprites");
            IEnumerable<string> actorPaths = null;
            try
            {
                actorPaths = Directory.EnumerateDirectories(contentPath);
            }
            catch (Exception e)
            {
                return;
            }

            //Add thumbnail buttons to the ScrollingImageTable
            int i = 0;
            foreach (string s in actorPaths)
            {

                //Get thumbnail location
                String actorThumbnail;
                try{
                    actorThumbnail = Directory.GetFiles(s)[0];
                }catch (Exception e){
                    try{
                        actorThumbnail = Directory.GetFiles(Directory.GetDirectories(s)[0])[0];
                    }catch (Exception x){
                        continue;
                    }
                }
                actorThumbnail = actorThumbnail.Substring(ResourceComponent.rootDirectory.Length + 1);

                //Get Handle
                Handle tempH = rc.get(actorThumbnail);

                //Create Button
                GUIButton tempB = new GUIButton(editor.editorGui, tempH, tempH);
                tempB.size = new Vector2(32, 32);
                
                //Add click event to button
                int actorIndex = i;
                tempB.mouseClickEvent += (pos, button) =>
                {
                    if (active)
                    {
                        currentActorIndex = (byte)actorIndex;
                    }
                };

                //Add the thumbnail
                thumbs.add(tempB);
                i++;
            }
            thumbs.performLayout();
        }

        public override void activate()
        {
            if (editor.engine.world.actorFactory != null)
                theActor = editor.engine.world.actorFactory.createActor(currentActorIndex, Vector2.Zero, Vector2.Zero);

            base.activate();
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public override void downAction(MouseKeyBinding.MouseButton button)
        {
            Vector2 screenPos = editor.engine.inputComponent.getMousePosition(); ;
            Vector2 worldPos = editor.engine.graphicsComponent.camera.screen2World(screenPos);
            
            Tile victim = editor.engine.world.getTileAt(editor.engine.graphicsComponent.camera.screen2World(screenPos));
            if (victim != null)
            {
                Actor p = editor.engine.world.actorFactory.createActor(currentActorIndex, new Vector2(worldPos.x, worldPos.y), new Vector2(0,0));
                editor.engine.world.addActor(p);
                
                Mapfile.ActorData w = new Mapfile.ActorData();
                w.id = (byte)currentActorIndex;
                w.x = worldPos.x;
                w.y = worldPos.y;
                w.z = 0;
                editor.engine.world.file.worldActorData.Add(w);

                undos.Clear();

                toolAction.Push(new ActorToolAction(w, this));
            }
        }

        public override void update()
        {
            Vector2 screenPos = editor.engine.inputComponent.getMousePosition();
            Vector2 worldPos = editor.engine.graphicsComponent.camera.screen2World(screenPos);

            GUIButton actorButton = (thumbs.getItem(currentActorIndex) as GUIButton);
            if (actorButton == null) return;

            Handle imgInd = actorButton.texture;
            Texture2D texture = imgInd.getResource<Texture2D>();
            floatingPic.texture = actorButton.texture;
            floatingPic.size = new Vector2(texture.width, texture.height);
            floatingPic.pos = screenPos + (new Vector2(theActor.xoffset, theActor.yoffset));
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public override void undoAction(ToolAction action)
        {
            Mapfile.ActorData w = (action as ActorToolAction).actor;
            editor.engine.world.file.worldActorData.Remove(w);
        }

        /**
        * desc here
        *
        * @param paramsdeschere
        *
        * @return returndeschere
        */
        public override void redoAction(ToolAction action)
        {
            Mapfile.ActorData w = (action as ActorToolAction).actor;
            editor.engine.world.file.worldActorData.Add(w);
        }

        public class ActorToolAction : ToolAction
        {

            public Mapfile.ActorData actor; ///< 

            /**
            * desc here
            *
            * @param paramsdeschere
            *
            * @return returndeschere
            */
            public ActorToolAction(Mapfile.ActorData actor, Tool parent)
                : base(parent)
            {
                this.actor = actor;
            }
        }
    }
}
