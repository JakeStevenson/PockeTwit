using System;
using System.Drawing;

using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using PockeTwit.FingerUI;

namespace FingerUI
{
    public class LowMemoryException : Exception { }

    class Portal : System.Windows.Forms.Control
    {
        #region GDI Imports
        [DllImport("coredll.dll")]
        static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        public enum TernaryRasterOperations : uint
        {
            /// <summary>dest = source</summary>
            SRCCOPY = 0x00CC0020,
            /// <summary>dest = source OR dest</summary>
            SRCPAINT = 0x00EE0086,
            /// <summary>dest = source AND dest</summary>
            SRCAND = 0x008800C6,
            /// <summary>dest = source XOR dest</summary>
            SRCINVERT = 0x00660046,
            /// <summary>dest = source AND (NOT dest)</summary>
            SRCERASE = 0x00440328,
            /// <summary>dest = (NOT source)</summary>
            NOTSRCCOPY = 0x00330008,
            /// <summary>dest = (NOT src) AND (NOT dest)</summary>
            NOTSRCERASE = 0x001100A6,
            /// <summary>dest = (source AND pattern)</summary>
            MERGECOPY = 0x00C000CA,
            /// <summary>dest = (NOT source) OR dest</summary>
            MERGEPAINT = 0x00BB0226,
            /// <summary>dest = pattern</summary>
            PATCOPY = 0x00F00021,
            /// <summary>dest = DPSnoo</summary>
            PATPAINT = 0x00FB0A09,
            /// <summary>dest = pattern XOR dest</summary>
            PATINVERT = 0x005A0049,
            /// <summary>dest = (NOT dest)</summary>
            DSTINVERT = 0x00550009,
            /// <summary>dest = BLACK</summary>
            BLACKNESS = 0x00000042,
            /// <summary>dest = WHITE</summary>
            WHITENESS = 0x00FF0062
        }
        #endregion

        private bool useDDB = !ClientSettings.UseDIB;
        public delegate void delNewImage();
        public event delNewImage NewImage = delegate { };
        public event delNewImage Panic = delegate { };
        public delegate void delProgress(int itemnumber, int totalnumber);
        public event delProgress Progress = delegate{ };
        
        public int WindowOffset;
        private Bitmap _Rendered;
        public Graphics _RenderedGraphics;
        public Bitmap Rendered
        {
            get
            {
                return _Rendered;
            }
        }

        private List<IDisplayItem> Items = new List<IDisplayItem>();
        public int MaxItems = 11;
        private int _BitmapHeight = 0;
        public int BitmapHeight
        {
            get { return _BitmapHeight; }
        }
        
        private int maxWidth = 0;
        public Portal()
        {
            SetBufferSize();
            PockeTwit.ThrottledArtGrabber.NewArtWasDownloaded += new PockeTwit.ThrottledArtGrabber.ArtIsReady(ThrottledArtGrabber_NewArtWasDownloaded);
        }

