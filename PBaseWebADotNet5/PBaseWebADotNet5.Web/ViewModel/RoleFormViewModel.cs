using System.ComponentModel.DataAnnotations;

namespace PBaseWebADotNet5.Web.ViewModel
{
    public class RoleFormViewModel
    {
        [Required, StringLength(256)]
        public string Name { get; set; }
    }
}
