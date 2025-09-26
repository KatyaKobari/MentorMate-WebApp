using System.Linq;

namespace MentorMate.ViewModels
{
    public class MentorCardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Expertise { get; set; }
        public int Years { get; set; }
        public double Rating { get; set; }
        public string Skills { get; set; }
        public string Bio { get; set; }
        public string Gender { get; set; } = "Male";
        public string AvatarUrl => Gender == "Female" ? 
            "data:image/svg+xml;base64," + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"<svg xmlns='http://www.w3.org/2000/svg' width='100' height='100' viewBox='0 0 100 100'><circle cx='50' cy='50' r='50' fill='#e91e63'/><text x='50' y='60' text-anchor='middle' fill='white' font-size='40' font-family='Arial'>♀</text></svg>")) :
            "data:image/svg+xml;base64," + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"<svg xmlns='http://www.w3.org/2000/svg' width='100' height='100' viewBox='0 0 100 100'><circle cx='50' cy='50' r='50' fill='#2196f3'/><text x='50' y='60' text-anchor='middle' fill='white' font-size='40' font-family='Arial'>♂</text></svg>"));

        public string[] Tags => Skills?.Split(',').Take(3).Select(s => s.Trim()).ToArray() ?? new string[0];
        public bool HasExistingRelationship { get; set; } = false;
    }
}