namespace ServerExperiment.Models.POCO
{
    public interface IResource
    {
        int VersionId { get; set; }
        bool IsDeleted { get; set; }
    }
}
