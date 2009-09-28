using System;

using System.Collections.Generic;
using System.Text;

namespace PockeTwit.Blog
{
    public abstract class BlogServiceBase : IBlogPostService
    {


        #region protected properties for config.

        protected bool _canSendGPS = false;
        protected int _maxCharacters = 140;
        protected bool _canSendPicture = false;
        protected bool _hasTimeLine = false;
        protected string _serviceName = string.Empty;

        #endregion

        #region IBlogPostService Members

        public event PostFinishEventHandler PostFinish;
        public event ErrorOccuredEventHandler ErrorOccured;
        public event MessageReadyEventHandler MessageReady;

        public abstract void PostBlogMessage(BlogPostObject blogPostObject);

        protected abstract void SetupService();

        public List<PockeTwit.Library.status> FetchTimeline(BlogTimeLineFetchType timelineType)
        {
            throw new NotImplementedException();
        }

        public List<PockeTwit.Library.status> SearchTimeline(BlogTimeLineFetchType timelineType, string SearchText)
        {
            throw new NotImplementedException();
        }

        public void SetFavorite(PockeTwit.Library.status SelectedStatus)
        {
            throw new NotImplementedException();
        }

        public void RemoveFavorite(PockeTwit.Library.status SelectedStatus)
        {
            throw new NotImplementedException();
        }

        //public void FollowUser(Library.user UserToFollow)
        //{
        //    throw new NotImplementedException();
        //}

        //public void StopFollowingUser(Library.user UserToStopFollowing)
        //{
        //    throw new NotImplementedException();
        //}

        #region getters and setters

        /// <summary>
        /// Return wheter the service can send GPS coordinates
        /// </summary>
        public bool CanSentGPS
        {
            get { return _canSendGPS; }
        }

        /// <summary>
        /// Return the maximum length of a blog message
        /// </summary>
        public int MaxCharacters
        {
            get { return _maxCharacters; }
        }

        /// <summary>
        /// Return wheter the service can send pictures
        /// </summary>
        public bool CanSendPicture
        {
            get { return _canSendPicture; }
        }

        /// <summary>
        /// Return whether the service has it's own timeline.
        /// </summary>
        public bool HasTimeLine
        {
            get { return _hasTimeLine; }
        }

        /// <summary>
        /// Return the name of the service
        /// </summary>
        public string ServiceName
        {
            get { return _serviceName; }
        }

        #endregion

        #endregion


        #region Event handlers

        protected virtual void OnUploadFinish(BlogServiceEventArgs e)
        {
            if (PostFinish != null)
            {
                try
                {
                    PostFinish(this, e);
                }
                catch (Exception)
                {
                    //Always continue after a missed event
                }
            }
        }

        protected virtual void OnErrorOccured(BlogServiceEventArgs e)
        {
            if (ErrorOccured != null)
            {
                try
                {
                    ErrorOccured(this, e);
                }
                catch (Exception)
                {
                    //Always continue after a missed event
                }
            }
        }

        protected virtual void OnMessageReady(BlogServiceEventArgs e)
        {
            if (MessageReady != null)
            {
                try
                {
                    MessageReady(this, e);
                }
                catch (Exception)
                {
                    //Always continue after a missed event
                }
            }
        }

        #endregion




        #region protected helper methods

        /// <summary>
        /// Create a content part for a string in a form.
        /// </summary>
        /// <param name="header">Identification for a new message part.</param>
        /// <param name="dispositionName">Name of the value.</param>
        /// <param name="valueToSend">Value for sending.</param>
        /// <returns></returns>
        protected string CreateContentPartString(string header, string dispositionName, string valueToSend)
        {
            StringBuilder contents = new StringBuilder();

            contents.Append(header);
            contents.Append("\r\n");
            contents.Append(String.Format("Content-Disposition: form-data;name=\"{0}\"\r\n", dispositionName));
            contents.Append("\r\n");
            contents.Append(valueToSend);
            contents.Append("\r\n");

            return contents.ToString();
        }

        /// <summary>
        /// After putting in a media part. The byte data must be added to stream in implementation.
        /// </summary>
        /// <param name="header">Name of the media part</param>
        /// <param name="dispositionName">Name of the media item.</param>
        /// <param name="contentType">default image/jpeg for images</param> 
        /// <returns></returns>
        protected string CreateContentPartMedia(string header, string dispositionName, string contentType)
        {
            string _contentType = contentType;
            if (string.IsNullOrEmpty(contentType))
            {
                _contentType = "image/jpeg";
            }
            
            StringBuilder contents = new StringBuilder();

            contents.Append(header);
            contents.Append("\r\n");
            contents.Append(string.Format("Content-Disposition:form-data; name=\"{0}\";filename=\"image.jpg\"\r\n", dispositionName));
            contents.Append(string.Format("Content-Type: {0}\r\n", _contentType));
            contents.Append("\r\n");
            
            return contents.ToString();
        }


        #endregion  
    

    }
}
