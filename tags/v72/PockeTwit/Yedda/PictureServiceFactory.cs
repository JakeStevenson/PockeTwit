using System;

using System.Collections.Generic;
using System.Text;
using Yedda;
using System.Collections;

namespace Yedda
{
    class PictureServiceFactory
    {
        #region private properties
        private static volatile PictureServiceFactory _instance;
        private static object syncRoot = new Object();
        /// <summary>
        /// Internal list of services
        /// </summary>
        private ArrayList serviceList;
        
        #endregion

        #region constructor
        /// <summary>
        /// Private constructor for usage in singleton.
        /// </summary>
        private PictureServiceFactory()
        {
            SetupServices();
        }

        /// <summary>
        /// Singleton constructor
        /// </summary>
        /// <returns></returns>
        public static PictureServiceFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new PictureServiceFactory();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region setup

        private void SetupServices()
        {
            serviceList = new ArrayList();
            //Adding services hardcoded, maybe something like reflection could be used?
            serviceList.Add(TwitPic.Instance);
            serviceList.Add(MobyPicture.Instance);
            serviceList.Add(yFrog.Instance);
            serviceList.Add(PikChur.Instance);
            //serviceList.Add(PixIm.Instance);
            serviceList.Add(TwitGoo.Instance);

            //setup every service the same way
            foreach (IPictureService service in serviceList)
            {
                service.ReadBufferSize = 512;
                service.RootPath = ClientSettings.AppPath;
                service.DefaultFileName = "image1.jpg";
                service.DefaultFilePath = "ArtCache";
                service.UseDefaultFileName = true;
                service.UseDefaultFilePath = true;
            }

        }

        #endregion

        #region public methods

        /// <summary>
        /// retrieve service by name. For getting the configured srvice.
        /// </summary>
        /// <param name="ServiceName">Name of the service</param>
        /// <returns>The service</returns>
        public IPictureService GetServiceByName(string ServiceName)
        {
            foreach (IPictureService service in serviceList)
            {
                if (service.ServiceName == ServiceName)
                {
                    return service;
                }
            }
            //return first possible service when not found.
            if (serviceList.Count > 0)
            {
                return (IPictureService) serviceList[0];
            }
            //return null when no services defined.
            return null;
        }

        /// <summary>
        /// Lookup whether a services for downloading a image is available. 
        /// </summary>
        /// <param name="URL">URL to the picture</param>
        /// <returns>True when a service can fetch the image</returns>
        public bool FetchServiceAvailable(string URL)
        {
            foreach (IPictureService service in serviceList)
            {
                if (service.CanFetchUrl(URL))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get the fetch services which can download the picture.
        /// </summary>
        /// <param name="URL">URL to the picture</param>
        /// <returns>The service that can fetch the picture</returns>
        public IPictureService LocateFetchService(string URL)
        {
            foreach (IPictureService service in serviceList)
            {
                if (service.CanFetchUrl(URL))
                {
                    return service;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the names of the available services.
        /// </summary>
        /// <returns>Collection of services.</returns>
        public ArrayList GetServiceNames()
        {
            ArrayList servicesList = new ArrayList();

            foreach (IPictureService service in serviceList)
            {
                if (service.CanUpload)
                {
                    servicesList.Add(service.ServiceName);
                }
            }
            return servicesList;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Set all the event handlers for the chosen picture service.
        /// Aparently after posting, event set are lost so these have to be set again.
        /// </summary>
        /// <param name="pictureService">Picture service on which the event handlers should be set.</param>
        //private void SetPictureEventHandlers(IPictureService service)
        //{
        //    if (!service.HasEventHandlersSet)
        //    {
        //        service.DownloadFinish += new DownloadFinishEventHandler(service_DownloadFinish);
        //        service.ErrorOccured += new ErrorOccuredEventHandler(service_ErrorOccured);
        //    }
        //}
  



        #endregion
    }
}
