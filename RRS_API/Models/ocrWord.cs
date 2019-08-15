using System;

    public class ocrWord
    {
        private int x;
        private int y;
        private String text;

        public ocrWord(int x, int y, String text)
        {
            this.x = x;
            this.y = y;
            this.text = text;
        }

        public int getX()
        {
            return this.x;
        }

        public int getY()
        {
            return this.y;
        }

        public String getText()
        {
            return this.text;
        }
    }
