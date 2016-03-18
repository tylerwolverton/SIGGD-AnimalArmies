using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{

    /*
     * Represents a textbox in the GUI
     */ 
    public class GUITextBox : GUILabel
    {

        public Color focusedColor;
        public Color blurredColor;
        public float minWidth = 100f;
        public float maxWidth = 100f;

        //Delay
        private char lastChar;
        private const int INITIALHOLDDELAY = 15;
        private const int HOLDDELAY = 2;
        private int delay;

        //Sets the initial text, and initializes members
        public GUITextBox(GUI gui, string text = "") :
            base(gui, text: text)
        {
            focusedColor = new Color(1f, .8f, 1f);
            blurredColor = Color.WHITE;
            bgColor = blurredColor;
            textColor = new Color(0, 0, 0);
            lastChar = '\0';
            delay = INITIALHOLDDELAY;
            this.pos = Vector2.Zero;
        }

        //Edits the current text based on the last-pressed key.
        public void update()
        {

            if (lastChar == '\0') return;
            if (delay > 0) return;
            delay = HOLDDELAY;

            if (lastChar == '\b')
            {
                if (text.Length > 0)
                    text = text.Remove(text.Length - 1);
            }
            else text = text + lastChar;

            size = Font.calcTextSize(font, text, fontSize);
            if (size.x < minWidth) size = new Vector2(minWidth, size.y);
            if (size.x > maxWidth) size = new Vector2(maxWidth, size.y);
        }
        
        //Sets the bgColor to focusedColor.
        public override void onFocus()
        {
            bgColor = focusedColor;
        }

        //Sets the bgColor to blurredColor. Stops update from adding the last-pressed key.
        public override void onBlur()
        {
            bgColor = blurredColor;
            lastChar = '\0';
        }

        //Adds the character to text and allows update to continue adding the last-pressed key.
        public override void onText(char c)
        {

            lastChar = c;
            delay = 0;
            update();
            delay = INITIALHOLDDELAY;
        }

        //Stops update from adding the last-pressed key.
        public override void offText(char c)
        {
            if (c == lastChar)
                lastChar = '\0';
        }
    }
}