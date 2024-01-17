using Microsoft.Extensions.Logging;
using Velopack.Sources;

namespace VeloWpfSample;

public class VelopackUpdateSource : SimpleWebSource
{
    public VelopackUpdateSource(
        string teamId = "1",
        string projectId = "VeloWpfSample",
        string channel = "Release", 
        IFileDownloader downloader = null, 
        ILogger logger = null) 
        : base($"http://localhost:5582/api/v1.0/manifest/{teamId}/{projectId}/{channel}", channel, downloader, logger)
    {
    }
}
