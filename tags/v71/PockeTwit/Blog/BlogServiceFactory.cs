using System;

using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace PockeTwit.Blog
{
    class BlogServiceFactory
    {
        #region private properties

        private static volatile BlogServiceFactory _instance;
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
        private BlogServiceFactory()
        {
            SetupServices();
        }

        /// <summary>
        /// Singleton constructor
        /// </summary>
        /// <returns></returns>
        public static BlogServiceFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new BlogServiceFactory();
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
           
            //setup every service the same way
            foreach (IBlogPostService service in serviceList)
            {
               //Set common settings.
            }

        }

        #endregion

        #region public methods

        /// <summary>
        /// retrieve service by name. For getting the configured srvice.
        /// </summary>
        /// <param name="ServiceName">Name of the service</param>
        /// <returns>The service</returns>
        public IBlogPostService GetServiceByName(string ServiceName)
        {
            foreach (IBlogPostService service in serviceList)
            {
                if (service.ServiceName == ServiceName)
                {
                    return service;
                }
            }
            //return first possible service when not found.
            if (serviceList.Count > 0)
            {
                return (IBlogPostService)serviceList[0];
            }
            //return null when no services defined.
            return null;
        }

        /// <summary>
        /// Get the names of the available services.
        /// </summary>
        /// <returns>Collection of services.</returns>
        public ArrayList GetServiceNames()
        {
            ArrayList servicesList = new ArrayList();

            foreach (IBlogPostService service in serviceList)
            {
                servicesList.Add(service.ServiceName);
            }
            return servicesList;
        }

        #endregion

        #region private methods


        #endregion
    }
}
