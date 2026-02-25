namespace TextFileSplitterApp;

public interface ITextFileSplitter
{
    Task FormatAsTranslationEntries(List<string> filePaths);
    Task<SplitFileInfo> GetSplittingInformation(string filePath, int maxLinesPerFile);
    Task<int> GetTotalLinesAsync(string filePath);
    Task<SplitFileOutcomeInfo> SplitFileAsync(SplitFileInfo splitFileInfo);
}