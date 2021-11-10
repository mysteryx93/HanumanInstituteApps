
namespace HanumanInstitute.NaturalGroundingPlayer.DataAccess {
    public class LocalFileInfo {
        public string FileName { get; set; }
        public bool IsInDatabase { get; set; }

        public LocalFileInfo() { }

        public LocalFileInfo(string fileName, bool isInDatabase) {
            this.FileName = fileName;
            this.IsInDatabase = isInDatabase;
        }
    }
}
