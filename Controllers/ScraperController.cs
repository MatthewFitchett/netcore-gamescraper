using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using scraper.Extensions;
using scraper.Models;

namespace scraper.Controllers
{
    public class ScraperController : Controller
    {
        private string baseUrl = "http://www.playstationtrophies.org/forum/{0}-ps4-/";
        
        public IActionResult Scrape()
        {
            var urls = new List<string>();
            urls = GetSubForumsPS4TrophiesURLs();
            var response = new List<string>();
            
            // Loop through each sub-forum...            
            foreach(var url in urls)
            {
                // Get the sub-sections list of games and loop through those...
                var gamesListUrls = GetGamesListUrlsForSubSection(url);
              //  foreach (var game in gamesListUrls)
              //  {
                    var rating  = GetTrophyDifficultyRatingForGame(gamesListUrls[0]);
                    response.Add(rating.GameName);
               // }                
            }
            ViewData["Response"] = urls[0] + "....................." +  String.Join(" *** " , response);
            return View();
        }

        /// Scrape the poll results for the difficulty ratings for this game
        /// The link for the poll could be a number of values so we try to capture them all...
        private RatingModel GetTrophyDifficultyRatingForGame(string gamepageUrl)
        {
            var trophyPollLinkText1 = ">Trophy Difficulty Rating</a>";
            var trophyPollLinkText2 = ">Estimated Platinum Difficulty</a>";
            var page = ScrapeSite(gamepageUrl);           
            
            var ratingModel = GetPollDataFromLinkText(trophyPollLinkText1, page);
            if(ratingModel == null)
            {
                Console.WriteLine("**** did not find link for text : " + trophyPollLinkText1 + ". Now trying " + trophyPollLinkText2);
                ratingModel = GetPollDataFromLinkText(trophyPollLinkText2, page);
            }
                
            return ratingModel;
        }

        /// Gets all the data from the page
        /// NOTE: The scraper isn't logged into the site so it gets access to the results immediately
        /// If you navigate to the site logged in then you don't see the same poll page
        private RatingModel GetPollDataFromLinkText(string trophyPollLinkText, string pageHTMLText)
        {
            Console.WriteLine("Now searching for poll text and link..");
            
            var pollLink = GetPollLink(trophyPollLinkText, pageHTMLText);
            if(string.IsNullOrEmpty(pollLink))
                return null;
            
            var pollPage = ScrapeSite(pollLink);
            
            // This gives us the location of the title on the page..
            var pollTable = pollPage.IndexOf("View Poll Results",StringComparison.CurrentCultureIgnoreCase);
            // Let's find the next tr then inspect that for the data we want to extract...
            var trLocation = GetNextTrTag(pollTable, pollPage);
            
            
            
            return new RatingModel(pollLink);
        }

        private int GetNextTrTag(int pollTable, string pollPage)
        {
            throw new NotImplementedException();
        }

        private void Log(string logText)
        {
            Console.WriteLine(logText);
        }

        /// Gets the location of the link to the poll
        private string GetPollLink(string trophyPollLinkText, string pageHTMLText)
        {
            var textLocation = pageHTMLText.IndexOf(trophyPollLinkText);
            if(textLocation == -1)
                return null;
             
            // Get the link from the HTML text by searching backwards for the link start form the text location..
            var linkLocation = pageHTMLText.LastIndexOf("<a href=", textLocation);
            var htmlLocation = pageHTMLText.IndexOf(".html", linkLocation) + 5;
            var pollLink = pageHTMLText.Substring(linkLocation + 9, (htmlLocation - linkLocation) + 9);
            Console.WriteLine("Poll Link : ");
            return pollLink;            
        }

        /// Gets all the list of games and their URLs for sub-section
        private List<string> GetGamesListUrlsForSubSection(string url)
        {
            var result = new List<string>();
            
            // Get the sections markup...
            var markup = ScrapeSite(url);
            // Now parse the HTML grabbing out all the links to the specific games forum...
            // Find the text for the clear.gif and we use this as a starting marker...
            
            const string cleargif = "http://www.playstationtrophies.org/forum/clear.gif";
            var cleargiflocations = markup.AllIndexesOf(cleargif);
            
            
            foreach(var gif in cleargiflocations)
            {
                // Find the next occurence of the link to the subforum...
                var gameForumUrlLocation = markup.IndexOf("http://www.playstationtrophies.org/forum/", gif + cleargif.Length, StringComparison.CurrentCultureIgnoreCase);
                // Once we've got it - then grab the link ..
                var endingChevron = markup.IndexOf(">",gameForumUrlLocation);
                var finalUrl = markup.Substring(gameForumUrlLocation, (endingChevron - gameForumUrlLocation) - 1);
                result.Add(finalUrl);
            }
                   
          return result;
        }


        /// Gets all the URLs for the PS4 sub-sections 
        /// In these sections is the list of games
        private List<string> GetSubForumsPS4TrophiesURLs()
        {
            var result = new List<String>();
            result.Add(string.Format(baseUrl, "a"));            
            return result;
        }

        private string ScrapeSite(string url)
        {
            Console.WriteLine("Now scraping : " + url);
            using(var client = new HttpClient())
            {
                var response = client.GetStringAsync(url);
                return response.Result;
            }
        }
    }
}