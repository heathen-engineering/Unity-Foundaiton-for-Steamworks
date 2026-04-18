#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents the result of a file details request made through the Steamworks API.
    /// </summary>
    /// <remarks>
    /// The <c>FileDetailsResult</c> struct is used to encapsulate the detailed information about a file
    /// retrieved from the Steamworks API. It contains file metadata including result status, file size,
    /// hash, and additional flags. This struct can be implicitly converted to and from the corresponding
    /// native <c>FileDetailsResult_t</c> struct used by the Steamworks API.
    /// </remarks>
    public struct FileDetailsResult
    {
        /// <summary>
        /// Represents the raw file details result data encapsulated within the <see cref="FileDetailsResult"/> struct.
        /// This data is retrieved from the Steamworks API and contains detailed information about a specific file,
        /// such as its size, hash, and flags.
        /// </summary>
        public FileDetailsResult_t Data;

        /// <summary>
        /// Represents the result status of the file details operation as provided by the Steamworks API.
        /// This property indicates whether the operation was successful and provides relevant error codes if applicable.
        /// </summary>
        public EResult Result => Data.m_eResult;

        /// <summary>
        /// Represents the size of the file, in bytes, as retrieved from the GetFileDetails response.
        /// This property is part of the data encapsulated within the <see cref="FileDetailsResult"/> struct.
        /// </summary>
        public ulong FileSize => Data.m_ulFileSize;

        /// <summary>
        /// Represents the SHA-1 hash of the file details result within the <see cref="FileDetailsResult"/> struct.
        /// This hash is a 20-byte array providing a unique fingerprint of the file's content,
        /// as returned by the Steamworks API.
        /// </summary>
        public byte[] SHA1Hash => Data.m_FileSHA;

        /// <summary>
        /// Represents the flags associated with the file details, as defined in the <see cref="FileDetailsResult_t"/> struct.
        /// These flags provide specific attributes or properties of the file, retrieved from the Steamworks API.
        /// </summary>
        public uint Flags => Data.m_unFlags;

        public static implicit operator FileDetailsResult(FileDetailsResult_t native) => new FileDetailsResult { Data = native };
        public static implicit operator FileDetailsResult_t(FileDetailsResult heathen) => heathen.Data;
    }
}
#endif