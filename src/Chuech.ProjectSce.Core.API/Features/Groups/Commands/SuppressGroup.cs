namespace Chuech.ProjectSce.Core.API.Features.Groups.Commands;

/// <summary>
/// Suppresses the given group.<br/>
/// 
/// <list type="bullet">
///     <listheader>
///         <description><b>Responses</b></description>
///     </listheader>
///     <item>
///         <b><see cref="Success"/></b>: The group exists and has been now been suppressed (if it wasn't already).
///         A <see cref="GroupSuppressed"/> event has been published.
///     </item>
///     <item>
///         <b><see cref="NotFound"/></b>: The group does not exist at all. 
///     </item>
/// </list>
/// </summary>
/// <param name="GroupId">The group id</param>
public record SuppressGroup(int GroupId) : IRespondWith<SuppressGroup.Success, SuppressGroup.NotFound>
{
    public record Success;

    public record NotFound;
}