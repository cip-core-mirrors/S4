namespace ABSA.RD.S4.S3Proxy.S3Client.Archives.Extractors
{
    interface IArchiveExtractor
    {
        ArchiveInfo ExtractInfo(byte[] data);

        byte[] ExtractFile(byte[] data);
    }
}