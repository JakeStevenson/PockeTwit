using NUnit.Framework;
using PockeTwit.MediaServices;

namespace PockeTwit.TestCode
{
    [TestFixture]
    public class TestMediaServiceFixture
    {
        [Test]
        public void TestMediaService()
        {
            
            var service = PictureServiceFactory.Instance.GetServiceByName("TweetPhoto");
            string testFile = (ClientSettings.AppPath + "\\testcode\\desert.jpg").Substring(6);
            var ppo = new PicturePostObject
                          {
                              Filename = testFile,
                              Message = "Test upload",
                              Username = "pocketwitest",  //REPLACE WITH YOUR OWN TEST ACCOUNT INFO
                              Password = "wms4ftr"
                          };
            Assert.IsTrue(service.PostPictureMessage(ppo));
        }

        [Test]
        public void TestDownloadFromTweetPhoto()
        {
            var picURL = @"http://pic.gd/ee92b2";
            var service = PictureServiceFactory.Instance.GetServiceByName("TweetPhoto");
            if(service.CanFetchUrl(picURL))
            service.FetchPicture(picURL);
            
        }
    }
}
