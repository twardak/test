using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace HackerNewProxy
{
    public class Story
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Uri { get; set; }
        public string PostedBy { get; set; }
        public int Time { get; set; }
        public int Score { get; set; }
        public int CommentCount { get; set; }

    }
}