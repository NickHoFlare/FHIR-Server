using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServerExperiment.Controllers.FhirControllers;
using ServerExperiment.Models.POCO;
using ServerExperiment.Models.Repository;

namespace ServerExperimentTests.ResourceTests
{
    [TestClass]
    public class DeviceTest
    {
        private DeviceController deviceController;
        private Mock<IDeviceRepository> mockedRepo;

        [TestInitialize]
        public void TestInitialize()
        {
            mockedRepo = new Mock<IDeviceRepository>();
            deviceController = new DeviceController(mockedRepo.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
        }

        /// <summary>
        /// Tests the Read method to fetch a FHIR resource.
        /// 3 different outcomes:
        /// 1) 200 OK if the device exists and is not deleted
        /// 2) 410 Gone if the device exists and is deleted
        /// 3) 404 Not Found if the device does not exist or some other problem happened.
        /// </summary>
        [TestMethod]
        public void TestRead()
        {
            int[] deviceIDs = {1, 3, 10, 11};

            mockedRepo.Setup(x => x.GetResourceByID(deviceIDs[0])).Returns(new Device() { DeviceId = deviceIDs[0] });
            mockedRepo.Setup(x => x.GetResourceByID(deviceIDs[1])).Returns(new Device() { DeviceId = deviceIDs[1], IsDeleted = true });
            mockedRepo.Setup(x => x.GetResourceByID(deviceIDs[2])).Returns(() =>null);

            mockedRepo.Setup(x => x.GetLatestRecord(deviceIDs[0]))
                .Returns(new DeviceRecord() { DeviceId = deviceIDs[0], Device = new Device(1, false, deviceIDs[1]) });


            // ID is 1 - All OK
            var response1 = deviceController.Read(deviceIDs[0]);
            var response1a = deviceController.Read(deviceIDs[0], "application/xml+FHIR", true);
            var response1b = deviceController.Read(deviceIDs[0], "application/json+FHIR");
            var response1c = deviceController.Read(deviceIDs[0], "application/json+FHIR", true);
            HttpResponseMessage[] responses1 = { response1, response1a, response1b, response1c };

            foreach (var response in responses1)
            {
                Assert.AreEqual(HttpStatusCode.OK, response1.StatusCode);
                Assert.IsNotNull(response1.Content.Headers.LastModified);
                Assert.AreEqual(typeof(StringContent), response1.Content.GetType());
            }

            // ID is 3 - Resource Deleted
            var response2 = deviceController.Read(deviceIDs[1]);
            Assert.AreEqual(HttpStatusCode.Gone, response2.StatusCode);

            // ID is 10 - Device missing
            var response3 = deviceController.Read(deviceIDs[2]);
            Assert.AreEqual(HttpStatusCode.NotFound, response3.StatusCode);

            // ID is 2 - Device does not exist: ID is invalid
            var response4 = deviceController.Read(deviceIDs[3]);
            Assert.AreEqual(HttpStatusCode.NotFound, response4.StatusCode);
        }

        /// <summary>
        /// Tests the Read method to create a FHIR resource.
        /// 2 different outcomes:
        /// 1) 400 Bad Request If ID element is provided in input xml/json
        /// 2) 201 Created if input xml/json is all good, + location header
        /// </summary>
        [TestMethod]
        public void TestCreate()
        {
        }

        /// <summary>
        /// Tests the Read method to update/create a FHIR resource.
        /// 
        /// </summary>
        [TestMethod]
        public void TestUpdate()
        {
        }

        [TestMethod]
        public void TestDelete()
        {
        }
    }
}
