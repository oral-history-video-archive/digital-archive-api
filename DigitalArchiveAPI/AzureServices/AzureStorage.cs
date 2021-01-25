using System.Threading.Tasks;
using System.Text.Json;

using Azure.Storage.Blobs;
using DigitalArchiveAPI.Models;
using Azure.Storage.Blobs.Models;
using Azure;

namespace DigitalArchiveAPI.AzureServices
{
    /// <summary>
    /// Encapsulates methods for interacting with Azure Blob Storage.
    /// </summary>
    /// <remarks>
    /// This class has been updated to use the lastest API with help from:
    /// https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-dotnet
    /// </remarks>
    public class AzureStorage
    {
        private readonly BlobServiceClient _blobClient;
        private readonly BlobContainerClient _dataContainer;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// Construct a new instance of the AzureStorage service object.
        /// </summary>
        /// <param name="connectionString">The connection string as given by the Azure portal.</param>
        public AzureStorage(string connectionString)
        {
            // Initialize and hold onto these for improved performance
            _blobClient = new BlobServiceClient(connectionString);
            _dataContainer = _blobClient.GetBlobContainerClient("data");

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Retrieve the JSON containing the details for the given biography.
        /// </summary>
        /// <param name="accession">A unique identifier for the biography.</param>
        /// <returns>A CloudBlockBlob containing a JSON serialized BiographyDetails document.</returns>
        public async Task<BiographyDetails> GetBiographyDetails(string accession)
        {
            if (accession == null)
            {
                return null;
            }
#if SCIENCEMAKERS_ONLY
            BiographyDetails result = await DeserializeBlobAsync<BiographyDetails>($"biography/details/{accession}").ConfigureAwait(false);
            if (result.IsScienceMaker)
                return result;
            else
                return null;
#else
            return await DeserializeBlobAsync<BiographyDetails>($"biography/details/{accession}").ConfigureAwait(false);
#endif
        }

        /// <summary>
        /// Retrieve the JSON document containing the details for the given story.
        /// </summary>
        /// <param name="storyID">The id of the story details document.</param>
        /// <returns>A CloudBlockBlob containing a JSON serialized StoryDetails document.</returns>
        public async Task<StoryDetails> GetStoryDetails(string storyID)
        {
            if (storyID == null)
            {
                return null;
            }

#if SCIENCEMAKERS_ONLY
            StoryDetails result = await DeserializeBlobAsync<StoryDetails>($"story/details/{storyID}").ConfigureAwait(false);
            if (result.IsScienceMaker)
                return result;
            else
                return null;
#else
            return await DeserializeBlobAsync<StoryDetails>($"story/details/{storyID}").ConfigureAwait(false);
#endif
        }

        /// <summary>
        /// Fetch a blob asynchronously and desearilize it as the specified type.
        /// </summary>
        /// <remarks>
        /// Based on code example from:
        /// https://elcamino.cloud/articles/2020-03-30-azure-storage-blobs-net-sdk-v12-upgrade-guide-and-tips.html
        /// </remarks>
        private async Task<T> DeserializeBlobAsync<T>(string blobName)
        {
            try
            {
                var blobClient = _dataContainer.GetBlobClient(blobName);

                using var stream = await blobClient.OpenReadAsync().ConfigureAwait(false);
                return await JsonSerializer.DeserializeAsync<T>(stream, _jsonSerializerOptions).ConfigureAwait(false);

            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                return default;
            }
        }
    }
}
