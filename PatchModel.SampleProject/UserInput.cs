using PatchModel.Attributes;

namespace PatchModel.SampleProject
{
    [PatchesType(typeof(User))]
    public partial record UserInput(string? NickName, int? Age);
}
