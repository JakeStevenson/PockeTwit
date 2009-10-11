using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace PockeTwit
{
    class userMapDrawable : TiledMaps.IGraphicsDrawable
    {
        public Library.User userToDraw;
        public float fSize = 9;

        public int charToUse = -1;
        public bool IsOpened = false;
        public Bitmap markerImage = null;

        private Brush B = new SolidBrush(Color.Black);
        #region IGraphicsDrawable Members
        public Rectangle Location = new Rectangle();

        public void Draw(System.Drawing.Graphics graphics, System.Drawing.Rectangle destRect, System.Drawing.Rectangle sourceRect)
        {
            if (IsOpened)
            {
                using (Brush b= new SolidBrush(Color.Red))
                {

                    graphics.FillPolygon(b, new Point[]{
                        new Point(destRect.Left + (destRect.Width/2)+ClientSettings.Margin, destRect.Bottom-(ClientSettings.Margin*4)),
                        new Point(destRect.Left + (destRect.Width/2), destRect.Bottom),
                        new Point(destRect.Left + (destRect.Width/2)-ClientSettings.Margin, destRect.Bottom-(ClientSettings.Margin*4)),
                    });
                    using (Image userbmp = ThrottledArtGrabber.GetArt(this.userToDraw.profile_image_url))
                    {
                        graphics.DrawImage(userbmp, destRect.X, destRect.Y);
                    }
                }
            }
            else
            {
                TiledMaps.IGraphicsDrawable graphicsDrawable = ThrottledArtGrabber.mapMarkerImage as TiledMaps.IGraphicsDrawable;
                graphicsDrawable.Draw(graphics, destRect, sourceRect);
                Rectangle CharRect = new Rectangle(destRect.X + 7, destRect.Y+2, destRect.Width - 6, destRect.Height-2);
                if (charToUse >= 0)
                {
                    using (Font f = new Font(FontFamily.GenericSansSerif, fSize, FontStyle.Regular))
                    {
                        graphics.DrawString(charToUse.ToString(), f, B, CharRect);
                    }
                }
            }
            Location = destRect;
        }

        #endregion

        #region IMapDrawable Members

        public int Width
        {
            get 
            {
                if (IsOpened)
                {
                    return ClientSettings.SmallArtSize;
                }
                else
                {
                    return ThrottledArtGrabber.mapMarkerImage.Width;
                }
            }
        }

        public int Height
        {
            get 
            {
                if (IsOpened)
                {
                    return ClientSettings.SmallArtSize + (ClientSettings.Margin * 4);
                }
                else
                {
                    return ThrottledArtGrabber.mapMarkerImage.Height;
                }
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            
        }

        #endregion
    }
}
