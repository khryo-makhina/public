using System.Collections.Generic;

namespace FilenameSanitizer
{
    public interface ISanitizerSetting
    {
        char ReplacementCharacter { get; }
        char[] ExcludedCharacters { get; }
    }
}