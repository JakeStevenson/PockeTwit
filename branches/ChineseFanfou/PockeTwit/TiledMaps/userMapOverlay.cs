using System;
using System.Drawing;
using TiledMaps;
using System.Collections.Generic;
using System.Text;

namespace PockeTwit
{
    class userMapOverlay : TiledMaps.IMapOverlay
    {
        private userMapDrawable _drawable;
        private Geocode _geocode;
        private Library.User _user;
        private Point _offset;
        public userMapOverlay(IMapDrawable drawable, Geocode geocode, Library.User user)
        {
            _drawable = (userMapDrawable)drawable;
            _geocode = geocode;
            _user = user;
            _offset = new Point((ClientSettings.SmallArtSize + (ClientSettings.Margin * 2) / 2), (ClientSettings.SmallArtSize + (ClientSettings.Margin * 2)));
        }
        #region IMapOverlay Members

        public IMapDrawable Drawable
        {
            get { return _drawable; }
        }

        public Geocode Geocode
        {
            get { return _geocode; }
        }

        public Point Offset
        {
            get { return _offset; }
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            userMapOverlay otherOverLay = (userMapOverlay)obj;
            return otherOverLay.Geocode.Latitude.CompareTo(this.Geocode.Latitude);
        }

        #endregion
    }
}
