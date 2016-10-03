using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServerExperiment.Controllers.FhirControllers;
using ServerExperiment.Models.FHIR.Mappers;
using ServerExperiment.Models.POCO;
using ServerExperiment.Models.Repository;

namespace ServerExperimentTests.ResourceTests
{
    [TestClass]
    public class DeviceTest
    {
        private DeviceController deviceController;
        private Mock<IDeviceRepository> mockedRepo;
        private Mock<IRecord> mockedRecord;

        [TestInitialize]
        public void TestInitialize()
        {
            mockedRepo = new Mock<IDeviceRepository>();
            mockedRecord = new Mock<IRecord>();

            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "SpecificDevice",
                routeTemplate: "fhir/Device/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost");
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(new HttpRoute());

            deviceController = new DeviceController(mockedRepo.Object)
            {
                Request = request,
                Configuration = config
                //Configuration = new HttpConfiguration()
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
            var response1A = deviceController.Read(deviceIDs[0], "application/xml+FHIR", true);
            var response1B = deviceController.Read(deviceIDs[0], "application/json+FHIR");
            var response1C = deviceController.Read(deviceIDs[0], "application/json+FHIR", true);
            HttpResponseMessage[] responses1 = { response1, response1A, response1B, response1C };

            foreach (var response in responses1)
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.IsNotNull(response.Content.Headers.LastModified);
                Assert.AreEqual(typeof(StringContent), response.Content.GetType());
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
        /// Tests the Create method to create a FHIR resource.
        /// 2 different outcomes:
        /// 1) 400 Bad Request If ID element is provided in input xml/json
        /// 2) 201 Created if input xml/json is all good, + location header
        /// 3) 404 Not Found if resource does not exist / resource type not supported
        /// </summary>
        [TestMethod]
        public void TestCreate()
        {
            var fhirDevice1 = new Hl7.Fhir.Model.Device {Id = "1"};
            var fhirDevice2 = new Hl7.Fhir.Model.Device();
            var fhirPatient = new Hl7.Fhir.Model.Patient { Id = "3" };

            mockedRepo.Setup(x => x.AddResource(It.IsAny<Device>())).Verifiable();
            mockedRepo.Setup(x => x.AddCreateRecord(It.IsAny<Device>())).Verifiable();
            mockedRepo.Setup(x => x.Save()).Verifiable();

            // Case where ID was provided in input resource
            var response1 = deviceController.Create(fhirDevice1);
            Assert.AreEqual(HttpStatusCode.BadRequest, response1.StatusCode);

            // All OK
            // If no deviceId is present in resource object, setting of Header location data will throw exception because no db to increment autonumber index.
            // Added a workaround by causing controller to mock deviceId to 7357 if deviceId is 0 (not set)
            var response2 = deviceController.Create(fhirDevice2, true);
            Assert.AreEqual(HttpStatusCode.Created, response2.StatusCode);
            Assert.IsTrue(response2.Headers.Location.ToString().EndsWith("/fhir/Device/7357"));
            mockedRepo.Verify(x => x.AddResource(It.IsAny<Device>()), Times.Once);
            mockedRepo.Verify(x => x.AddCreateRecord(It.IsAny<Device>()), Times.Once);
            mockedRepo.Verify(x => x.Save(), Times.Exactly(2));

            // Case where resource type not supported
            var response3 = deviceController.Update(fhirPatient, 3);
            Assert.AreEqual(HttpStatusCode.NotFound, response3.StatusCode);

        }

        /// <summary>
        /// Tests the Update method to update/create a FHIR resource.
        /// 5 different outcomes:
        /// 1) 400 Bad Request if no ID element is provided in input xml/json
        /// 2) 200 OK if resource was updated
        /// 3) 201 Created if resource was created + location header
        /// 4) 400 Bad Request if resource could not be parsed (Cannot test)
        /// 5) 404 Not Found if resource does not exist / resource type not supported
        /// </summary>
        [TestMethod]
        public void TestUpdate()
        {
            var fhirDevice1 = new Hl7.Fhir.Model.Device();
            var fhirDevice2 = new Hl7.Fhir.Model.Device { Id = "5" };
            var fhirDevice3 = new Hl7.Fhir.Model.Device { Id = "1" };
            var fhirDevice4 = new Hl7.Fhir.Model.Device { Id = "2" };
            var fhirPatient = new Hl7.Fhir.Model.Patient { Id = "3" };

            mockedRepo.Setup(x => x.Save()).Verifiable();
            mockedRepo.Setup(x => x.AddResource(It.IsAny<Device>())).Verifiable();
            mockedRepo.Setup(x => x.AddCreateRecord(It.IsAny<Device>())).Verifiable();
            mockedRepo.Setup(x => x.UpdateResource(It.IsAny<Device>())).Verifiable();
            mockedRepo.Setup(x => x.AddUpdateRecord(It.IsAny<Device>(), It.IsAny<IRecord>())).Verifiable();
            mockedRepo.Setup(x => x.ResourceExists(int.Parse(fhirDevice3.Id))).Returns(true);
            mockedRepo.Setup(x => x.ResourceExists(int.Parse(fhirDevice4.Id))).Returns(false);

            // Case where no ID element provided in input xml/json
            var response1 = deviceController.Update(fhirDevice1, 1);
            Assert.AreEqual(HttpStatusCode.BadRequest, response1.StatusCode);

            // Case where ID in resource does not match URL ID
            var response2 = deviceController.Update(fhirDevice2, 6);
            Assert.AreEqual(HttpStatusCode.BadRequest, response2.StatusCode);

            // Case where resource was updated
            var response3 = deviceController.Update(fhirDevice3, 1);
            Assert.AreEqual(HttpStatusCode.OK, response3.StatusCode);
            mockedRepo.Verify(x => x.UpdateResource(It.IsAny<Device>()), Times.Once);
            mockedRepo.Verify(x => x.AddUpdateRecord(It.IsAny<Device>(), It.IsAny<IRecord>()), Times.Once);
            mockedRepo.Verify(x => x.Save(), Times.Once);

            // Case where resource was created
            var response4 = deviceController.Update(fhirDevice4, 2, true);
            Assert.AreEqual(HttpStatusCode.Created, response4.StatusCode);
            Assert.IsTrue(response4.Headers.Location.ToString().EndsWith("/fhir/Device/7357"));
            mockedRepo.Verify(x => x.AddResource(It.IsAny<Device>()), Times.Once);
            mockedRepo.Verify(x => x.AddCreateRecord(It.IsAny<Device>()), Times.Once);
            mockedRepo.Verify(x => x.Save(), Times.Exactly(3));

            // Case where resource type not supported
            var response5 = deviceController.Update(fhirPatient, 3);
            Assert.AreEqual(HttpStatusCode.NotFound, response5.StatusCode);
        }

        /// <summary>
        /// Tests the Delete method to delete a FHIR resource.
        /// x different outcomes:
        /// 1) 204 No Content if resource is successfully deleted / does not exist
        /// 2) 200 OK if resource has already been deleted
        /// </summary>
        [TestMethod]
        public void TestDelete()
        {
        }
    }
}
