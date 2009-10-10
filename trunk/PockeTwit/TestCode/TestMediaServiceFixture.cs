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
                              Username = "someone",  //REPLACE WITH YOUR OWN TEST ACCOUNT INFO
                              Password = "somepass"
                          };
            service.PostPictureMessage(ppo);

        }
    }
}
