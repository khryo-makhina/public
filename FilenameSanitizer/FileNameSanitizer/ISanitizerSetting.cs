using System.Collections.Generic;

namespace FilenameSanitizer
{
    public interface ISanitizerSetting
    {
        string ReplacementCharacter { get; }
        List<string> ExcludedCharacters { get; }
        bool IsEmpty();
    }
}