using System;
using System.Collections.Generic;
using System.Text;

namespace PockeTwit.Blog
{
    interface IBlogPostService
    {
        event PostFinishEventHandler PostFinish;
        event ErrorOccuredEventHandler ErrorOccured;
        event MessageReadyEventHandler MessageReady;

        /// <summary>
        /// Return whether service can post gps coördinates
        /// </summary>
        bool CanSentGPS { get; }

        /// <summary>
        /// Return whether service can post pictures
        /// </summary>
        bool CanSendPicture { get; }

        /// <summary>
        /// The maximum length a service can post.
        /// </summary>
        int MaxCharacters { get; }

        /// <summary>
        /// Return whether the service can retrieve it's own timeline.
        /// </summary>
        bool HasTimeLine { get; }

        /// <summary>
        /// The name of the service
        /// </summary>
        string ServiceName { get;  }

        /// <summary>
        /// Post the blog message
        /// </summary>
        /// <param name="blogPostObject"></param>
        void PostBlogMessage(BlogPostObject blogPostObject);

        /// <summary>
        /// Retrieve a timeline.
        /// </summary>
        /// <param name="timelineType">The timeline to fetch</param>
        /// <returns></returns>
        List<Library.status> FetchTimeline(BlogTimeLineFetchType timelineType);

        
        /// <summary>
        /// Search in a timeline.
        /// </summary>
        /// <param name="timelineType">The timeline to search in.</param>
        /// <param name="SearchText">text to search for.</param>
        /// <returns></returns>
        List<Library.status> SearchTimeline(BlogTimeLineFetchType timelineType, string SearchText);

        /// <summary>
        /// Make a post a favorite.
        /// </summary>
        /// <param name="SelectedStatus"></param>
        void SetFavorite(Library.status SelectedStatus);
        
        /// <summary>
        /// Unset a favorite.
        /// </summary>
        /// <param name="SelectedStatus"></param>
        void RemoveFavorite(Library.status SelectedStatus);

        /// <summary>
        /// Follow a user.
        /// </summary>
        /// <param name="UserToFollow"></param>
        //void FollowUser(Library.user UserToFollow);
        
        /// <summary>
        /// Unfollow a user.
        /// </summary>
        /// <param name="UserToStopFollowing"></param>
        //void StopFollowingUser(Library.user UserToStopFollowing);

    }

}
