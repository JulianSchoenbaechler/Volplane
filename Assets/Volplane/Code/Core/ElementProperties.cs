/*
 * Copyright - Julian Schoenbaechler
 * https://github.com/JulianSchoenbaechler/Volplane
 *
 * This file is part of the Volplane project.
 *
 * The Volplane project is free software: you can redistribute it
 * and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version.
 *
 * The Volplane project is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with the Volplane project.
 * If not, see http://www.gnu.org/licenses/.
 */

namespace Volplane
{
    using Newtonsoft.Json.Linq;
    using System;
    using UnityEngine;

    public class ElementProperties
    {
        protected JObject data;
        protected bool hidden = false;
        protected string[] images = new string[5];
        protected string text = null;
        protected Alignment textAlign;
        protected int[] padding = new int[2];
        protected string font = "Helvetica";
        protected int fontSize = 12;
        protected Color fontColor;

        /// <summary>
        /// Initializes a new instance of the <see cref="Volplane.ElementProperties"/> class.
        /// </summary>
        public ElementProperties()
        {
            data = new JObject();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Volplane.ElementProperties"/> class.
        /// </summary>
        /// <param name="visible">If set to <c>true</c> element will be visible.</param>
        public ElementProperties(bool visible) : base()
        {
            this.Hidden = !visible;
        }

        /// <summary>
        /// Text alignment.
        /// </summary>
        public enum Alignment
        {
            Left,
            Right,
            Center,
            Justify
        }

        /// <summary>
        /// Web font types.
        /// </summary>
        public enum WebFont
        {
            Georgia,
            PalatinoLinotype,
            TimesNewRoman,
            Arial,
            ArialBlack,
            ComicSansMS,
            Helvetica,
            Impact,
            LucidaSansUnicode,
            Tahoma,
            TrebuchetMS,
            Verdana,
            CourierNew,
            LucidaConsole
        }

        /// <summary>
        /// Gets or sets a value indicating whether this element is hidden.
        /// </summary>
        /// <value><c>true</c> if hidden; otherwise, <c>false</c>.</value>
        public bool Hidden
        {
            get { return hidden; }

            set
            {
                if(data["hidden"] != null)
                    data["hidden"] = value;
                else
                    data.Add("hidden", value);
                
                hidden = value;
            }
        }

        /// <summary>
        /// Gets or sets the image of this element.
        /// </summary>
        /// <value>The image.</value>
        public string Image
        {
            get { return images[0]; }

            set
            {
                if(data["image"] != null)
                    data["image"] = String.Format("img/{0:G}", value);
                else
                    data.Add("image", String.Format("img/{0:G}", value));
                
                images[0] = value;
            }
        }

        /// <summary>
        /// Gets or sets the highlight image of this element.
        /// Element must be a button or a swipe field!
        /// </summary>
        /// <value>The highlight image name (including extension).</value>
        public string HighlightImage
        {
            get { return images[1]; }

            set
            {
                if(data["highlightImage"] != null)
                    data["highlightImage"] = String.Format("img/{0:G}", value);
                else
                    data.Add("highlightImage", String.Format("img/{0:G}", value));
                
                images[1] = value;
            }
        }

        /// <summary>
        /// Gets or sets the handler image of this element.
        /// Element must be a relative dpad!
        /// </summary>
        /// <value>The handler image name (including extension).</value>
        public string HandlerImage
        {
            get { return images[1]; }

            set
            {
                if(data["handlerImage"] != null)
                    data["handlerImage"] = String.Format("img/{0:G}", value);
                else
                    data.Add("handlerImage", String.Format("img/{0:G}", value));
                
                images[1] = value;
            }
        }

        /// <summary>
        /// Gets or sets the highlight image (up) of this element.
        /// Element must be a dpad!
        /// </summary>
        /// <value>The highlight image name (including extension).</value>
        public string UpHighlightImage
        {
            get { return images[1]; }

            set
            {
                if(data["upHighlightImage"] != null)
                    data["upHighlightImage"] = String.Format("img/{0:G}", value);
                else
                    data.Add("upHighlightImage", String.Format("img/{0:G}", value));

                images[1] = value;
            }
        }

        /// <summary>
        /// Gets or sets the highlight image (down) of this element.
        /// Element must be a dpad!
        /// </summary>
        /// <value>The highlight image name (including extension).</value>
        public string DownHighlightImage
        {
            get { return images[2]; }

            set
            {
                if(data["downHighlightImage"] != null)
                    data["downHighlightImage"] = String.Format("img/{0:G}", value);
                else
                    data.Add("downHighlightImage", String.Format("img/{0:G}", value));

                images[2] = value;
            }
        }

        /// <summary>
        /// Gets or sets the highlight image (left) of this element.
        /// Element must be a dpad!
        /// </summary>
        /// <value>The highlight image name (including extension).</value>
        public string LeftHighlightImage
        {
            get { return images[3]; }

            set
            {
                if(data["leftHighlightImage"] != null)
                    data["leftHighlightImage"] = String.Format("img/{0:G}", value);
                else
                    data.Add("leftHighlightImage", String.Format("img/{0:G}", value));

                images[3] = value;
            }
        }

        /// <summary>
        /// Gets or sets the highlight image (right) of this element.
        /// Element must be a dpad!
        /// </summary>
        /// <value>The highlight image name (including extension).</value>
        public string RightHighlightImage
        {
            get { return images[4]; }

            set
            {
                if(data["rightHighlightImage"] != null)
                    data["rightHighlightImage"] = String.Format("img/{0:G}", value);
                else
                    data.Add("rightHighlightImage", String.Format("img/{0:G}", value));

                images[4] = value;
            }
        }

        /// <summary>
        /// Gets or sets the stick image of this element.
        /// Element must be a joystick!
        /// </summary>
        /// <value>The stick image name (including extension).</value>
        public string StickImage
        {
            get { return images[1]; }

            set
            {
                if(data["stickImage"] != null)
                    data["stickImage"] = String.Format("img/{0:G}", value);
                else
                    data.Add("stickImage", String.Format("img/{0:G}", value));

                images[1] = value;
            }
        }

        /// <summary>
        /// Gets or sets the thumb image of this element.
        /// Element must be a relative joystick!
        /// </summary>
        /// <value>The thumb image name (including extension).</value>
        public string ThumbImage
        {
            get { return images[2]; }

            set
            {
                if(data["thumbImage"] != null)
                    data["thumbImage"] = String.Format("img/{0:G}", value);
                else
                    data.Add("thumbImage", String.Format("img/{0:G}", value));

                images[2] = value;
            }
        }

        /// <summary>
        /// Gets or sets the text of this element.
        /// </summary>
        /// <value>A text.</value>
        public string Text
        {
            get { return text; }

            set
            {
                if(data["text"] != null)
                    data["text"] = value;
                else
                    data.Add("text", value);

                text = value;
            }
        }

        /// <summary>
        /// Gets or sets the text alignment of this element.
        /// </summary>
        /// <value>The text alignment.</value>
        public Alignment TextAlignment
        {
            get { return textAlign; }

            set
            {
                switch(value)
                {
                    case Alignment.Left:
                        if(data["textAlign"] != null)
                            data["textAlign"] = "left";
                        else
                            data.Add("textAlign", "left");
                        break;

                    case Alignment.Right:
                        if(data["textAlign"] != null)
                            data["textAlign"] = "right";
                        else
                            data.Add("textAlign", "right");
                        break;

                    case Alignment.Justify:
                        if(data["textAlign"] != null)
                            data["textAlign"] = "justify";
                        else
                            data.Add("textAlign", "justify");
                        break;

                    default:
                        if(data["textAlign"] != null)
                            data["textAlign"] = "center";
                        else
                            data.Add("textAlign", "center");
                        break;
                }

                textAlign = value;
            }
        }

        /// <summary>
        /// Gets or sets the vertical padding of the text.
        /// Applies only for text elements!
        /// </summary>
        /// <value>The vertical padding.</value>
        public int PaddingVertical
        {
            get { return padding[0]; }

            set
            {
                if(data["paddingVertical"] != null)
                    data["paddingVertical"] = value;
                else
                    data.Add("paddingVertical", value);

                padding[0] = value;
            }
        }

        /// <summary>
        /// Gets or sets the horizontal padding of the text.
        /// Applies only for text elements!
        /// </summary>
        /// <value>The horizontal padding.</value>
        public int PaddingHorizontal
        {
            get { return padding[1]; }

            set
            {
                if(data["paddingHorizontal"] != null)
                    data["paddingHorizontal"] = value;
                else
                    data.Add("paddingHorizontal", value);

                padding[1] = value;
            }
        }

        /// <summary>
        /// Gets or sets the font used for an elements text.
        /// </summary>
        /// <value>The font name.</value>
        public string Font
        {
            get { return font; }

            set
            {
                if(data["font"] != null)
                    data["font"] = value;
                else
                    data.Add("font", value);

                font = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the font used for an elements text.
        /// </summary>
        /// <value>The size of the font.</value>
        public int FontSize
        {
            get { return fontSize; }

            set
            {
                if(data["fontSize"] != null)
                    data["fontSize"] = value;
                else
                    data.Add("fontSize", value);

                fontSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the the font color used for an elements text.
        /// </summary>
        /// <value>The color of the font.</value>
        public Color FontColor
        {
            get { return fontColor; }

            set
            {
                if(data["fontColor"] != null)
                {
                    data["fontColor"] = String.Format("rgba({0:F0},{1:F0},{2:F0},{3:F0})",
                                                      value.r * 255f,
                                                      value.g * 255f,
                                                      value.b * 255f,
                                                      value.a * 255f);
                }
                else
                {
                    data.Add("fontColor", String.Format("rgba({0:F0},{1:F0},{2:F0},{3:F0})",
                                                        value.r * 255f,
                                                        value.g * 255f,
                                                        value.b * 255f,
                                                        value.a * 255f));
                }

                fontColor = value;
            }
        }

        /// <summary>
        /// Gets the element properties data.
        /// </summary>
        /// <value>The element properties data.</value>
        public string Data
        {
            get { return data.ToString(); }
        }

        /// <summary>
        /// Converts a the web font from this class into a string which can be used for setting
        /// an elements text font.
        /// </summary>
        /// <returns>A font string.</returns>
        /// <param name="font">Web font.</param>
        public string WebFontToString(WebFont font)
        {
            switch(font)
            {
                case WebFont.Georgia:
                    return "Georgia, serif";

                case WebFont.PalatinoLinotype:
                    return "'Palatino Linotype', 'Book Antiqua', Palatino, serif";

                case WebFont.TimesNewRoman:
                    return "'Times New Roman', Times, serif";

                case WebFont.Arial:
                    return "Arial, Helvetica, sans-serif";

                case WebFont.ArialBlack:
                    return "'Arial Black', Gadget, sans-serif";

                case WebFont.ComicSansMS:
                    return "'Comic Sans MS', cursive, sans-serif";

                case WebFont.Impact:
                    return "Impact, Charcoal, sans-serif";

                case WebFont.LucidaSansUnicode:
                    return "'Lucida Sans Unicode', 'Lucida Grande', sans-serif";

                case WebFont.Tahoma:
                    return "Tahoma, Geneva, sans-serif";

                case WebFont.TrebuchetMS:
                    return "'Trebuchet MS', Helvetica, sans-serif";

                case WebFont.Verdana:
                    return "Verdana, Geneva, sans-serif";

                case WebFont.CourierNew:
                    return "'Courier New', Courier, monospace";

                case WebFont.LucidaConsole:
                    return "'Lucida Console', Monaco, monospace";

                default:
                    return "Helvetica, Arial, sans-serif";
            }
        }
    }
}
