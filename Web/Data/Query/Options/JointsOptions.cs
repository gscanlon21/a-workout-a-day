using Web.Models.Exercise;

namespace Web.Data.Query.Options;

public class JointsOptions : IOptions
{
    public JointsOptions() { }

    public JointsOptions(Joints joints)
    {
        Joints = joints;
    }

    public Joints? Joints { get; set; }
}
