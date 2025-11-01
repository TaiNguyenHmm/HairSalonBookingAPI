using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace WebClient.Pages.ServicesCustomer
{
    [Authorize(Roles = "Customer")]
    public class MyBookingsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
