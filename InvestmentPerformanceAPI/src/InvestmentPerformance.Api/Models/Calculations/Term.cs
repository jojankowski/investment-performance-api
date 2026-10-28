namespace InvestmentPerformance.Api.Models.Calculations;

public enum Term
{
	Short,
	Long,

	/// <summary>Some lots are short term and some are long term.</summary>
	Mixed
}
