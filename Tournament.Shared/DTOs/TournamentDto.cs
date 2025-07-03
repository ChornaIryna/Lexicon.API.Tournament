namespace Tournament.Shared.DTOs;
public record TournamentDto
{
    public required string Title { get; init; }

    private DateTime _startDate;
    public DateTime StartDate
    {
        get => _startDate;
        set
        {
            _startDate = value;
            _endDate = _startDate.AddMonths(3);
        }
    }

    private DateTime _endDate;
    public DateTime EndDate
    {
        get => _endDate;
        set => _endDate = value;
    }
}
