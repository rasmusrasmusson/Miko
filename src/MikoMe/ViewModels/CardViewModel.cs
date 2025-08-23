namespace MikoMe.ViewModels
{
    public class CardViewModel
    {
        public int Id { get; set; }
        public string Hanzi { get; set; }
        public string Pinyin { get; set; }
        public string English { get; set; }

        // ✅ keep it as string, not CardDirection
        public string Direction { get; set; }

        public DateTime DueAtUtc { get; set; }
    }
}
