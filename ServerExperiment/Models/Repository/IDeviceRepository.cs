using Hl7.Fhir.Model;
using ServerExperiment.Models.POCO;
using Device = ServerExperiment.Models.POCO.Device;

namespace ServerExperiment.Models.Repository
{
    public interface IDeviceRepository
    {
        IResource GetResourceByID(int resourceId);
        void AddResource(IResource resource);
        void UpdateResource(IResource resource);
        void DeleteResource(IResource resource);
        bool ResourceExists(int resourceId);
        IRecord GetLatestRecord(int resourceId);
        IRecord AddCreateRecord(IResource resource);
        IRecord AddUpdateRecord(IResource resource, IRecord record);
        IRecord AddDeleteRecord(IResource resource, IRecord record);
        Resource AddMetadata(IResource resource, Resource fhirResource, IRecord record);
        void Save();
        void Dispose();
    }
}
