namespace MikoMe.Models
{
    public class Word
    {
        public int Id { get; set; }
        public string Hanzi { get; set; } = string.Empty;
        public string Pinyin { get; set; } = string.Empty;
        public string English { get; set; } = string.Empty;

        public ICollection<Card> Cards { get; set; } = new List<Card>();

        // Add missing props
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
