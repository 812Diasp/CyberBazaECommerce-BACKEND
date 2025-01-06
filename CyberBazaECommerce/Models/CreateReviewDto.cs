namespace CyberBazaECommerce.Models
{
	public class CreateReviewDto
	{
		public string Title { get; set; }
		public string Pros { get; set; }
		public string Cons { get; set; }
		public string Comment { get; set; }
		public int Rating { get; set; }
	}
}
