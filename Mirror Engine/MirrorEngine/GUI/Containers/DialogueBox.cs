using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public class DialogueBox : GUIContainer
    {
        public GUIButton confirm;
        public GUIButton cancel;
        public Vector2 position;
        public Vector2 size;
        public GUILabel prompt;
        public GUITextBox rebuttal;
        public GUIButton background;

        public DialogueBox(GUI gooey, string prom, Vector2 pos) : base()
        {
            size = new Vector2(gooey.graphics.width * .20f, gooey.graphics.height * .10f);
            position = pos;
            
            background.pos = pos;
            background.size = size;
            
        }
        public override void performLayout(Vector2 asdasd)
        {

        }
    }
}
