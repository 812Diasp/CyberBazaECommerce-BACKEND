using System.ComponentModel.DataAnnotations;

namespace CyberBazaECommerce.Models
{
	public class CreateReviewDto
	{
		[Required]
		public string Title { get; set; }
		public string Pros { get; set; }
		public string Cons { get; set; }
		public string Comment { get; set; }
		[Required]
		[Range(1, 5)]
		public int Rating { get; set; }
	}
}
