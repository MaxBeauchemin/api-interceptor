namespace Maxbeauchemin.Api.Interceptor.Utilities;

public static class JsonMatchUtility
{
    public static bool MatchesJson(string path, List<string> values)
    {
        return false;
    }
    
    /// <summary>
    /// converts the string path to a List of commands to perform on the object
    /// 
    /// $.X = X property at root of object
    /// $.X.Y = Y property inside the X object
    /// $[0] = value in the first position of the root array
    /// $[*] = value at any position of the root array
    /// $[1].Z = Z property of object at 2nd position of the root array
    /// $.X[2] = value in the third position of the X array
    /// $[*][3] = value in the fourth position of any positions of the root array
    /// </summary>
    public static PathToken? TokenizePath(string path)
    {
        if (path == null || !path.StartsWith('$')) return null;

        var remainingPath = path.Remove(0, 1);

        PathToken? rootToken = null;
        PathToken? prevToken = null;
        
        while (remainingPath != string.Empty)
        {
            PathToken? currToken = null;
            
            if (remainingPath.StartsWith('.'))
            {
                //Property Token

                var propertyToken = new PropertyToken();
                
                remainingPath = remainingPath.Remove(0, 1);
                
                var nextDotIndex = remainingPath.IndexOf('.');
                var nextOpenBracketIndex = remainingPath.IndexOf('[');
                
                string propertyName;
                
                if (nextDotIndex == -1 && nextOpenBracketIndex == -1)
                {
                    propertyName = remainingPath;
                    remainingPath = string.Empty;
                }
                else
                {
                    int minNextIndex = 0;

                    if (nextDotIndex == -1) minNextIndex = nextOpenBracketIndex;
                    else if (nextOpenBracketIndex == -1) minNextIndex = nextDotIndex;
                    else minNextIndex = Math.Min(nextDotIndex, nextOpenBracketIndex);
                    
                    propertyName = remainingPath.Substring(0, minNextIndex);
                    remainingPath = remainingPath.Remove(0, minNextIndex);
                }
                
                if (propertyName == string.Empty) throw new ArgumentException("Invalid Path - No Property Name after Dot");

                propertyToken.PropertyName = propertyName;
                propertyToken.TokenPath = $".{propertyName}";
                
                currToken = propertyToken;
            }
            else if (remainingPath.StartsWith('['))
            {
                //Array Token
                
                var closeBracketIndex = remainingPath.IndexOf(']');

                if (closeBracketIndex == -1) throw new ArgumentException("Invalid Path - Missing closing bracket");

                var arrayToken = new ArrayToken();
                
                var bracketContents = remainingPath.Substring(1, closeBracketIndex - 1);

                arrayToken.TokenPath = $"[{bracketContents}]";
                
                remainingPath = remainingPath.Remove(0, closeBracketIndex + 1);
                
                if (bracketContents != "*")
                {
                    if (!int.TryParse(bracketContents, out int bracketContentInteger)) throw new ArgumentException("Invalid Path - Invalid bracket content");

                    arrayToken.Position = bracketContentInteger;
                }

                currToken = arrayToken;
            }
            else
            {
                throw new ArgumentException("Invalid Path - Unrecognized Delimiter");
            }

            if (prevToken != null)
            {
                prevToken.ChildToken = currToken;
                currToken.FullPath = prevToken.FullPath + currToken.TokenPath;
            }
            else
            {
                currToken.FullPath = $"${currToken.TokenPath}";
                rootToken = currToken;
            }

            prevToken = currToken;
        }

        return rootToken;
    }

    public class PathToken
    {
        public string FullPath { get; set; }
        public string TokenPath { get; set; }
        public PathToken? ChildToken { get; set; }
    }

    public class PropertyToken : PathToken
    {
        public string PropertyName { get; set; }
    }

    public class ArrayToken : PathToken
    {
        public int? Position { get; set; }
    }
}