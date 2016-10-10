using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServerExperiment.Controllers;
using ServerExperiment.Models.POCO;
using ServerExperiment.Models.Repository;

namespace ServerExperimentTests.ResourceTests
{
    [TestClass]
    public class PatientTest
    {
        private PatientController patientController;
        private Mock<IPatientRepository> mockedRepo;
        private Mock<IRecord> mockedRecord;

        [TestInitialize]
        public void TestInitialize()
        {
            mockedRepo = new Mock<IPatientRepository>();
            mockedRecord = new Mock<IRecord>();

            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "SpecificPatient",
                routeTemplate: "fhir/Patient/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost");
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(new HttpRoute());

            patientController = new PatientController(mockedRepo.Object)
            {
                Request = request,
                Configuration = config
            };
        }

        /// <summary>
        /// Tests the Read method to fetch a FHIR resource.
        /// 3 different outcomes:
        /// 1) 200 OK if the Patient exists and is not deleted
        /// 2) 410 Gone if the Patient exists and is deleted
        /// 3) 404 Not Found if the Patient does not exist or some other problem happened.
        /// </summary>
        [TestMethod]
        public void TestRead()
        {
            int[] patientIDs = { 1, 3, 10, 11 };

            mockedRepo.Setup(x => x.GetResourceByID(patientIDs[0])).Returns(new Patient() { PatientId = patientIDs[0] });
            mockedRepo.Setup(x => x.GetResourceByID(patientIDs[1])).Returns(new Patient() { PatientId = patientIDs[1], IsDeleted = true });
            mockedRepo.Setup(x => x.GetResourceByID(patientIDs[2])).Returns(() => null);

            mockedRepo.Setup(x => x.GetLatestRecord(patientIDs[0]))
                .Returns(new PatientRecord() { PatientId = patientIDs[0], Patient = new Patient() { VersionId = 1, IsDeleted = false, PatientId = patientIDs[0] } });

            // ID is 1 - All OK
            var response1 = patientController.Read(patientIDs[0]);
            var response1A = patientController.Read(patientIDs[0], "application/xml+FHIR", true);
            var response1B = patientController.Read(patientIDs[0], "application/json+FHIR");
            var response1C = patientController.Read(patientIDs[0], "application/json+FHIR", true);
            HttpResponseMessage[] responses1 = { response1, response1A, response1B, response1C };

            foreach (var response in responses1)
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.IsNotNull(response.Content.Headers.LastModified);
            }

            // ID is 3 - Resource Deleted
            var response2 = patientController.Read(patientIDs[1]);
            Assert.AreEqual(HttpStatusCode.Gone, response2.StatusCode);

            // ID is 10 - Patient missing
            var response3 = patientController.Read(patientIDs[2]);
            Assert.AreEqual(HttpStatusCode.NotFound, response3.StatusCode);

            // ID is 2 - Patient does not exist: ID is invalid
            var response4 = patientController.Read(patientIDs[3]);
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
            var fhirPatient1 = new Hl7.Fhir.Model.Patient { Id = "1" };
            var fhirPatient2 = new Hl7.Fhir.Model.Patient();
            var fhirDevice = new Hl7.Fhir.Model.Device { Id = "3" };

            mockedRepo.Setup(x => x.AddResource(It.IsAny<Patient>())).Verifiable();
            mockedRepo.Setup(x => x.AddCreateRecord(It.IsAny<Patient>())).Verifiable();
            mockedRepo.Setup(x => x.Save()).Verifiable();

            // Case where ID was provided in input resource
            var response1 = patientController.Create(fhirPatient1);
            Assert.AreEqual(HttpStatusCode.BadRequest, response1.StatusCode);

            // All OK
            // If no PatientId is present in resource object, setting of Header location data will throw exception because no db to increment autonumber index.
            // Added a workaround by causing controller to mock PatientId to 7357 if PatientId is 0 (not set)
            var response2 = patientController.Create(fhirPatient2, "application/xml+FHIR", true);
            Assert.AreEqual(HttpStatusCode.Created, response2.StatusCode);
            Assert.IsTrue(response2.Headers.Location.ToString().EndsWith("/fhir/Patient/7357"));
            mockedRepo.Verify(x => x.AddResource(It.IsAny<Patient>()), Times.Once);
            mockedRepo.Verify(x => x.AddCreateRecord(It.IsAny<Patient>()), Times.Once);
            mockedRepo.Verify(x => x.Save(), Times.Exactly(2));

