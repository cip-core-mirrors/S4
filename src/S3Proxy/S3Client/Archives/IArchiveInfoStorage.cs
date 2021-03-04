namespace ABSA.RD.S4.S3Proxy.S3Client.Archives
{
    interface IArchiveInfoStorage
    {
        bool TryGet(string reference, out ArchiveInfo info);

        void Save(string reference, ArchiveInfo zipInfo);
    }
}