        private void SetBufferSize()
        {
            
            Rectangle Screen = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            if (Screen.Width > Screen.Height)
            {
                maxWidth = Screen.Width;
            }
            else
            {
                maxWidth = Screen.Height;
            }
            if (useDDB)
            {
                //Try to create temporary bitmaps for everything we'll need so we can try it out.
                Bitmap TestMap = null;

                Bitmap ScreenMap = new Bitmap(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                                              System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
                Bitmap AvatarMap = new Bitmap(ClientSettings.SmallArtSize, ClientSettings.SmallArtSize);
                try
                {
                    TestMap = new Bitmap(maxWidth, MaxItems*ClientSettings.ItemHeight);
                }
                catch (OutOfMemoryException)
                {
                    ClientSettings.UseDIB = true;
                    ClientSettings.SaveSettings();
                    if (TestMap != null)
                    {
                        TestMap.Dispose();
                    }
                    useDDB = false;
                    try
                    {
                        TestMap = GraphicsLibs.DIB.CreateDIB(maxWidth, MaxItems*ClientSettings.ItemHeight);
                    }
                    catch (OutOfMemoryException)
                    {
                        throw;
                    }
                }
                finally
                {
                    if (TestMap != null)
                    {
                        TestMap.Dispose();
                    }
                }
                ScreenMap.Dispose();
                AvatarMap.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            
            _BitmapHeight = MaxItems * ClientSettings.ItemHeight;
            _Rendered = useDDB ? new Bitmap(maxWidth, _BitmapHeight) : GraphicsLibs.DIB.CreateDIB(maxWidth, _BitmapHeight);
            _RenderedGraphics = Graphics.FromImage(_Rendered);
            
        }

        delegate void delNewArt(string url);
        void ThrottledArtGrabber_NewArtWasDownloaded(string url)
        {
            //Don't bother if it's in the middle of rendering
            try
            {
                lock (Items)
                {
                    try
                    {
                        for (int i = 0; i < Items.Count; i++)
                        {
                            StatusItem s = (StatusItem)Items[i];
                            if (s.Tweet.user.profile_image_url != null)
                            {
                                if (s.Tweet.user.profile_image_url == url)
                                {
                                    Rectangle itemBounds = new Rectangle(0, ClientSettings.ItemHeight * i, s.Bounds.Width, ClientSettings.ItemHeight);
                                    s.RenderAvatarArea(_RenderedGraphics, itemBounds);
                                }
                            }
                        }
                        NewImage();
                    }
                    
                    catch (Exception)
                    {
                        //What happened here?
                        //System.Windows.Forms.MessageBox.Show(ex.Message);
                    }
                    
                }
            }
            catch
            {
            }
        }

        public void SetItemList(List<IDisplayItem> SetOfItems)
        {
            IDisplayItem FirstNewItem = SetOfItems[0];
            int SpacesMoved = 0;
            lock (Items)
            {
                if (Items.Contains(FirstNewItem) && SetOfItems.Count > SpacesMoved)
                {
                    try
                    {
                        //Items added to the end
                        SpacesMoved = Items.IndexOf(FirstNewItem);
                        IDisplayItem[] ItemsToAdd = new IDisplayItem[SpacesMoved];
                        Array.Copy(SetOfItems.ToArray(), SetOfItems.Count - SpacesMoved, ItemsToAdd, 0, SpacesMoved);
                        System.Diagnostics.Debug.WriteLine("Blitting " + SpacesMoved + " to the end of the image.");
                        AddItemsToEnd(ItemsToAdd);
                        return;
                    }
                    catch (ArgumentOutOfRangeException) { }
                }
                else
                {
                    try
                    {
                        IDisplayItem LastNewItem = SetOfItems[SetOfItems.Count - 1];
                        if (Items.Contains(LastNewItem))
                        {
                            //Items added to the start
                            SpacesMoved = MaxItems - (Items.IndexOf(LastNewItem) + 1);
                            IDisplayItem[] ItemsToAdd = new IDisplayItem[SpacesMoved];
                            Array.Copy(SetOfItems.ToArray(), 0, ItemsToAdd, 0, SpacesMoved);
                            System.Diagnostics.Debug.WriteLine("Blitting " + SpacesMoved + " to the start of the image.");
                            AddItemsToStart(ItemsToAdd);
                            return;
                        }
                    }
                    catch (ArgumentOutOfRangeException) { }
                }
                System.Diagnostics.Debug.WriteLine("Jumped " + SpacesMoved + " spaces");
                Items.Clear();
                Items = new List<IDisplayItem>(SetOfItems);
                if (Items.Count > MaxItems)
                {
                    Items.RemoveRange(MaxItems, Items.Count - MaxItems);
                }
            }
            Rerender();
        }

        public void Clear()
        {
            lock (Items)
            {
                Items.Clear();
            }
            _RenderedGraphics.Clear(ClientSettings.BackColor);
        }

        public void AddItemsToStart(IDisplayItem[] Items)
        {
            lock (Items)
            {
                for (int i = Items.Length - 1; i >= 0; i--)
                {
                    AddItemToStart(Items[i]);
                }
                NewImage();
            }
        }
        public void AddItemToStart(IDisplayItem Item)
        {
            lock (Items)
            {
                Items.Insert(0, Item);
                if (Items.Count > MaxItems)
                {
                    Items.RemoveAt(Items.Count - 1);
                    Items.TrimExcess();
                    RenderNewItemAtStart();
                }
            }
        }
        public void AddItemsToEnd(IDisplayItem[] Items)
        {
            lock (Items)
            {
                foreach (IDisplayItem Item in Items)
                {
                    AddItemToEnd(Item);
                }
            }
            NewImage();
        }
        public void AddItemToEnd(IDisplayItem Item)
        {
            lock (Items)
            {
                Items.Add(Item);
                if (Items.Count > MaxItems)
                {
                    Items.RemoveAt(0);
                    Items.TrimExcess();
                    RenderNewItemAtEnd();
                }
            }
        }

        public void ReRenderItem(IDisplayItem Item)
        {
            try
            {
                lock (Items)
                {
                    if (Items.Contains(Item))
                    {
                        int i = Items.IndexOf(Item);
                        Rectangle itemBounds = new Rectangle(0, ClientSettings.ItemHeight * i, Item.Bounds.Width, ClientSettings.ItemHeight);
                        Item.Render(_RenderedGraphics, itemBounds);
                    }
                }
            }
            catch (Exception)
            {
                //System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        public void Rerender()
        {
            RenderBackgroundHighPriority(null);
        }
        
        private void RenderBackgroundHighPriority(object state)
        {
            System.Diagnostics.Debug.WriteLine("RenderBackground called");
            Render(System.Threading.ThreadPriority.Highest);
        }
        
        private void Render(System.Threading.ThreadPriority p)
        {
            try
            {
                lock (Items)
                {
                    try
                    {
                        int itemsDrawn = 1;
                        int StartItem = Math.Max(WindowOffset/ClientSettings.ItemHeight, 0);
                        int EndItem = StartItem + 4;
                        if (EndItem > Items.Count)
                        {
                            EndItem = Items.Count-1;
                            StartItem = Math.Max(EndItem - 4, 0);
                        }
                        System.Diagnostics.Debug.WriteLine("Prioritize items " + StartItem + " to " + EndItem);
                        // Onscreen items first
                        for (int item = StartItem; item < EndItem; item++)
                        {
                            DrawSingleItem(item, _RenderedGraphics);
                            itemsDrawn = reportProgress(itemsDrawn);
                        }

                        for (int i = 0; i < StartItem; i++)
                        {
                            DrawSingleItem(i, _RenderedGraphics);
                            itemsDrawn = reportProgress(itemsDrawn);
                        }
                        for (int i = EndItem; i < Items.Count; i++)
                        {
                            DrawSingleItem(i, _RenderedGraphics);
                            itemsDrawn = reportProgress(itemsDrawn);
                        }
                    }
                    catch (Exception)
                    {
                        //JeepNaked's error here?
                        //Specified argument was out of range of valid values. Parameter name: index
                        //System.Windows.Forms.MessageBox.Show(ex.Message);
                    }
                    NewImage();
                }
            }
            catch(OutOfMemoryException)
            {
                PanicMode();
                Rerender();
                //throw new LowMemoryException();
            }
        }

        private int reportProgress(int itemsDrawn)
        {
            Progress(itemsDrawn, Items.Count);
            itemsDrawn++;
            return itemsDrawn;
        }

        private void PanicMode()
        {
            _RenderedGraphics.Dispose();
            _Rendered.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            SetBufferSize();
            Panic();
        }

        private void DrawSingleItem(int i, Graphics g)
        {
            lock (Items)
            {
                IDisplayItem Item = Items[i];
                Rectangle ItemBounds = new Rectangle(0, i*ClientSettings.ItemHeight, Item.Bounds.Width,
                                                     ClientSettings.ItemHeight);
                using (Pen whitePen = new Pen(ClientSettings.LineColor))
                {
                    g.DrawLine(whitePen, ItemBounds.Left, ItemBounds.Top, ItemBounds.Right, ItemBounds.Top);
                    g.DrawLine(whitePen, ItemBounds.Left, ItemBounds.Bottom, ItemBounds.Right, ItemBounds.Bottom);
                    g.DrawLine(whitePen, ItemBounds.Right, ItemBounds.Top, ItemBounds.Right, ItemBounds.Bottom);
                }
                Item.Render(g, ItemBounds);
            }
        }
        private void RenderNewItemAtStart()
        {
            lock (Items)
            {
                //Copy all but last item from top down one.
                IntPtr gPtr = _RenderedGraphics.GetHdc();

                BitBlt(gPtr, 0, ClientSettings.ItemHeight, _Rendered.Width, _Rendered.Height - ClientSettings.ItemHeight, gPtr, 0, 0, TernaryRasterOperations.SRCCOPY);
                _RenderedGraphics.ReleaseHdc(gPtr);
                //Draw the first item.
                IDisplayItem Item = Items[0];
                Rectangle ItemBounds = new Rectangle(0, 0, Item.Bounds.Width, Item.Bounds.Height);
                using (Pen whitePen = new Pen(ClientSettings.LineColor))
                {
                    _RenderedGraphics.DrawLine(whitePen, ItemBounds.Left, ItemBounds.Top, ItemBounds.Right, ItemBounds.Top);
                    _RenderedGraphics.DrawLine(whitePen, ItemBounds.Left, ItemBounds.Bottom, ItemBounds.Right, ItemBounds.Bottom);
                    _RenderedGraphics.DrawLine(whitePen, ItemBounds.Right, ItemBounds.Top, ItemBounds.Right, ItemBounds.Bottom);
                }
                Item.Render(_RenderedGraphics, ItemBounds);
            }
        }
        private void RenderNewItemAtEnd()
        {
            lock (Items)
            {
                //Copy all but first item from top up one.
                IntPtr gPtr = _RenderedGraphics.GetHdc();
                BitBlt(gPtr, 0, 0, _Rendered.Width, _Rendered.Height - ClientSettings.ItemHeight, gPtr, 0, ClientSettings.ItemHeight, TernaryRasterOperations.SRCCOPY);
                _RenderedGraphics.ReleaseHdc(gPtr);
                //Draw the last item.
                IDisplayItem Item = Items[Items.Count - 1];
                Rectangle ItemBounds = new Rectangle(0, (MaxItems - 1) * ClientSettings.ItemHeight, Item.Bounds.Width, Item.Bounds.Height);
                using (Pen whitePen = new Pen(ClientSettings.LineColor))
                {
                    _RenderedGraphics.DrawLine(whitePen, ItemBounds.Left, ItemBounds.Top, ItemBounds.Right, ItemBounds.Top);
                    _RenderedGraphics.DrawLine(whitePen, ItemBounds.Left, ItemBounds.Bottom, ItemBounds.Right, ItemBounds.Bottom);
                    _RenderedGraphics.DrawLine(whitePen, ItemBounds.Right, ItemBounds.Top, ItemBounds.Right, ItemBounds.Bottom);
                }
                Item.Render(_RenderedGraphics, ItemBounds);
            }
        }
    }
}
