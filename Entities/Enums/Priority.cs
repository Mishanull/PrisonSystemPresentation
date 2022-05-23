using System.Runtime.Serialization;

namespace Entities;

public enum Priority
{
    [EnumMember(Value = "Low")]
    Low,
    [EnumMember(Value = "Medium")]
    Medium,
    [EnumMember(Value = "High")]
    High
}