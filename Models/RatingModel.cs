using System.Collections.Generic;

namespace scraper.Models
{
    public class RatingModel 
    {
        /// string gameName
        public RatingModel(string gameName)
        {
            Ratings = new Dictionary<int,int>();
            GameName = gameName;
        }
        
        /// string gameName, Dictionary<int,int> ratings
        public RatingModel(string gameName, Dictionary<int,int> ratings)
        {
            Ratings = ratings;
            GameName = gameName;
        }        
        
        public string GameName {get;private set;}
        
        /// A dictionary representing the games ratings
        /// Key = 1 to 10 rating (where 10 is Very Hard)
        /// Value = Number of votes for this rating
        public Dictionary<int,int> Ratings {get; private set;} 
        
        public int TotalNumberOfVotes {
            get
            {
                var result = 0;
                foreach(var item in Ratings.Values)
                    result = result + item;
                    
                 return result;
            }
        }
    }
}