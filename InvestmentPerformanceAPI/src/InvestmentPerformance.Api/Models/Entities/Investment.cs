namespace InvestmentPerformance.Api.Models.Entities;

// A user's holding in one security. Each purchase is a separate lot.
public class Investment
{
	public int Id { get; set; }
	public int UserId { get; set; }
	public int SecurityId { get; set; }
	public Security Security { get; set; } = null!;
	public List<Lot> Lots { get; set; } = [];
}
