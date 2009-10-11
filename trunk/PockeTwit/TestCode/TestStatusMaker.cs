using System;

using System.Collections.Generic;
using System.Text;

namespace PockeTwit.TestCode
{
    static class TestStatusMaker
    {
        public static Library.status[] GenerateTestStatuses(int NumberOfItems)
        {
            Library.status[] ret = new PockeTwit.Library.status[NumberOfItems];

            for (int i = 0; i < NumberOfItems; i++)
            {
                DateTime t = DateTime.Now.Subtract(new TimeSpan(0, 0, NumberOfItems - i));
                ret[i] = new PockeTwit.Library.status();
                ret[i].createdAt = t;
                ret[i].id = System.Guid.NewGuid().ToString();
                ret[i].text = "Test status" + i + " " + t.ToString() + " " + ret[i].id;
                ret[i].user = new PockeTwit.Library.User();
                ret[i].user.name = "Test user";
                ret[i].user.screen_name = "jakes";
                ret[i].user.profile_image_url = "http://s3.amazonaws.com/twitter_production/profile_images/59702617/avatar_normal.png";
                ret[i].user.needsFetching = false;
            }
            return ret;
        }
    }
}
