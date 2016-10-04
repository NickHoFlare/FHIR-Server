using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServerExperiment.Controllers.FhirControllers;
using ServerExperiment.Models.POCO;
using ServerExperiment.Models.Repository;

namespace ServerExperimentTests.ResourceTests
{
    [TestClass]
    public class ObservationTest
    {
        private ObservationController ObservationController;
        private Mock<IObservationRepository> mockedRepo;
        private Mock<IRecord> mockedRecord;

        [TestInitialize]
        public void TestInitialize()
        {
            mockedRepo = new Mock<IObservationRepository>();
            mockedRecord = new Mock<IRecord>();

            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "SpecificObservation",
                routeTemplate: "fhir/observation/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost");
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(new HttpRoute());

            ObservationController = new ObservationController(mockedRepo.Object)
            {
                Request = request,
                Configuration = config
            };
        }

        /// <summary>
        /// Tests the Read method to fetch a FHIR resource.
        /// 3 different outcomes:
        /// 1) 200 OK if the observation exists and is not deleted
        /// 2) 410 Gone if the observation exists and is deleted
        /// 3) 404 Not Found if the observation does not exist or some other problem happened.
        /// </summary>
        [TestMethod]
        public void TestRead()
        {
            int[] observationIDs = { 1, 3, 10, 11 };

            mockedRepo.Setup(x => x.GetResourceByID(observationIDs[0])).Returns(new Observation() { ObservationId = observationIDs[0] });
            mockedRepo.Setup(x => x.GetResourceByID(observationIDs[1])).Returns(new Observation() { ObservationId = observationIDs[1], IsDeleted = true });
            mockedRepo.Setup(x => x.GetResourceByID(observationIDs[2])).Returns(() => null);

            mockedRepo.Setup(x => x.GetLatestRecord(observationIDs[0]))
                .Returns(new ObservationRecord() { ObservationId = observationIDs[0], Observation = new Observation() { VersionId = 1, IsDeleted = false, ObservationId = observationIDs[0] } });

            // ID is 1 - All OK
            var response1 = ObservationController.Read(observationIDs[0]);
            var response1A = ObservationController.Read(observationIDs[0], "application/xml+FHIR", true);
            var response1B = ObservationController.Read(observationIDs[0], "application/json+FHIR");
            var response1C = ObservationController.Read(observationIDs[0], "application/json+FHIR", true);
            HttpResponseMessage[] responses1 = { response1, response1A, response1B, response1C };

            foreach (var response in responses1)
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.IsNotNull(response.Content.Headers.LastModified);
            }

            // ID is 3 - Resource Deleted
            var response2 = ObservationController.Read(observationIDs[1]);
            Assert.AreEqual(HttpStatusCode.Gone, response2.StatusCode);

            // ID is 10 - observation missing
            var response3 = ObservationController.Read(observationIDs[2]);
            Assert.AreEqual(HttpStatusCode.NotFound, response3.StatusCode);

            // ID is 2 - observation does not exist: ID is invalid
            var response4 = ObservationController.Read(observationIDs[3]);
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
            var fhirobservation1 = new Hl7.Fhir.Model.Observation { Id = "1" };
            var fhirobservation2 = new Hl7.Fhir.Model.Observation();
            var fhirPatient = new Hl7.Fhir.Model.Patient { Id = "3" };

            mockedRepo.Setup(x => x.AddResource(It.IsAny<Observation>())).Verifiable();
            mockedRepo.Setup(x => x.AddCreateRecord(It.IsAny<Observation>())).Verifiable();
            mockedRepo.Setup(x => x.Save()).Verifiable();

            // Case where ID was provided in input resource
            var response1 = ObservationController.Create(fhirobservation1);
            Assert.AreEqual(HttpStatusCode.BadRequest, response1.StatusCode);

            // All OK
            // If no observationId is present in resource object, setting of Header location data will throw exception because no db to increment autonumber index.
            // Added a workaround by causing controller to mock observationId to 7357 if observationId is 0 (not set)
            var response2 = ObservationController.Create(fhirobservation2, true);
            Assert.AreEqual(HttpStatusCode.Created, response2.StatusCode);
            Assert.IsTrue(response2.Headers.Location.ToString().EndsWith("/fhir/observation/7357"));
            mockedRepo.Verify(x => x.AddResource(It.IsAny<Observation>()), Times.Once);
            mockedRepo.Verify(x => x.AddCreateRecord(It.IsAny<Observation>()), Times.Once);
            mockedRepo.Verify(x => x.Save(), Times.Exactly(2));

            // Case where resource type not supported
            var response3 = ObservationController.Update(fhirPatient, 3);
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
            var fhirobservation1 = new Hl7.Fhir.Model.Observation();
            var fhirobservation2 = new Hl7.Fhir.Model.Observation { Id = "5" };
            var fhirobservation3 = new Hl7.Fhir.Model.Observation { Id = "1" };
            var fhirobservation4 = new Hl7.Fhir.Model.Observation { Id = "2" };
            var fhirPatient = new Hl7.Fhir.Model.Patient { Id = "3" };

