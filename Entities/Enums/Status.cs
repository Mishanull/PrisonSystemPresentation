using System.Runtime.Serialization;

namespace Entities;

public enum Status
{
    [EnumMember(Value = "Waiting")]
    Waiting,
    [EnumMember(Value = "Denied")]
    Denied,
    [EnumMember(Value = "Approved")]
    Approved,
    [EnumMember(Value = "Fulfilled")]
    Fulfilled
}