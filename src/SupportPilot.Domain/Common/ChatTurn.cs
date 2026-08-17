using SupportPilot.Domain.Enums;

namespace SupportPilot.Domain.Common;

/// <summary>
/// Single turn in a conversation — role + text content.
/// </summary>
public record ChatTurn(ChatRole Role, string Content);