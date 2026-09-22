using System.Security.Cryptography;

namespace JayamaliOptical.Web.Services
{
    // Shared by order and booking number generation so both use the same collision-resistant
    // format (a cryptographically random suffix, not the shared-instance-unsafe `new Random()`)
    // and so a future change to the format only needs to happen in one place.
    public static class ReferenceNumberGenerator
    {
        public static string Generate(string prefix) =>
            $"{prefix}-{TimeHelper.Now:yyyyMMddHHmmss}-{RandomNumberGenerator.GetInt32(100000, 999999)}";
    }
}
