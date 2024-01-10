using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

public class IndexModel : PageModel
{
    [BindProperty]
    public string NewTodo { get; set; }

    public static List<string> Todos = new List<string>();

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (!string.IsNullOrEmpty(NewTodo))
        {
            Todos.Add(NewTodo);
            NewTodo = string.Empty; // Formu temizle
        }
        return RedirectToPage();
    }

    public JsonResult OnGetTodos()
    {
        return new JsonResult(Todos);
    }
}
