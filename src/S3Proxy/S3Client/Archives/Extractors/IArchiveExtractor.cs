namespace ABSA.RD.S4.S3Proxy.S3Client.Archives.Extractors
{
    /// <summary>
    /// Defines operations for extracting and manipulating various archive related information
    /// </summary>
    interface IArchiveExtractor
    {
        /// <summary>
        /// Extract <seealso cref="ArchiveInfo"/> about archive entries such as local offsets, sizes e.t.c
        /// </summary>
        /// <param name="data">Array of bytes representing the source archive.</param>
        /// <returns></returns>
        ArchiveInfo ExtractInfo(byte[] data);
    }
}