namespace ABSA.RD.S4.S3Proxy.S3Client.Archives.Extractors
{
    /// <summary>
    /// Defines operations for extracting and manipulating various archive related information
    /// </summary>
    interface IArchiveExtractor
    {
        /// <summary>
        /// Extracts <seealso cref="ArchiveInfo"/> about archive entries such as local offsets, sizes e.t.c
        /// </summary>
        /// <param name="data">Array of bytes representing the source archive.</param>
        /// <returns></returns>
        ArchiveInfo ExtractInfo(byte[] data);

        /// <summary>
        /// Extracts a file from his clipped deflate view. (content without central directory)
        /// </summary>
        /// <param name="data">Clipped deflate view.</param>
        /// <returns>Uncompressed original data.</returns>
        /// <remarks>To extract an original file, we have to create a central directory, append it to the source data and finally restore it from deflate view</remarks>
        byte[] ExtractFile(byte[] data);
    }
}