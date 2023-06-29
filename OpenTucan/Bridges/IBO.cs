namespace OpenTucan.Bridges
{
    public interface IBO
    {
        int Id { get; }

        void Create<T>(T[] data) where T : struct;
        void Update<T>(T[] data) where T : struct;
        void Delete();
    }
}