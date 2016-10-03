using ServerExperiment.Models.POCO;

namespace ServerExperiment.Models.Repository
{
    public interface IPatientRepository
    {
        IResource GetResourceByID(int resourceId);
        void AddResource(IResource resource);
        void UpdateResource(IResource resource);
        void DeleteResource(IResource resource);
        bool ResourceExists(int resourceId);
        IRecord GetLatestRecord(int resourceId);
        void AddCreateRecord(IResource resource);
        void AddUpdateRecord(IResource resource, IRecord record);
        void AddDeleteRecord(IResource resource, IRecord record);
        void Save();
        void Dispose();
    }
}
