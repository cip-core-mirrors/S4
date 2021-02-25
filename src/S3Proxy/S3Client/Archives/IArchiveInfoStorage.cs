namespace ABSA.RD.S4.S3Proxy.S3Client.Archives
{
    /// <summary>
    /// Defines a common operations for storing archive information
    /// </summary>
    interface IArchiveInfoStorage
    {
        /// <summary>
        /// Tries to get desired <seealso cref="ArchiveInfo"/> by reference.
        /// </summary>
        /// <param name="reference">Unique reference to the desired archive info.</param>
        /// <param name="info">Desired <seealso cref="ArchiveInfo"/></param>
        /// <returns></returns>
        bool TryGet(string reference, out ArchiveInfo info);

        /// <summary>
        /// Saves <seealso cref="ArchiveInfo"/> with unique reference.
        /// </summary>
        /// <param name="reference">The reference that uniquely identifies the specified archive information.</param>
        /// <param name="zipInfo">Desired <seealso cref="ArchiveInfo"/></param>
        void Save(string reference, ArchiveInfo zipInfo);
    }
}