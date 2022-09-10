using System;
using System.Text;
//using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Windows.Media;

namespace QuickZip.MiniHtml2
{

    public class CurrentStateType
    {
        private List<HtmlTag> activeStyle = new List<HtmlTag>();
        private bool bold;
        private bool italic;
        private bool underline;
        private bool subscript;
        private bool superscript;
        private string hyperlink = null;

        private string imglink = null;
        private double? imgWidth = null;
        private double? imgHeight = null;

        private string videolink = null;
        private double? videoWidth = null;
        private double? videoHeight = null;

        private Color? foreground;
        private string font = null;
        private double? fontSize;

        private double? baseImgHeight = 100;
        private double? baseImgWidth = 100;

        public bool Bold { get { return bold; } }
        public bool Italic { get { return italic; } }
        public bool Underline { get { return underline; } }
        public bool SubScript { get { return subscript; } }
        public bool SuperScript { get { return superscript; } }
        public string HyperLink { get { return hyperlink; } }

        public string ImgLink { get { return imglink; } }
        public double? ImgWidth { get { return imgWidth; } }
        public double? ImgHeight { get { return imgHeight; } }

        public string VideoLink { get { return videolink; } }
        public double? VideoWidth { get { return videoWidth; } }
        public double? VideoHeight { get { return videoWidth; } }

        public Color? Foreground { get { return foreground; } }
        public string Font { get { return font; } }
        public double? FontSize { get { return fontSize; } }

        public void UpdateStyle(HtmlTag aTag)
        {
            if (!aTag.IsEndTag)
                activeStyle.Add(aTag);
            else
                for (int i = activeStyle.Count - 1; i >= 0; i--)
                    if ('/' + activeStyle[i].Name == aTag.Name)
                    {
                        activeStyle.RemoveAt(i);
                        break;
                    }
            updateStyle();
        }


        private void updateStyle()
        {
            bold = false;
            italic = false;
            underline = false;
            subscript = false;
            superscript = false;
            foreground = null;
            font = null;
            hyperlink = "";
            imglink = "";
            videolink = "";
            imgHeight = null;
            imgWidth = null;
            videoHeight = null;
            videoWidth = null;
            fontSize = null;

            foreach (HtmlTag aTag in activeStyle)
                switch (aTag.Name)
                {
                    case "b": bold = true; break;
                    case "i": italic = true; break;
                    case "u": underline = true; break;
                    case "sub": subscript = true; break;
                    case "sup": superscript = true; break;
                    case "a": if (aTag.Contains("href")) hyperlink = aTag["href"]; break;
                    case "img":
                        if (aTag.Contains("href"))
                            imglink = aTag["href"];
                        if(aTag.Contains("width"))
                            try { imgWidth = Double.Parse(aTag["width"]); }
                            catch { imgWidth = baseImgWidth;}
                        else
                        {
                            imgWidth = baseImgWidth;
                        }
                        if (aTag.Contains("height"))
                            try { imgHeight = Double.Parse(aTag["height"]); }
                            catch { imgHeight = baseImgHeight; }
                        else
                        {
                            imgHeight = baseImgHeight;
                        }
                        break;

                    case "font" :
                        if (aTag.Contains("color"))
                            try { foreground = (Color)ColorConverter.ConvertFromString(aTag["color"]); }
                            catch { foreground = Colors.White; }
                        if (aTag.Contains("face"))
                            font = aTag["face"];
                        if (aTag.Contains("size"))
                            try { fontSize= Double.Parse(aTag["size"]); }
                            catch { };
                        break;
                }
        }

        public CurrentStateType()
        {

        }



    }
	
}
/*
                     case "video":
                        if (aTag.Contains("href"))
                            videolink = aTag["href"];
                        if (aTag.Contains("width"))
                            try { videoWidth = Double.Parse(aTag["width"]); }
                            catch { videoWidth = baseImgWidth; }
                        else
                        {
                            videoWidth = baseImgWidth;
                        }
                        if (aTag.Contains("height"))
                            try { videoHeight = Double.Parse(aTag["height"]); }
                            catch { videoHeight = baseImgHeight; }
                        else
                        {
                            videoHeight = baseImgHeight;
                        }
                        break;*/