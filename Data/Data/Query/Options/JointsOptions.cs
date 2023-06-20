using Core.Models.Exercise;

namespace Data.Data.Query.Options;

public class JointsOptions : IOptions
{
    public JointsOptions() { }

    public JointsOptions(Joints joints)
    {
        Joints = joints;
    }

    public Joints? Joints { get; set; }
}
