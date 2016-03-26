using Engine;
using Game;

namespace Game
{
    public class GameGUI : Engine.GUI
    {
        GUIList testList; 

        public GameGUI(GameGraphics graphics)
            : base(graphics)
        {
           
           testList = new GUIList();
           testList.align = GUIList.Align.RIGHT;
            
        }

        public override void loadContent()
        {
            base.loadContent();

            // Cannot layout until textures and fonts are loaded (for size)
            //testList.performLayout();  // Layout the gui items
            testList.performLayout(new Vector2(100, 100));
        }
    }
}