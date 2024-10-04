namespace UtilityBills.Aggregates;

/// <summary>
/// Provide access to a security key.
/// </summary>
public interface ISecurityKeyProvider
{
    /// <summary>
    /// Get a security key.
    /// </summary>
    /// <returns></returns>
    byte[] GetKey();

    /// <summary>
    /// Get IV bytes
    /// </summary>
    /// <returns></returns>
    byte[] GetInitVector();
}