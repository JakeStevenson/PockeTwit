using System.Drawing;
using System.Drawing.Imaging;

namespace TiledMaps
{
    public interface IMapOverlay: System.IComparable
    {
        IMapDrawable Drawable
        {
            get;
        }

        Geocode Geocode
        {
            get;
        }

        Point Offset
        {
            get;
        }

    }

    public class MapOverlay : IMapOverlay
    {
        public MapOverlay()
        {
        }

        public int CompareTo(object obj)
        {
            MapOverlay otherOverLay = (MapOverlay)obj;
            return otherOverLay.Geocode.Latitude.CompareTo(this.Geocode.Latitude);
        }

        public MapOverlay(IMapDrawable drawable, Geocode geocode, Point offset)
        {
            myDrawable = drawable;
            myOffset = offset;
            myGeocode = geocode;
        }

        IMapDrawable myDrawable;

        public IMapDrawable Drawable
        {
            get { return myDrawable; }
            set { myDrawable = value; }
        }
        Geocode myGeocode;

        public Geocode Geocode
        {
            get { return myGeocode; }
            set { myGeocode = value; }
        }
        Point myOffset;

        public Point Offset
        {
            get { return myOffset; }
            set { myOffset = value; }
        }
    }
}