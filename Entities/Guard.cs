using System.Linq.Expressions;

namespace Entities;

public class Guard : User
{
    
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public Guard()
    {
        Role = "guard";
    }

    
}