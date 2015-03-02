using System;
using PReviewer.Domain;

namespace PReviewer.Model
{
    [Serializable]
    public class FileComment
    {
        public string FileName { get; set; }
        public string Comments { get; set; }
        public ReviewStatus ReviewStatus { get; set; }
    }
}