            mockedRepo.Setup(x => x.Save()).Verifiable();
            mockedRepo.Setup(x => x.AddResource(It.IsAny<Observation>())).Verifiable();
            mockedRepo.Setup(x => x.AddCreateRecord(It.IsAny<Observation>())).Verifiable();
            mockedRepo.Setup(x => x.UpdateResource(It.IsAny<Observation>())).Verifiable();
            mockedRepo.Setup(x => x.AddUpdateRecord(It.IsAny<Observation>(), It.IsAny<IRecord>())).Verifiable();
            mockedRepo.Setup(x => x.ResourceExists(int.Parse(fhirobservation3.Id))).Returns(true);
            mockedRepo.Setup(x => x.ResourceExists(int.Parse(fhirobservation4.Id))).Returns(false);

            // Case where no ID element provided in input xml/json
            var response1 = ObservationController.Update(fhirobservation1, 1);
            Assert.AreEqual(HttpStatusCode.BadRequest, response1.StatusCode);

            // Case where ID in resource does not match URL ID
            var response2 = ObservationController.Update(fhirobservation2, 6);
            Assert.AreEqual(HttpStatusCode.BadRequest, response2.StatusCode);

            // Case where resource was updated
            var response3 = ObservationController.Update(fhirobservation3, 1);
            Assert.AreEqual(HttpStatusCode.OK, response3.StatusCode);
            mockedRepo.Verify(x => x.UpdateResource(It.IsAny<Observation>()), Times.Once);
            mockedRepo.Verify(x => x.AddUpdateRecord(It.IsAny<Observation>(), It.IsAny<IRecord>()), Times.Once);
            mockedRepo.Verify(x => x.Save(), Times.Once);

            // Case where resource was created
            var response4 = ObservationController.Update(fhirobservation4, 2, true);
            Assert.AreEqual(HttpStatusCode.Created, response4.StatusCode);
            Assert.IsTrue(response4.Headers.Location.ToString().EndsWith("/fhir/observation/7357"));
            mockedRepo.Verify(x => x.AddResource(It.IsAny<Observation>()), Times.Once);
            mockedRepo.Verify(x => x.AddCreateRecord(It.IsAny<Observation>()), Times.Once);
            mockedRepo.Verify(x => x.Save(), Times.Exactly(3));

            // Case where resource type not supported
            var response5 = ObservationController.Update(fhirPatient, 3);
            Assert.AreEqual(HttpStatusCode.NotFound, response5.StatusCode);
        }

        /// <summary>
        /// Tests the Delete method to delete a FHIR resource.
        /// 3 different outcomes:
        /// 1) 204 No Content if resource is successfully deleted
        /// 1) 204 No Content if resource does not exist
        /// 2) 200 OK if resource has already been deleted
        /// </summary>
        [TestMethod]
        public void TestDelete()
        {
            int[] observationIDs = { 1, 3, 10 };

            mockedRepo.Setup(x => x.GetResourceByID(observationIDs[0])).Returns(new Observation() { ObservationId = observationIDs[0] });
            mockedRepo.Setup(x => x.GetResourceByID(observationIDs[1])).Returns(new Observation() { ObservationId = observationIDs[1], IsDeleted = true });
            mockedRepo.Setup(x => x.GetResourceByID(observationIDs[2])).Returns(() => null);

            mockedRepo.Setup(x => x.GetLatestRecord(observationIDs[0]))
                .Returns(new ObservationRecord() { ObservationId = observationIDs[0], Observation = new Observation() {VersionId = 1, IsDeleted = false, ObservationId = observationIDs[0] }});

            mockedRepo.Setup(x => x.Save()).Verifiable();
            mockedRepo.Setup(x => x.DeleteResource(It.IsAny<Observation>())).Verifiable();
            mockedRepo.Setup(x => x.AddDeleteRecord(It.IsAny<Observation>(), It.IsAny<IRecord>())).Verifiable();


            // Case where resource is deleted
            var response1 = ObservationController.Delete(observationIDs[0]);
            Assert.AreEqual(HttpStatusCode.NoContent, response1.StatusCode);
            mockedRepo.Verify(x => x.Save(), Times.Once);
            mockedRepo.Verify(x => x.DeleteResource(It.IsAny<Observation>()), Times.Once);
            mockedRepo.Verify(x => x.AddDeleteRecord(It.IsAny<Observation>(), It.IsAny<IRecord>()), Times.Once);

            // Case where resource has already been deleted
            var response2 = ObservationController.Delete(observationIDs[1]);
            Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);

            // Case where resource does not exist
            var response3 = ObservationController.Delete(observationIDs[2]);
            Assert.AreEqual(HttpStatusCode.NoContent, response3.StatusCode);
        }
    }
}
