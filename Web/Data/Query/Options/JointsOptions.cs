using Web.Models.Exercise;

namespace Web.Data.Query.Options;

public class JointsOptions
{
    public JointsOptions() { }

    public JointsOptions(Joints joints)
    {
        Joints = joints;
    }

    public Joints? Joints { get; set; }
}
