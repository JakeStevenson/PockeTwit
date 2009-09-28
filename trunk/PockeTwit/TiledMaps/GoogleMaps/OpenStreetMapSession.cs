using System.Threading;
using System.Net;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using System;

namespace TiledMaps
{
    public class OpenStreetMapSession : HttpMapSession
    {
        protected override Uri GetUriForKey(Key key)
        {
            return new Uri(String.Format("http://tile.openstreetmap.org/{2}/{0}/{1}.png", key.X, key.Y, key.Zoom));
        }
    }
}