            // Case where resource type not supported
            var response3 = patientController.Update(fhirDevice, 3);
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
            var fhirPatient1 = new Hl7.Fhir.Model.Patient();
            var fhirPatient2 = new Hl7.Fhir.Model.Patient { Id = "5" };
            var fhirPatient3 = new Hl7.Fhir.Model.Patient { Id = "1" };
            var fhirPatient4 = new Hl7.Fhir.Model.Patient { Id = "2" };
            var fhirDevice = new Hl7.Fhir.Model.Device { Id = "3" };

            mockedRepo.Setup(x => x.Save()).Verifiable();
            mockedRepo.Setup(x => x.AddResource(It.IsAny<Patient>())).Verifiable();
            mockedRepo.Setup(x => x.AddCreateRecord(It.IsAny<Patient>())).Verifiable();
            mockedRepo.Setup(x => x.UpdateResource(It.IsAny<Patient>())).Verifiable();
            mockedRepo.Setup(x => x.AddUpdateRecord(It.IsAny<Patient>(), It.IsAny<IRecord>())).Verifiable();
            mockedRepo.Setup(x => x.ResourceExists(int.Parse(fhirPatient3.Id))).Returns(true);
            mockedRepo.Setup(x => x.ResourceExists(int.Parse(fhirPatient4.Id))).Returns(false);

            // Case where no ID element provided in input xml/json
            var response1 = patientController.Update(fhirPatient1, 1);
            Assert.AreEqual(HttpStatusCode.BadRequest, response1.StatusCode);

            // Case where ID in resource does not match URL ID
            var response2 = patientController.Update(fhirPatient2, 6);
            Assert.AreEqual(HttpStatusCode.BadRequest, response2.StatusCode);

            // Case where resource was updated
            var response3 = patientController.Update(fhirPatient3, 1);
            Assert.AreEqual(HttpStatusCode.OK, response3.StatusCode);
            mockedRepo.Verify(x => x.UpdateResource(It.IsAny<Patient>()), Times.Once);
            mockedRepo.Verify(x => x.AddUpdateRecord(It.IsAny<Patient>(), It.IsAny<IRecord>()), Times.Once);
            mockedRepo.Verify(x => x.Save(), Times.Once);

            // Case where resource was created
            var response4 = patientController.Update(fhirPatient4, 2, "application/xml+FHIR", true);
            Assert.AreEqual(HttpStatusCode.Created, response4.StatusCode);
            Assert.IsTrue(response4.Headers.Location.ToString().EndsWith("/fhir/Patient/7357"));
            mockedRepo.Verify(x => x.AddResource(It.IsAny<Patient>()), Times.Once);
            mockedRepo.Verify(x => x.AddCreateRecord(It.IsAny<Patient>()), Times.Once);
            mockedRepo.Verify(x => x.Save(), Times.Exactly(3));

            // Case where resource type not supported
            var response5 = patientController.Update(fhirDevice, 3);
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
            int[] PatientIDs = { 1, 3, 10 };

            mockedRepo.Setup(x => x.GetResourceByID(PatientIDs[0])).Returns(new Patient() { PatientId = PatientIDs[0] });
            mockedRepo.Setup(x => x.GetResourceByID(PatientIDs[1])).Returns(new Patient() { PatientId = PatientIDs[1], IsDeleted = true });
            mockedRepo.Setup(x => x.GetResourceByID(PatientIDs[2])).Returns(() => null);

            mockedRepo.Setup(x => x.GetLatestRecord(PatientIDs[0]))
                .Returns(new PatientRecord() { PatientId = PatientIDs[0], Patient = new Patient() { VersionId = 1, IsDeleted = false, PatientId = PatientIDs[0] } });

            mockedRepo.Setup(x => x.Save()).Verifiable();
            mockedRepo.Setup(x => x.DeleteResource(It.IsAny<Patient>())).Verifiable();
            mockedRepo.Setup(x => x.AddDeleteRecord(It.IsAny<Patient>(), It.IsAny<IRecord>())).Verifiable();


            // Case where resource is deleted
            var response1 = patientController.Delete(PatientIDs[0]);
            Assert.AreEqual(HttpStatusCode.NoContent, response1.StatusCode);
            mockedRepo.Verify(x => x.Save(), Times.Once);
            mockedRepo.Verify(x => x.DeleteResource(It.IsAny<Patient>()), Times.Once);
            mockedRepo.Verify(x => x.AddDeleteRecord(It.IsAny<Patient>(), It.IsAny<IRecord>()), Times.Once);

            // Case where resource has already been deleted
            var response2 = patientController.Delete(PatientIDs[1]);
            Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);

            // Case where resource does not exist
            var response3 = patientController.Delete(PatientIDs[2]);
            Assert.AreEqual(HttpStatusCode.NoContent, response3.StatusCode);
        }
    }
}
