namespace Maxbeauchemin.Api.Interceptor.DTOs;

public class EndpointBodyProperty
{
    /// <summary>
    /// the JSON path to match on, if this path doesn't exist, will never match
    ///
    /// examples:
    ///     $.X = X property at root of object
    ///     $.X.Y = Y property inside the X object
    ///     $[0] = value in the first position of the root array
    ///     $[*] = value at any position of the root array
    ///     $[1].Z = Z property of object at 2nd position of the root array
    ///     $.X[2] = value in the third position of the X array
    ///     $[*][3] = value in the fourth position of any positions of the root array
    /// </summary>
    public string Path { get; set; }
    
    /// <summary>
    /// the list of values that can be matched for this property
    /// </summary>
    public List<string> Values { get; set; }
}