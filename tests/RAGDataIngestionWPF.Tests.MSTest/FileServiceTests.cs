using RAGDataIngestionWPF.Core.Services;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class FileServiceTests
{
    private sealed class Payload
    {
        public int Count { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private string _root = string.Empty;

    [TestInitialize]
    public void Initialize()
    {
        _root = Path.Combine(Path.GetTempPath(), "file-service-tests", Guid.NewGuid().ToString("N"));
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }
    }

    [TestMethod]
    public void SaveCreatesDirectoryAndPersistsFile()
    {
        FileService service = new();

        service.Save(_root, "payload.json", new Payload { Name = "alpha", Count = 9 });

        Assert.IsTrue(Directory.Exists(_root));
        Assert.IsTrue(File.Exists(Path.Combine(_root, "payload.json")));
    }

    [TestMethod]
    public void SaveThenReadRoundTripsPayload()
    {
        FileService service = new();
        Payload payload = new() { Name = "beta", Count = 3 };
        service.Save(_root, "payload.json", payload);

        Payload readBack = service.Read<Payload>(_root, "payload.json");

        Assert.IsNotNull(readBack);
        Assert.AreEqual("beta", readBack.Name);
        Assert.AreEqual(3, readBack.Count);
    }

    [TestMethod]
    public void ReadMissingFileReturnsDefaultForReferenceType()
    {
        FileService service = new();

        Payload result = service.Read<Payload>(_root, "missing.json");

        Assert.IsNull(result);
    }

    [TestMethod]
    public void DeleteExistingFileRemovesFile()
    {
        FileService service = new();
        service.Save(_root, "delete-me.json", new Payload { Name = "gamma", Count = 1 });

        service.Delete(_root, "delete-me.json");

        Assert.IsFalse(File.Exists(Path.Combine(_root, "delete-me.json")));
    }

    [TestMethod]
    public void DeleteWithNullFileNameDoesNothing()
    {
        FileService service = new();
        service.Save(_root, "keep.json", new Payload { Name = "delta", Count = 4 });

        service.Delete(_root, null!);

        Assert.IsTrue(File.Exists(Path.Combine(_root, "keep.json")));
    }
}
