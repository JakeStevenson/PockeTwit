using System.Threading;
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
            var waitHandle = new AutoResetEvent(false); 
            var picURL = @"http://pic.gd/ee92b2";
            var service = PictureServiceFactory.Instance.GetServiceByName("TweetPhoto");
            service.DownloadFinish += delegate(object sender, PictureServiceEventArgs args)
                                          {
                                              waitHandle.Set();
                                              Assert.IsNotEmpty(args.PictureFileName);
                                          };
            service.ErrorOccured += delegate(object sender, PictureServiceEventArgs args)
                                        {
                                            waitHandle.Set();
                                            Assert.Fail(args.ErrorMessage);
                                        };

            if(service.CanFetchUrl(picURL))
            service.FetchPicture(picURL);
            if (!waitHandle.WaitOne(100000, false))
            {
                Assert.Fail("Test timed out.");
            }  
        }

        void service_ErrorOccured(object sender, PictureServiceEventArgs eventArgs)
        {
            throw new System.NotImplementedException();
        }

        void service_DownloadFinish(object sender, PictureServiceEventArgs eventArgs)
        {
            throw new System.NotImplementedException();
        }
    }
}
