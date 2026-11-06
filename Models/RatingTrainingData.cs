
using Microsoft.ML.Data;

namespace Intalnirea1.Models
{
    public class RatingTrainingData
    {
        public uint ReaderId { get; set; }
        public uint BookId { get; set; }
        public float Label { get; set; }
    }

    public class RatingPrediction
    {
        public float Score { get; set; }
    }
}
